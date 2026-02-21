using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using GamePlay.Ingame;
using Random = System.Random;

namespace GamePlay.GridMap
{
    public class GridManager : MonoBehaviour
    {
        [SerializeField] private RevealMaskController revealMask;
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Header("Grid")]
        [SerializeField] public int cellSize = 32;

        [SerializeField] private Vector2Int startPosXRange;
        [SerializeField] private Vector2Int startPosYRange;

        [SerializeField] private Vector2Int startFillSizeXRange;
        [SerializeField] private Vector2Int startFillSizeYRange;
        

        public Vector2Int StartPos { get; set; }
        
        [Header("Enemies")]
        [SerializeField] private List<EnemyController> enemies = new List<EnemyController>();

        private Grid _grid;

        private int _cellW;
        private int _cellH;
        private int _nodeW;
        private int _nodeH;

        private int totalCellCount;
        private int capturedCellCount;

        private Vector3 _origin;
        private float _cellWorldSize;

        public int CellWidth => _cellW;
        public int CellHeight => _cellH;
        public int NodeWidth => _nodeW;
        public int NodeHeight => _nodeH;

        public Vector3 Origin => _origin;
        public float CellWorldSize => _cellWorldSize;

        [SerializeField] private InGameManager inGameManager;
        
        private void Awake()
        {
            var sprite = spriteRenderer.sprite;

            _cellW = Mathf.Max(1, Mathf.RoundToInt(sprite.rect.width / cellSize));
            _cellH = Mathf.Max(1, Mathf.RoundToInt(sprite.rect.height / cellSize));

            _nodeW = _cellW + 1;
            _nodeH = _cellH + 1;

            _origin = spriteRenderer.bounds.min;

            var worldW = spriteRenderer.bounds.size.x;
            var worldH = spriteRenderer.bounds.size.y;

            var sx = worldW / _cellW;
            var sy = worldH / _cellH;
            
            _cellWorldSize = (sx + sy) * 0.5f;
            
            var startPosX =  UnityEngine.Random.Range(startPosXRange.x, startPosXRange.y);
            var startPosY =  UnityEngine.Random.Range(startPosYRange.x, startPosYRange.y);
            StartPos = new Vector2Int(startPosX, startPosY);
            
            var startFillSizeX = UnityEngine.Random.Range(startFillSizeXRange.x,  startFillSizeXRange.y);
            var startFillSizeY = UnityEngine.Random.Range(startFillSizeYRange.x, startFillSizeYRange.y);
            var startFillSize = new Vector2Int(startFillSizeX, startFillSizeY);
            
            _grid = new Grid(_cellW, _cellH, StartPos, startFillSize);

            totalCellCount = Mathf.Max(0, (_cellW) * (_cellH));
        }

        private void Start()
        {
            inGameManager.RenewPercentage(CountCapturePercentage());
        }

        public SystemEnum.eSellState GetCell(int x, int y) => _grid.Cells[x, y];

        public Vector2 GetCellCenterWorld(int x, int y) => GridMath.CellCenterToWorld(x, y, _origin, _cellWorldSize);
        public Vector2 GetNodeWorld(int x, int y) => GridMath.NodeToWorld(x, y, _origin, _cellWorldSize);

        public Vector2Int WorldToCell(Vector2 worldPos) => GridMath.WorldToCell(worldPos, _origin, _cellWorldSize);
        public Vector2Int WorldToNode(Vector2 worldPos) => GridMath.WorldToNode(worldPos, _origin, _cellWorldSize);

        public Vector2Int EnemyTransformToNode(EnemyController enemy)
        {
            if (enemy == null) return new Vector2Int(-9999, -9999);
            return WorldToNode(enemy.transform.position);
        }

        public bool IsCellInBounds(Vector2Int c) => c.x >= 0 && c.y >= 0 && c.x < _cellW && c.y < _cellH;
        public bool IsNodeInBounds(Vector2Int n) => n.x >= 0 && n.y >= 0 && n.x < _nodeW && n.y < _nodeH;

        private bool IsFilledCell(Vector2Int c)
        {
            if (!IsCellInBounds(c)) return false;
            return _grid.Cells[c.x, c.y] == SystemEnum.eSellState.Filled;
        }

        private bool IsEmptyCell(Vector2Int c)
        {
            if (!IsCellInBounds(c)) return false;
            return _grid.Cells[c.x, c.y] == SystemEnum.eSellState.Empty;
        }

        private bool IsEmptyOrOutForDraw(Vector2Int c)
        {
            if (!IsCellInBounds(c)) return true; // 드로잉에서는 맵 밖을 "빈 공간"처럼 취급
            return _grid.Cells[c.x, c.y] == SystemEnum.eSellState.Empty;
        }

        // nodePos -> nodePos+dir 엣지에 인접한 셀 2개
        private void GetEdgeSideCells(Vector2Int nodePos, Vector2Int dir, out Vector2Int c1, out Vector2Int c2)
        {
            var a = nodePos;
            var b = nodePos + dir;

            if (a.y == b.y)
            {
                int y = a.y;
                int x = Mathf.Min(a.x, b.x);
                c1 = new Vector2Int(x, y);
                c2 = new Vector2Int(x, y - 1);
                return;
            }

            int vx = a.x;
            int vy = Mathf.Min(a.y, b.y);
            c1 = new Vector2Int(vx, vy);
            c2 = new Vector2Int(vx - 1, vy);
        }

        // "Filled 경계에 닿아있는 노드" 판정 (드로잉 시작/닫힘에 사용)
        // out-of-bounds는 Empty로 취급해서, Filled와 맞닿으면 경계로 인정
        public bool IsCaptureBoundaryNode(Vector2Int node)
        {
            if (!IsNodeInBounds(node)) return false;

            return IsCaptureBoundaryEdge(node, Vector2Int.right)
                || IsCaptureBoundaryEdge(node, Vector2Int.left)
                || IsCaptureBoundaryEdge(node, Vector2Int.up)
                || IsCaptureBoundaryEdge(node, Vector2Int.down);
        }

        private bool IsCaptureBoundaryEdge(Vector2Int nodePos, Vector2Int dir)
        {
            var to = nodePos + dir;
            if (!IsNodeInBounds(to)) return false;

            GetEdgeSideCells(nodePos, dir, out var a, out var b);

            bool sa = IsCellInBounds(a) && IsFilledCell(a);
            bool sb = IsCellInBounds(b) && IsFilledCell(b);

            bool ea = !IsCellInBounds(a) || IsEmptyCell(a);
            bool eb = !IsCellInBounds(b) || IsEmptyCell(b);

            return (sa && eb) || (sb && ea);
        }
        public bool CanMoveBorder(Vector2Int nodePos, Vector2Int dir)
        {
            var to = nodePos + dir;
            if (!IsNodeInBounds(to)) return false;

            GetEdgeSideCells(nodePos, dir, out var a, out var b);

            bool aIn = IsCellInBounds(a);
            bool bIn = IsCellInBounds(b);

            bool aFilled = aIn && _grid.Cells[a.x, a.y] == SystemEnum.eSellState.Filled;
            bool bFilled = bIn && _grid.Cells[b.x, b.y] == SystemEnum.eSellState.Filled;

            // 둘 다 그리드 안이면: 한쪽만 Filled인 경계만 이동 가능
            if (aIn && bIn)
                return aFilled ^ bFilled;

            // 한쪽만 그리드 안이면: 그 "안쪽"이 Filled일 때만(=점령 외곽 타기 금지 유지)
            if (aIn ^ bIn)
                return aIn ? aFilled : bFilled;

            // 둘 다 밖이면 이동 불가
            return false;
        }

        // 드로잉 시작: 맵 밖을 빈 공간처럼 취급해서 외곽에서도 시작 가능
        public bool CanStartDrawEdge(Vector2Int nodePos, Vector2Int dir)
        {
            var to = nodePos + dir;
            if (!IsNodeInBounds(to)) return false;

            GetEdgeSideCells(nodePos, dir, out var a, out var b);

            // 둘 다 맵 밖이면 의미 없는 엣지
            if (!IsCellInBounds(a) && !IsCellInBounds(b)) return false;

            return IsEmptyOrOutForDraw(a) && IsEmptyOrOutForDraw(b);
        }

        // 드로잉 이동: 노드 범위 안이면 OK, 단 "양쪽이 모두 단단(=Filled 또는 맵 밖)"이면 금지
        public bool CanMoveDrawingEdge(Vector2Int nodePos, Vector2Int dir)
        {
            var to = nodePos + dir;
            if (!IsNodeInBounds(to)) return false;

            GetEdgeSideCells(nodePos, dir, out var a, out var b);

            bool sa = IsCellInBounds(a) ? IsFilledCell(a) : true; // 맵 밖 = 단단
            bool sb = IsCellInBounds(b) ? IsFilledCell(b) : true;

            return !(sa && sb);
        }

        public bool IsEdgeBothSidesSolid(Vector2Int nodePos, Vector2Int dir)
        {
            var to = nodePos + dir;
            if (!IsNodeInBounds(to)) return true;

            GetEdgeSideCells(nodePos, dir, out var a, out var b);

            bool sa = IsCellInBounds(a) ? IsFilledCell(a) : true;
            bool sb = IsCellInBounds(b) ? IsFilledCell(b) : true;

            return sa && sb;
        }

        private bool IsBlockedBetweenCells(int x1, int y1, int x2, int y2, HashSet<Edge> lineEdges)
        {
            if (x2 == x1 + 1 && y2 == y1)
                return lineEdges.Contains(new Edge(new Vector2Int(x1 + 1, y1), new Vector2Int(x1 + 1, y1 + 1)));

            if (x2 == x1 - 1 && y2 == y1)
                return lineEdges.Contains(new Edge(new Vector2Int(x1, y1), new Vector2Int(x1, y1 + 1)));

            if (y2 == y1 + 1 && x2 == x1)
                return lineEdges.Contains(new Edge(new Vector2Int(x1, y1 + 1), new Vector2Int(x1 + 1, y1 + 1)));

            if (y2 == y1 - 1 && x2 == x1)
                return lineEdges.Contains(new Edge(new Vector2Int(x1, y1), new Vector2Int(x1 + 1, y1)));

            return false;
        }

        private void TryFloodStep(
            int x1, int y1,
            int x2, int y2,
            HashSet<Edge> lineEdges,
            bool[,] visited,
            Queue<Vector2Int> q)
        {
            if (x2 < 0 || y2 < 0 || x2 >= _cellW || y2 >= _cellH) return;
            if (visited[x2, y2]) return;
            if (_grid.Cells[x2, y2] != SystemEnum.eSellState.Empty) return;
            if (IsBlockedBetweenCells(x1, y1, x2, y2, lineEdges)) return;

            visited[x2, y2] = true;
            q.Enqueue(new Vector2Int(x2, y2));
        }

        private void EnqueueEnemySeedCells(Queue<Vector2Int> q, bool[,] visited)
        {
            void TrySeed(Vector2Int c)
            {
                if (!IsCellInBounds(c)) return;
                if (_grid.Cells[c.x, c.y] != SystemEnum.eSellState.Empty) return;
                if (visited[c.x, c.y]) return;

                visited[c.x, c.y] = true;
                q.Enqueue(c);
            }

            void SeedFromNode(Vector2Int n)
            {
                TrySeed(new Vector2Int(n.x, n.y));
                TrySeed(new Vector2Int(n.x - 1, n.y));
                TrySeed(new Vector2Int(n.x, n.y - 1));
                TrySeed(new Vector2Int(n.x - 1, n.y - 1));
            }

            if (enemies == null) return;

            for (int i = 0; i < enemies.Count; i++)
            {
                var e = enemies[i];
                if (e == null) continue;

                var nodes = e.CurrentNode;
                if (nodes != null && nodes.Count > 0)
                {
                    for (int k = 0; k < nodes.Count; k++)
                        SeedFromNode(nodes[k]);
                }
                else
                {
                    SeedFromNode(EnemyTransformToNode(e));
                }
            }
        }
        private void GetAdjacentCellsToEdge(Edge e, out Vector2Int c1, out Vector2Int c2)
        {
            var a = e.A;
            var b = e.B;

            if (a.y == b.y)
            {
                int y = a.y;
                int x = Mathf.Min(a.x, b.x);
                c1 = new Vector2Int(x, y);
                c2 = new Vector2Int(x, y - 1);
                return;
            }

            int vx = a.x;
            int vy = Mathf.Min(a.y, b.y);
            c1 = new Vector2Int(vx, vy);
            c2 = new Vector2Int(vx - 1, vy);
        }
        private void MarkEnemyComponentsByLabel(bool[] compHasEnemy, int[,] label)
        {
            void MarkCell(Vector2Int c)
            {
                if (!IsCellInBounds(c)) return;
                if (_grid.Cells[c.x, c.y] != SystemEnum.eSellState.Empty) return;

                int id = label[c.x, c.y];
                if (id >= 0 && id < compHasEnemy.Length)
                    compHasEnemy[id] = true;
            }

            void MarkByNode(Vector2Int n)
            {
                MarkCell(new Vector2Int(n.x, n.y));
                MarkCell(new Vector2Int(n.x - 1, n.y));
                MarkCell(new Vector2Int(n.x, n.y - 1));
                MarkCell(new Vector2Int(n.x - 1, n.y - 1));
            }

            if (enemies == null) return;

            for (int i = 0; i < enemies.Count; i++)
            {
                var e = enemies[i];
                if (e == null) continue;

                var nodes = e.CurrentNode;
                if (nodes != null && nodes.Count > 0)
                {
                    for (int k = 0; k < nodes.Count; k++)
                        MarkByNode(nodes[k]);
                }
                else
                {
                    MarkByNode(EnemyTransformToNode(e));
                }
            }
        }

        public void CaptureByLine(HashSet<Edge> lineEdges)
        {
            int w = _cellW;
            int h = _cellH;

            // 1) lineEdges를 "벽"으로 보고 Empty 컴포넌트 라벨링
            var label = new int[w, h];
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                label[x, y] = -1;

            var sizes = new List<int>(64);
            var q = new Queue<Vector2Int>(1024);

            int FloodComp(int sx, int sy, int id)
            {
                int size = 0;
                label[sx, sy] = id;
                q.Enqueue(new Vector2Int(sx, sy));

                while (q.Count > 0)
                {
                    var c = q.Dequeue();
                    size++;

                    TryEnq(c.x, c.y, c.x + 1, c.y);
                    TryEnq(c.x, c.y, c.x - 1, c.y);
                    TryEnq(c.x, c.y, c.x, c.y + 1);
                    TryEnq(c.x, c.y, c.x, c.y - 1);
                }

                return size;

                void TryEnq(int x1, int y1, int x2, int y2)
                {
                    if (x2 < 0 || y2 < 0 || x2 >= w || y2 >= h) return;
                    if (label[x2, y2] != -1) return;
                    if (_grid.Cells[x2, y2] != SystemEnum.eSellState.Empty) return;
                    if (IsBlockedBetweenCells(x1, y1, x2, y2, lineEdges)) return;

                    label[x2, y2] = id;
                    q.Enqueue(new Vector2Int(x2, y2));
                }
            }

            int compId = 0;
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                if (_grid.Cells[x, y] != SystemEnum.eSellState.Empty) continue;
                if (label[x, y] != -1) continue;

                sizes.Add(FloodComp(x, y, compId));
                compId++;
            }

            if (compId == 0) return;

            // 2) 적이 속한 컴포넌트 마킹
            var compHasEnemy = new bool[compId];
            MarkEnemyComponentsByLabel(compHasEnemy, label);

            // 3) "이번에 그린 선"에 인접한 컴포넌트만 후보로 모음
            var adjacent = new HashSet<int>();

            void AddAdjCell(Vector2Int c)
            {
                if (!IsCellInBounds(c)) return;
                if (_grid.Cells[c.x, c.y] != SystemEnum.eSellState.Empty) return;

                int id = label[c.x, c.y];
                if (id >= 0) adjacent.Add(id);
            }

            foreach (var e in lineEdges)
            {
                GetAdjacentCellsToEdge(e, out var c1, out var c2);
                AddAdjCell(c1);
                AddAdjCell(c2);
            }

            if (adjacent.Count == 0) return;

            // 4) adjacent 중 "적 없는 컴포넌트"만 점령 대상으로.
            //    여러 개면 가장 작은 것 1개만 점령 (원치 않는 '전부 점령' 방지)
            int target = -1;
            int best = int.MaxValue;

            foreach (var id in adjacent)
            {
                if (id < 0 || id >= compId) continue;
                if (compHasEnemy[id]) continue;

                int s = sizes[id];
                if (s < best)
                {
                    best = s;
                    target = id;
                }
            }

            // adjacent 양쪽 다 적이 있으면 점령 안 함
            if (target == -1) return;

            // 5) target 컴포넌트만 Filled로 변경 + 마스크 반영
            var changed = new List<Vector2Int>(best);

            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                if (label[x, y] == target)
                {
                    _grid.Cells[x, y] = SystemEnum.eSellState.Filled;
                    changed.Add(new Vector2Int(x, y));
                }
            }

            if (changed.Count > 0)
            {
                revealMask?.RevealCells(changed);
                
                
                // 증가량 측정
                var increasedPercentage = CountCapturePercentage() - currentPercentage;
                // 최종적으로 현재 점령도 적용
                currentPercentage = CountCapturePercentage();
                
                // 현재 점령도 
                inGameManager.RenewPercentage(currentPercentage);
                inGameManager.IncreaseScore(increasedPercentage);
                
                
            }

        }

        private int currentPercentage = 0;
        
        [ContextMenu("점령도 계산")]
        public int CountCapturePercentage()
        {
            var count = _grid.Cells.Cast<SystemEnum.eSellState>().Count(cell => cell == SystemEnum.eSellState.Filled); 
            
            Debug.Log($"점령된 셀 개수 : {count}, 총 셀 개수 : {totalCellCount}"); 
            Debug.Log($"{(count * 100)/totalCellCount}%"); 
            
            return (count * 100) / totalCellCount;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            if (enemies == null) return;

            foreach (var enemy in enemies)
            {
                if (enemy == null || enemy.CurrentNode == null) continue;

                foreach (var node in enemy.CurrentNode)
                {
                    var p2 = GridMath.NodeToWorld(node.x, node.y, _origin, _cellWorldSize);
                    Gizmos.DrawSphere(new Vector3(p2.x, p2.y, 0f), _cellWorldSize * 0.15f);
                }
            }
        }
    }
}
