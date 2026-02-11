using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GamePlay.GridMap;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GamePlay.Player
{
    public enum TraverseMode
    {
        Border,
        Drawing,
        Returning
    }

    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float speed = 1f;
        [SerializeField] private float moveDelay = 0.2f;
        [SerializeField] private GridManager gridManager;

        [Header("Draw Outline")]
        [SerializeField] private bool useLineRenderer = true;
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private float lineWidth = 0.03f;
        [SerializeField] private int lineSortingOrder = 50;

        [SerializeField] private Vector2Int currentNode;

        private PlayerInput _playerInput;
        private InputAction _moveAction;
        private InputAction _occupyAction;

        [SerializeField] private bool occupying;
        private Vector2Int _moveDir;

        private bool _canMove = true;
        private TraverseMode _mode = TraverseMode.Border;

        private readonly HashSet<Edge> _lineEdges = new HashSet<Edge>();
        private readonly HashSet<Vector2Int> _lineNodes = new HashSet<Vector2Int>();
        private readonly Stack<Vector2Int> _drawStack = new Stack<Vector2Int>();
        private readonly List<Vector3> _drawPoints = new List<Vector3>(256);

        private Vector2Int _drawStartNode;
        private bool _returnRequested;

        private CancellationTokenSource _returnCts;

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
            _moveAction = _playerInput.actions["Move"];
            _occupyAction = _playerInput.actions["Occupy"];

            if (useLineRenderer && lineRenderer == null)
            {
                var go = new GameObject("DrawLine");
                go.transform.SetParent(transform, false);
                lineRenderer = go.AddComponent<LineRenderer>();
                lineRenderer.useWorldSpace = true;
                lineRenderer.positionCount = 0;
                lineRenderer.startWidth = lineWidth;
                lineRenderer.endWidth = lineWidth;
                lineRenderer.sortingOrder = lineSortingOrder;
            }
        }

        private void Start()
        {
            currentNode = gridManager.startPos;
            var p2 = gridManager.GetNodeWorld(currentNode.x, currentNode.y);
            transform.position = new Vector3(p2.x, p2.y, 0f);
        }

        private void OnEnable()
        {
            _occupyAction.started += OnOccupyPressed;
            _occupyAction.canceled += OnOccupyReleased;
        }

        private void OnDisable()
        {
            _occupyAction.started -= OnOccupyPressed;
            _occupyAction.canceled -= OnOccupyReleased;
        }

        private void Update()
        {
            if (_mode == TraverseMode.Returning) return;

            RefreshMove();

            if (_mode == TraverseMode.Drawing && _returnRequested && _canMove && _moveDir == Vector2Int.zero)
            {
                ReturnToStart().Forget();
                return;
            }

            if (_canMove && _moveDir != Vector2Int.zero)
                MoveApply().Forget();
        }

        private static int AxisToInt(float v)
        {
            if (v > 0.5f) return 1;
            if (v < -0.5f) return -1;
            return 0;
        }

        private void RefreshMove()
        {
            var raw = _moveAction.ReadValue<Vector2>();
            _moveDir = new Vector2Int(AxisToInt(raw.x), AxisToInt(raw.y));
        }

        private async UniTask MoveApply()
        {
            if (_mode == TraverseMode.Returning) return;
            if (!_canMove) return;

            _canMove = false;

            int x = _moveDir.x;
            int y = _moveDir.y;

            if (y != 0)
            {
                if (await TryMoveCandidate(new Vector2Int(0, y), Vector2Int.zero)) { _canMove = true; return; }
                if (await TryMoveCandidate(new Vector2Int(0, y), new Vector2Int(1, 0))) { _canMove = true; return; }
                if (await TryMoveCandidate(new Vector2Int(0, y), new Vector2Int(-1, 0))) { _canMove = true; return; }
            }

            if (x != 0)
            {
                if (await TryMoveCandidate(new Vector2Int(x, 0), Vector2Int.zero)) { _canMove = true; return; }
                if (await TryMoveCandidate(new Vector2Int(x, 0), new Vector2Int(0, 1))) { _canMove = true; return; }
                if (await TryMoveCandidate(new Vector2Int(x, 0), new Vector2Int(0, -1))) { _canMove = true; return; }
            }

            _canMove = true;
        }

        private bool ContainsNodeSim(Vector2Int node, Vector2Int[] extraNodes, int extraCount)
        {
            if (_lineNodes.Contains(node)) return true;
            for (int i = 0; i < extraCount; i++)
                if (extraNodes[i] == node) return true;
            return false;
        }

        private bool ContainsEdgeSim(Edge e, Edge[] extraEdges, int extraCount)
        {
            if (_lineEdges.Contains(e)) return true;
            for (int i = 0; i < extraCount; i++)
                if (extraEdges[i].Equals(e)) return true;
            return false;
        }

        private bool CanMoveDrawingStepSim(
            Vector2Int from,
            Vector2Int step,
            Vector2Int drawStart,
            Vector2Int[] extraNodes,
            int extraNodeCount,
            Edge[] extraEdges,
            int extraEdgeCount,
            out bool willClose)
        {
            willClose = false;

            var to = from + step;
            if (!gridManager.IsNodeInBounds(to)) return false;

            if (!gridManager.CanMoveDrawingEdge(from, step)) return false;
            if (gridManager.IsEdgeBothSidesSolid(from, step)) return false;

            var e = new Edge(from, to);
            if (ContainsEdgeSim(e, extraEdges, extraEdgeCount)) return false;

            if (ContainsNodeSim(to, extraNodes, extraNodeCount))
            {
                if (to == drawStart && gridManager.IsCaptureBoundaryNode(to))
                {
                    willClose = true;
                    return true;
                }
                return false;
            }

            if (gridManager.IsCaptureBoundaryNode(to))
            {
                willClose = true;
                return true;
            }

            return true;
        }

        private async UniTask<bool> TryMoveCandidate(Vector2Int direction, Vector2Int correction)
        {
            int stepCount = correction == Vector2Int.zero ? 1 : 2;
            Vector2Int step0 = correction;
            Vector2Int step1 = direction;

            var simMode = _mode;
            var simPos = currentNode;

            int drawStartIndex = -1;
            int closeIndex = -1;

            var simDrawStart = _drawStartNode;

            var extraNodes = new Vector2Int[3];
            var extraEdges = new Edge[2];
            int extraNodeCount = 0;
            int extraEdgeCount = 0;

            for (int i = 0; i < stepCount; i++)
            {
                var step = (stepCount == 1) ? step1 : (i == 0 ? step0 : step1);
                var next = simPos + step;

                if (!gridManager.IsNodeInBounds(next))
                    return false;

                if (simMode == TraverseMode.Border)
                {
                    if (gridManager.CanMoveBorder(simPos, step))
                    {
                        simPos = next;
                        continue;
                    }

                    bool canStart = occupying
                                    && gridManager.IsCaptureBoundaryNode(simPos)
                                    && gridManager.CanStartDrawEdge(simPos, step);

                    if (!canStart) return false;

                    // 보정으로 그리기 시작 금지(옆으로 한칸 튀는 현상 제거)
                    if (correction != Vector2Int.zero) return false;

                    simMode = TraverseMode.Drawing;
                    simDrawStart = simPos;
                    drawStartIndex = i;

                    extraNodeCount = 0;
                    extraEdgeCount = 0;
                    extraNodes[extraNodeCount++] = simPos;
                }

                if (simMode == TraverseMode.Drawing)
                {
                    if (!CanMoveDrawingStepSim(simPos, step, simDrawStart, extraNodes, extraNodeCount, extraEdges, extraEdgeCount, out bool willClose))
                        return false;

                    var to = simPos + step;

                    if (willClose)
                    {
                        closeIndex = i;
                        simPos = to;
                        break;
                    }

                    extraEdges[extraEdgeCount++] = new Edge(simPos, to);
                    extraNodes[extraNodeCount++] = to;
                    simPos = to;
                }
            }

            int execCount = closeIndex >= 0 ? closeIndex + 1 : stepCount;
            float duration = (moveDelay * speed) / execCount;

            var execPos = currentNode;

            for (int i = 0; i < execCount; i++)
            {
                var step = (stepCount == 1) ? step1 : (i == 0 ? step0 : step1);

                if (_mode == TraverseMode.Border && drawStartIndex == i)
                {
                    BeginDrawing(execPos);
                    _mode = TraverseMode.Drawing;
                }

                var next = execPos + step;

                await MoveToNode(next, duration, CancellationToken.None);

                if (_mode == TraverseMode.Drawing)
                {
                    var e = new Edge(execPos, next);
                    _lineEdges.Add(e);
                    _lineNodes.Add(next);
                    _drawStack.Push(next);
                    AddDrawPoint(next);
                }

                currentNode = next;
                execPos = next;

                if (_mode == TraverseMode.Drawing && _returnRequested)
                {
                    await ReturnToStart();
                    return true;
                }

                if (_mode == TraverseMode.Drawing && closeIndex == i)
                {
                    gridManager.CaptureByLine(_lineEdges);
                    EndDrawingToBorder();
                    return true;
                }
            }

            return true;
        }

        private void BeginDrawing(Vector2Int start)
        {
            _returnRequested = false;

            _lineEdges.Clear();
            _lineNodes.Clear();
            _drawStack.Clear();

            _drawStartNode = start;

            _lineNodes.Add(start);
            _drawStack.Push(start);

            ClearDrawLine();
            AddDrawPoint(start);
        }

        private void EndDrawingToBorder()
        {
            _returnRequested = false;

            _lineEdges.Clear();
            _lineNodes.Clear();
            _drawStack.Clear();

            _mode = TraverseMode.Border;

            DisposeReturnCts();
            ClearDrawLine();
        }

        private void DisposeReturnCts()
        {
            _returnCts?.Cancel();
            _returnCts?.Dispose();
            _returnCts = null;
        }

        private async UniTask ReturnToStart()
        {
            if (_mode != TraverseMode.Drawing) return;

            _mode = TraverseMode.Returning;
            _returnRequested = false;

            DisposeReturnCts();
            _returnCts = new CancellationTokenSource();
            var token = _returnCts.Token;

            float duration = moveDelay * speed;

            while (_drawStack.Count > 1)
            {
                var from = _drawStack.Pop();
                var to = _drawStack.Peek();

                bool canceled = await MoveToNode(to, duration, token);

                _lineEdges.Remove(new Edge(from, to));
                _lineNodes.Remove(from);

                currentNode = to;
                RemoveLastDrawPoint();

                if (canceled)
                {
                    _mode = TraverseMode.Drawing;
                    DisposeReturnCts();
                    return;
                }
            }

            EndDrawingToBorder();
        }

        private async UniTask<bool> MoveToNode(Vector2Int to, float duration, CancellationToken token)
        {
            var start = transform.position;
            var end2 = gridManager.GetNodeWorld(to.x, to.y);
            var end = new Vector3(end2.x, end2.y, 0f);

            float t = 0f;
            while (t < 1f)
            {
                if (token.IsCancellationRequested)
                {
                    transform.position = end;
                    return true;
                }

                t += Time.deltaTime / duration;
                transform.position = Vector3.Lerp(start, end, t);
                await UniTask.Yield();
            }

            transform.position = end;
            return false;
        }

        private void OnOccupyPressed(InputAction.CallbackContext context)
        {
            occupying = true;

            if (_mode == TraverseMode.Returning)
                _returnCts?.Cancel();
        }

        private void OnOccupyReleased(InputAction.CallbackContext context)
        {
            occupying = false;

            if (_mode == TraverseMode.Drawing)
                _returnRequested = true;
        }

        private void ClearDrawLine()
        {
            _drawPoints.Clear();
            if (!useLineRenderer || lineRenderer == null) return;
            lineRenderer.positionCount = 0;
        }

        private void AddDrawPoint(Vector2Int node)
        {
            var p2 = gridManager.GetNodeWorld(node.x, node.y);
            var p = new Vector3(p2.x, p2.y, 0f);

            _drawPoints.Add(p);

            if (!useLineRenderer || lineRenderer == null) return;

            int idx = _drawPoints.Count - 1;
            lineRenderer.positionCount = _drawPoints.Count;
            lineRenderer.SetPosition(idx, p);
        }

        private void RemoveLastDrawPoint()
        {
            if (_drawPoints.Count == 0) return;

            _drawPoints.RemoveAt(_drawPoints.Count - 1);

            if (!useLineRenderer || lineRenderer == null) return;

            lineRenderer.positionCount = _drawPoints.Count;
        }
    }
}
