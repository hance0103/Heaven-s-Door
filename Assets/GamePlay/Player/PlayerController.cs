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

        [SerializeField] private Vector2Int currentPos;

        [Header("Draw Line")]
        [SerializeField] private LineRenderer drawLineRenderer;
        [SerializeField] private float drawLineZ = 0f;

        private PlayerInput _playerInput;
        private InputAction _moveAction;
        private InputAction _occupyAction;

        [SerializeField] private bool occupying;
        private Vector2Int _moveDirection;

        private bool _canMove = true;
        private TraverseMode _mode = TraverseMode.Border;

        private readonly HashSet<Edge> _lineEdges = new HashSet<Edge>();
        private readonly HashSet<Vector2Int> _lineNodes = new HashSet<Vector2Int>();
        private readonly Stack<Vector2Int> _drawStack = new Stack<Vector2Int>();

        private readonly List<Vector2Int> _drawPath = new List<Vector2Int>();

        private Vector2Int _drawStartNode;
        private bool _returnRequested;

        private CancellationTokenSource _returnCts;

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
            _moveAction = _playerInput.actions["Move"];
            _occupyAction = _playerInput.actions["Occupy"];
        }

        private void Start()
        {
            InitPlayerPosition(gridManager.startPos.x - 1, gridManager.startPos.y - 1);
            SetLineVisible(false);
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

            RefreshMoveFromAction();

            if (_mode == TraverseMode.Drawing && _returnRequested && _canMove && _moveDirection == Vector2Int.zero)
            {
                ReturnToStart().Forget();
                return;
            }

            if (_canMove && _moveDirection != Vector2Int.zero)
            {
                MoveApply().Forget();
            }
        }

        private static int AxisToInt(float v)
        {
            if (v > 0.5f) return 1;
            if (v < -0.5f) return -1;
            return 0;
        }

        private void RefreshMoveFromAction()
        {
            var raw = _moveAction.ReadValue<Vector2>();
            _moveDirection = new Vector2Int(AxisToInt(raw.x), AxisToInt(raw.y));
        }

        private async UniTask MoveApply()
        {
            if (_mode == TraverseMode.Returning) return;
            if (!_canMove) return;

            _canMove = false;

            var x = _moveDirection.x;
            var y = _moveDirection.y;

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
            for (var i = 0; i < extraCount; i++)
            {
                if (extraNodes[i] == node) return true;
            }
            return false;
        }

        private bool ContainsEdgeSim(Edge e, Edge[] extraEdges, int extraCount)
        {
            if (_lineEdges.Contains(e)) return true;
            for (var i = 0; i < extraCount; i++)
            {
                if (extraEdges[i].Equals(e)) return true;
            }
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

            if (gridManager.IsEdgeBothSidesHardSolid(from, step)) return false;

            var e = new Edge(from, to);
            if (ContainsEdgeSim(e, extraEdges, extraEdgeCount)) return false;

            if (ContainsNodeSim(to, extraNodes, extraNodeCount))
            {
                if (to == drawStart && gridManager.IsBorderNode(to))
                {
                    willClose = true;
                    return true;
                }
                return false;
            }

            if (gridManager.IsBorderNode(to))
            {
                willClose = true;
                return true;
            }

            return true;
        }

        private async UniTask<bool> TryMoveCandidate(Vector2Int direction, Vector2Int correction)
        {
            if (_mode == TraverseMode.Returning) return false;

            var stepCount = correction == Vector2Int.zero ? 1 : 2;
            var step0 = correction;
            var step1 = direction;

            var simMode = _mode;
            var simPos = currentPos;

            var drawStartIndex = -1;
            var closeIndex = -1;

            var simDrawStart = _drawStartNode;

            var extraNodes = new Vector2Int[3];
            var extraEdges = new Edge[2];
            var extraNodeCount = 0;
            var extraEdgeCount = 0;

            for (var i = 0; i < stepCount; i++)
            {
                var step = (stepCount == 1) ? step1 : (i == 0 ? step0 : step1);

                if (simMode == TraverseMode.Border)
                {
                    if (gridManager.CanMoveBorder(simPos, step))
                    {
                        simPos += step;
                        continue;
                    }

                    var canStart = occupying
                                   && gridManager.IsBorderNode(simPos)
                                   && gridManager.IsBothSidesPureEmpty(simPos, step);

                    if (!canStart) return false;

                    simMode = TraverseMode.Drawing;
                    simDrawStart = simPos;
                    drawStartIndex = i;

                    extraNodeCount = 0;
                    extraEdgeCount = 0;

                    extraNodes[extraNodeCount++] = simPos;
                }

                if (simMode == TraverseMode.Drawing)
                {
                    if (!CanMoveDrawingStepSim(simPos, step, simDrawStart, extraNodes, extraNodeCount, extraEdges, extraEdgeCount, out var willClose))
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

            var execCount = closeIndex >= 0 ? closeIndex + 1 : stepCount;
            var duration = (moveDelay * speed) / execCount;

            var execPos = currentPos;

            for (var i = 0; i < execCount; i++)
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
                    _lineEdges.Add(new Edge(execPos, next));
                    _lineNodes.Add(next);
                    _drawStack.Push(next);

                    _drawPath.Add(next);
                    UpdateDrawLine();
                }

                currentPos = next;
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

        private void BeginDrawing(Vector2Int startNode)
        {
            _returnRequested = false;

            _lineEdges.Clear();
            _lineNodes.Clear();
            _drawStack.Clear();

            _drawPath.Clear();

            _drawStartNode = startNode;

            _lineNodes.Add(startNode);
            _drawStack.Push(startNode);

            _drawPath.Add(startNode);
            SetLineVisible(true);
            UpdateDrawLine();
        }

        private void EndDrawingToBorder()
        {
            _returnRequested = false;

            _lineEdges.Clear();
            _lineNodes.Clear();
            _drawStack.Clear();

            _drawPath.Clear();
            SetLineVisible(false);

            _mode = TraverseMode.Border;

            DisposeReturnCts();
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

            var duration = moveDelay * speed;

            while (_drawStack.Count > 1)
            {
                var from = _drawStack.Pop();
                var to = _drawStack.Peek();

                var canceled = await MoveToNode(to, duration, token);

                _lineEdges.Remove(new Edge(from, to));
                _lineNodes.Remove(from);

                if (_drawPath.Count > 0)
                {
                    _drawPath.RemoveAt(_drawPath.Count - 1);
                    UpdateDrawLine();
                }

                currentPos = to;

                if (canceled)
                {
                    _mode = TraverseMode.Drawing;
                    DisposeReturnCts();
                    return;
                }
            }

            EndDrawingToBorder();
        }

        private async UniTask<bool> MoveToNode(Vector2Int toNode, float duration, CancellationToken token)
        {
            var start = transform.position;

            var dest2D = gridManager.GetNodePosition(toNode.x, toNode.y);
            var end = new Vector3(dest2D.x, dest2D.y, drawLineZ);

            var t = 0f;
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

        private void InitPlayerPosition(int x, int y)
        {
            var pos = gridManager.GetNodePosition(x, y);
            transform.position = new Vector3(pos.x, pos.y, drawLineZ);
            currentPos = new Vector2Int(x, y);
        }

        private void OnOccupyPressed(InputAction.CallbackContext context)
        {
            occupying = true;

            if (_mode == TraverseMode.Returning)
            {
                _returnCts?.Cancel();
            }
        }

        private void OnOccupyReleased(InputAction.CallbackContext context)
        {
            occupying = false;

            if (_mode == TraverseMode.Drawing)
            {
                _returnRequested = true;
            }
        }

        private void SetLineVisible(bool visible)
        {
            if (drawLineRenderer == null) return;
            drawLineRenderer.enabled = visible;
            if (!visible) drawLineRenderer.positionCount = 0;
        }

        private void UpdateDrawLine()
        {
            if (drawLineRenderer == null) return;

            if (_drawPath.Count < 2)
            {
                drawLineRenderer.positionCount = 0;
                return;
            }

            drawLineRenderer.positionCount = _drawPath.Count;

            for (int i = 0; i < _drawPath.Count; i++)
            {
                var n = _drawPath[i];
                var p2 = gridManager.GetNodePosition(n.x, n.y);
                drawLineRenderer.SetPosition(i, new Vector3(p2.x, p2.y, drawLineZ));
            }
        }
    }
}
