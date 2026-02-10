using System;
using System.Collections.Generic;
using UnityEngine;
using GamePlay;

namespace GamePlay.GridMap
{
    public class GridManager : MonoBehaviour
    {
        [SerializeField] private RevealMaskController revealMask;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] public int cellSize = 32;
        [SerializeField] public Vector2Int startPos = new Vector2Int(1, 1);
        [SerializeField] private bool drawGizmos = true;

        private Vector2Int _gridSize;
        private Vector2Int _nodeSize;
        private Vector3 _mapOrigin;
        
        private Grid _grid;

        public Vector2Int GridSize => _gridSize;
        public Vector2Int NodeSize => _nodeSize;
        
        public int GridWidth => _grid.Width;
        public int GridHeight => _grid.Height;
        public Vector3 MapOrigin => _mapOrigin;
        public float CellWorldSize => cellSize / 100f;
        

        private void Awake()
        {
            var sprite = spriteRenderer.sprite;

            var gridWidth = (int)sprite.rect.width / cellSize + 2;
            var gridHeight = (int)sprite.rect.height / cellSize + 2;

            _gridSize = new Vector2Int(gridWidth, gridHeight);
            _nodeSize = new Vector2Int(gridWidth - 1, gridHeight - 1);

            _grid = new Grid(gridWidth, gridHeight, startPos);

            _mapOrigin = spriteRenderer.bounds.min - Vector3.one * cellSize / 200f;
        }

        
        public SystemEnum.eSellState GetCell(int x, int y)
        {
            return _grid.Cells[x, y];
        }
        
        public Vector2 GetGridPosition(int x, int y)
        {
            return GridMath.GridToWorld(x, y, _mapOrigin, cellSize);
        }

        public Vector2 GetNodePosition(int x, int y)
        {
            return GridMath.NodeToWorld(x, y, _mapOrigin, cellSize);
        }

        public bool IsNodeInBounds(Vector2Int n)
        {
            return n.x >= 0 && n.y >= 0 && n.x < _nodeSize.x && n.y < _nodeSize.y;
        }

        private static bool IsCaptured(SystemEnum.eSellState s)
        {
            return s == SystemEnum.eSellState.Filled;
        }

        private static bool IsHardSolid(SystemEnum.eSellState s)
        {
            return s == SystemEnum.eSellState.Filled || s == SystemEnum.eSellState.Wall;
        }

        private void GetEdgeSideCells(Vector2Int node, Vector2Int dir, out Vector2Int c1, out Vector2Int c2)
        {
            var x = node.x;
            var y = node.y;

            if (dir.y > 0)
            {
                c1 = new Vector2Int(x, y + 1);
                c2 = new Vector2Int(x + 1, y + 1);
                return;
            }

            if (dir.y < 0)
            {
                c1 = new Vector2Int(x, y);
                c2 = new Vector2Int(x + 1, y);
                return;
            }

            if (dir.x > 0)
            {
                c1 = new Vector2Int(x + 1, y);
                c2 = new Vector2Int(x + 1, y + 1);
                return;
            }

            c1 = new Vector2Int(x, y);
            c2 = new Vector2Int(x, y + 1);
        }

        public bool CanStartDrawEdge(Vector2Int nodePos, Vector2Int dir)
        {
            GetEdgeSideCells(nodePos, dir, out var ca, out var cb);
            var a = _grid.Cells[ca.x, ca.y];
            var b = _grid.Cells[cb.x, cb.y];

            // 드로잉은 “점령(Filled)과 겹치지 않는” 엣지로만 시작
            return a != SystemEnum.eSellState.Filled && b != SystemEnum.eSellState.Filled;
        }
        
        public bool CanMoveBorder(Vector2Int nodePos, Vector2Int dir)
        {
            if (!IsNodeInBounds(nodePos + dir)) return false;

            GetEdgeSideCells(nodePos, dir, out var ca, out var cb);
            var a = _grid.Cells[ca.x, ca.y];
            var b = _grid.Cells[cb.x, cb.y];

            // “정확히 한쪽만 점령(Filled)”이면 경계 이동 가능
            return IsCaptured(a) ^ IsCaptured(b);
        }
        public bool IsBorderNode(Vector2Int node)
        {
            if (!IsNodeInBounds(node)) return false;
            return _grid.Nodes[node.x, node.y] == SystemEnum.eNodeState.Moveable;
        }

        public bool IsBothSidesPureEmpty(Vector2Int nodePos, Vector2Int dir)
        {
            GetEdgeSideCells(nodePos, dir, out var ca, out var cb);
            return _grid.Cells[ca.x, ca.y] == SystemEnum.eSellState.Empty
                   && _grid.Cells[cb.x, cb.y] == SystemEnum.eSellState.Empty;
        }

        public bool IsEdgeBothSidesHardSolid(Vector2Int nodePos, Vector2Int dir)
        {
            GetEdgeSideCells(nodePos, dir, out var ca, out var cb);
            var a = _grid.Cells[ca.x, ca.y];
            var b = _grid.Cells[cb.x, cb.y];
            return IsHardSolid(a) && IsHardSolid(b);
        }
        private bool IsBlockedBetweenCells(int x1, int y1, int x2, int y2, HashSet<Edge> lineEdges)
        {
            // 셀 (x1,y1) <-> (x2,y2)는 4방 인접만 들어온다고 가정

            // 오른쪽 (x+1)
            if (x2 == x1 + 1 && y2 == y1)
            {
                // 두 셀 사이의 경계는 "수직 엣지" (노드 (x1, y1-1) -> (x1, y1))
                var n1 = new Vector2Int(x1, y1 - 1);
                var n2 = new Vector2Int(x1, y1);
                return lineEdges.Contains(new Edge(n1, n2));
            }

            // 왼쪽 (x-1)
            if (x2 == x1 - 1 && y2 == y1)
            {
                var n1 = new Vector2Int(x2, y1 - 1);
                var n2 = new Vector2Int(x2, y1);
                return lineEdges.Contains(new Edge(n1, n2));
            }

            // 위쪽 (y+1)
            if (y2 == y1 + 1 && x2 == x1)
            {
                // 두 셀 사이의 경계는 "수평 엣지" (노드 (x1-1, y1) -> (x1, y1))
                var n1 = new Vector2Int(x1 - 1, y1);
                var n2 = new Vector2Int(x1, y1);
                return lineEdges.Contains(new Edge(n1, n2));
            }

            // 아래쪽 (y-1)
            if (y2 == y1 - 1 && x2 == x1)
            {
                var n1 = new Vector2Int(x1 - 1, y2);
                var n2 = new Vector2Int(x1, y2);
                return lineEdges.Contains(new Edge(n1, n2));
            }

            return false;
        }
        public void CaptureByLine(HashSet<Edge> lineEdges)
        {
            var w = _grid.Width;
            var h = _grid.Height;

            var label = new int[w, h];
            for (var x = 0; x < w; x++)
            {
                for (var y = 0; y < h; y++)
                {
                    label[x, y] = -1;
                }
            }

            var sizes = new List<int>();
            var q = new Queue<Vector2Int>();

            int MakeComponent(int sx, int sy, int id)
            {
                var size = 0;
                label[sx, sy] = id;
                q.Enqueue(new Vector2Int(sx, sy));

                while (q.Count > 0)
                {
                    var c = q.Dequeue();
                    size++;

                    var x = c.x;
                    var y = c.y;

                    TryEnqueue(x, y, x + 1, y, id);
                    TryEnqueue(x, y, x - 1, y, id);
                    TryEnqueue(x, y, x, y + 1, id);
                    TryEnqueue(x, y, x, y - 1, id);
                }

                return size;
            }

            void TryEnqueue(int x1, int y1, int x2, int y2, int id)
            {
                if (x2 < 0 || y2 < 0 || x2 >= w || y2 >= h) return;
                if (label[x2, y2] != -1) return;
                if (_grid.Cells[x2, y2] != SystemEnum.eSellState.Empty) return;

                if (IsBlockedBetweenCells(x1, y1, x2, y2, lineEdges)) return;

                label[x2, y2] = id;
                q.Enqueue(new Vector2Int(x2, y2));
            }

            var compId = 0;
            for (var x = 0; x < w; x++)
            {
                for (var y = 0; y < h; y++)
                {
                    if (_grid.Cells[x, y] != SystemEnum.eSellState.Empty) continue;
                    if (label[x, y] != -1) continue;

                    var size = MakeComponent(x, y, compId);
                    sizes.Add(size);
                    compId++;
                }
            }

            // 선 주변(양쪽)을 정확히 컴포넌트로 수집: GetEdgeSideCells를 양방향으로 사용
            var adjacentComps = new HashSet<int>();

            void AddAdjCell(Vector2Int c)
            {
                if (c.x < 0 || c.y < 0 || c.x >= w || c.y >= h) return;
                if (_grid.Cells[c.x, c.y] != SystemEnum.eSellState.Empty) return;

                var id = label[c.x, c.y];
                if (id != -1) adjacentComps.Add(id);
            }

            foreach (var e in lineEdges)
            {
                var a = e.A;
                var b = e.B;
                var dir = b - a; // (1,0), (-1,0), (0,1), (0,-1)

                // a 기준으로 dir 쪽(2셀) + 반대쪽(2셀) => 총 4셀을 모두 확인
                GetEdgeSideCells(a, dir, out var s1, out var s2);
                GetEdgeSideCells(a, -dir, out var t1, out var t2);

                AddAdjCell(s1);
                AddAdjCell(s2);
                AddAdjCell(t1);
                AddAdjCell(t2);
            }

            if (adjacentComps.Count == 0)
            {
                _grid.RecalculateNodes();
                return;
            }
            
            //TODO 적이 없는 방향을 캡처하도록 나중에 추가할것
            // 작은 쪽 캡처(적/퀵스 없을 때 임시 룰)
            var target = -1;
            var minSize = int.MaxValue;

            foreach (var id in adjacentComps)
            {
                var s = sizes[id];
                if (s < minSize)
                {
                    minSize = s;
                    target = id;
                }
            }

            
            var totalEmpty = 0;
            for (var i = 0; i < sizes.Count; i++)
                totalEmpty += sizes[i];
            
            if (target == -1)
            {
                _grid.RecalculateNodes();
                return;
            }
            
            // 안전장치: 선이 실제로 영역을 나누지 못해서 Empty가 하나뿐이면 캡처하지 않음
            if (sizes[target] == totalEmpty)
            {
                _grid.RecalculateNodes();
                return;
            }

            var changed = new List<Vector2Int>();
            
            for (var x = 0; x < w; x++)
            {
                for (var y = 0; y < h; y++)
                {
                    if (label[x, y] == target)
                    {
                        _grid.Cells[x, y] = SystemEnum.eSellState.Filled;
                        changed.Add(new Vector2Int(x, y));
                    }
                }
            }
            
            revealMask?.RevealCells(changed);
            _grid.RecalculateNodes();
        }

        private void TryStepCell(
            int x1, int y1,
            int x2, int y2,
            bool[,] vBlock, bool[,] hBlock,
            bool[,] visited,
            Queue<Vector2Int> q)
        {
            if (x2 < 0 || y2 < 0 || x2 >= visited.GetLength(0) || y2 >= visited.GetLength(1)) return;
            if (visited[x2, y2]) return;
            if (_grid.Cells[x2, y2] != SystemEnum.eSellState.Empty) return;

            if (x2 == x1 + 1)
            {
                if (vBlock[x1, y1]) return;
            }
            else if (x2 == x1 - 1)
            {
                if (vBlock[x2, y1]) return;
            }
            else if (y2 == y1 + 1)
            {
                if (hBlock[x1, y1]) return;
            }
            else if (y2 == y1 - 1)
            {
                if (hBlock[x1, y2]) return;
            }

            visited[x2, y2] = true;
            q.Enqueue(new Vector2Int(x2, y2));
        }

        private void OnDrawGizmos()
        {
            if (!drawGizmos) return;
            if (_grid == null) return;

            for (var x = 0; x < _gridSize.x; x++)
            {
                for (var y = 0; y < _gridSize.y; y++)
                {
                    var pos = GetGridPosition(x, y);

                    switch (_grid.Cells[x, y])
                    {
                        case SystemEnum.eSellState.Empty:
                            Gizmos.color = Color.clear;
                            break;
                        case SystemEnum.eSellState.Wall:
                            Gizmos.color = new Color(1, 0, 0, 0.15f);
                            break;
                        case SystemEnum.eSellState.Filled:
                            Gizmos.color = new Color(0, 0, 1, 0.15f);
                            break;
                        default:
                            Gizmos.color = Color.magenta;
                            break;
                    }

                    Gizmos.DrawCube(pos, Vector3.one * cellSize / 100f);
                }
            }

            for (var x = 0; x < _nodeSize.x; x++)
            {
                for (var y = 0; y < _nodeSize.y; y++)
                {
                    var p = GetNodePosition(x, y);

                    switch (_grid.Nodes[x, y])
                    {
                        case SystemEnum.eNodeState.CannotMove:
                            Gizmos.color = Color.red;
                            break;
                        case SystemEnum.eNodeState.Moveable:
                            Gizmos.color = Color.green;
                            break;
                        case SystemEnum.eNodeState.Drawable:
                            Gizmos.color = Color.yellow;
                            break;
                        default:
                            Gizmos.color = Color.magenta;
                            break;
                    }

                    Gizmos.DrawSphere(p, 0.02f);
                }
            }
        }
    }
}
