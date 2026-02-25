using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using GamePlay.Enemy;
using GamePlay.Ingame;
using Managers;
using Random = System.Random;

namespace GamePlay.GridMap
{
    public class GridManager : MonoBehaviour
    {
        [SerializeField] private RevealMaskController revealMask;
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Header("점령구역 경계선")] 
        [SerializeField] private Transform boundaryColliderRoot;
        private readonly List<GameObject> boundaryColliders = new List<GameObject>();
        
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
        
        private void Awake()
        {
            GameManager.Instance.gridManager = this;
            
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
            GameManager.Instance.inGameManager.RenewPercentage(CountCapturePercentage());
            RebuildCapturedBoundaryEdgeColliders();
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

            var vx = a.x;
            var vy = Mathf.Min(a.y, b.y);
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

            var sa = IsCellInBounds(a) && IsFilledCell(a);
            var sb = IsCellInBounds(b) && IsFilledCell(b);

            var ea = !IsCellInBounds(a) || IsEmptyCell(a);
            var eb = !IsCellInBounds(b) || IsEmptyCell(b);

            return (sa && eb) || (sb && ea);
        }
        public bool CanMoveBorder(Vector2Int nodePos, Vector2Int dir)
        {
            var to = nodePos + dir;
            if (!IsNodeInBounds(to)) return false;

            GetEdgeSideCells(nodePos, dir, out var a, out var b);

            var aIn = IsCellInBounds(a);
            var bIn = IsCellInBounds(b);

            var aFilled = aIn && _grid.Cells[a.x, a.y] == SystemEnum.eSellState.Filled;
            var bFilled = bIn && _grid.Cells[b.x, b.y] == SystemEnum.eSellState.Filled;

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

            var sa = !IsCellInBounds(a) || IsFilledCell(a); // 맵 밖 = 단단
            var sb = !IsCellInBounds(b) || IsFilledCell(b);

            return !(sa && sb);
        }

        public bool IsEdgeBothSidesSolid(Vector2Int nodePos, Vector2Int dir)
        {
            var to = nodePos + dir;
            if (!IsNodeInBounds(to)) return true;

            GetEdgeSideCells(nodePos, dir, out var a, out var b);

            var sa = !IsCellInBounds(a) || IsFilledCell(a);
            var sb = !IsCellInBounds(b) || IsFilledCell(b);

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
            void SeedFromNode(Vector2Int n)
            {
                TrySeed(new Vector2Int(n.x, n.y));
                TrySeed(new Vector2Int(n.x - 1, n.y));
                TrySeed(new Vector2Int(n.x, n.y - 1));
                TrySeed(new Vector2Int(n.x - 1, n.y - 1));
            }

            if (enemies == null) return;

            foreach (var e in enemies)
            {
                if (e == null) continue;
                
                SeedFromNode(e.CurrentNode);
            }

            return;

            void TrySeed(Vector2Int c)
            {
                if (!IsCellInBounds(c)) return;
                if (_grid.Cells[c.x, c.y] != SystemEnum.eSellState.Empty) return;
                if (visited[c.x, c.y]) return;

                visited[c.x, c.y] = true;
                q.Enqueue(c);
            }
        }
        private void GetAdjacentCellsToEdge(Edge e, out Vector2Int c1, out Vector2Int c2)
        {
            var a = e.A;
            var b = e.B;

            if (a.y == b.y)
            {
                var y = a.y;
                var x = Mathf.Min(a.x, b.x);
                c1 = new Vector2Int(x, y);
                c2 = new Vector2Int(x, y - 1);
                return;
            }

            var vx = a.x;
            var vy = Mathf.Min(a.y, b.y);
            c1 = new Vector2Int(vx, vy);
            c2 = new Vector2Int(vx - 1, vy);
        }
        private void MarkEnemyComponentsByLabel(bool[] compHasEnemy, int[,] label)
        {
            void MarkByNode(Vector2Int n)
            {
                MarkCell(new Vector2Int(n.x, n.y));
                MarkCell(new Vector2Int(n.x - 1, n.y));
                MarkCell(new Vector2Int(n.x, n.y - 1));
                MarkCell(new Vector2Int(n.x - 1, n.y - 1));
            }

            if (enemies == null) return;

            foreach (var e in enemies.Where(e => e != null))
            {
                MarkByNode(e.CurrentNode);
            }

            return;

            void MarkCell(Vector2Int c)
            {
                if (!IsCellInBounds(c)) return;
                if (_grid.Cells[c.x, c.y] != SystemEnum.eSellState.Empty) return;

                var id = label[c.x, c.y];
                if (id >= 0 && id < compHasEnemy.Length)
                    compHasEnemy[id] = true;
            }
        }

        public void CaptureByLine(HashSet<Edge> lineEdges)
        {
            var w = _cellW;
            var h = _cellH;

            // 1) lineEdges를 "벽"으로 보고 Empty 컴포넌트 라벨링
            var label = new int[w, h];
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                label[x, y] = -1;

            var sizes = new List<int>(64);
            var q = new Queue<Vector2Int>(1024);

            var compId = 0;
            for (var x = 0; x < w; x++)
            for (var y = 0; y < h; y++)
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

                var id = label[c.x, c.y];
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
            var target = -1;
            var best = int.MaxValue;

            foreach (var id in adjacent)
            {
                if (id < 0 || id >= compId) continue;
                if (compHasEnemy[id]) continue;

                var s = sizes[id];
                if (s >= best) continue;
                best = s;
                target = id;
            }

            // adjacent 양쪽 다 적이 있으면 점령 안 함
            if (target == -1) return;

            // 5) target 컴포넌트만 Filled로 변경 + 마스크 반영
            var changed = new List<Vector2Int>(best);

            for (var x = 0; x < w; x++)
            for (var y = 0; y < h; y++)
            {
                if (label[x, y] != target) continue;
                _grid.Cells[x, y] = SystemEnum.eSellState.Filled;
                changed.Add(new Vector2Int(x, y));
            }

            if (changed.Count <= 0) return;
            
            revealMask?.RevealCells(changed);
                
            var prevPercentage = currentPercentage;
            currentPercentage = CountCapturePercentage();
                
            var delta = currentPercentage - prevPercentage;
            GameManager.Instance.inGameManager.OnCapture(currentPercentage, delta);
            return;

            int FloodComp(int sx, int sy, int id)
            {
                var size = 0;
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
        }

        #region Boundary

        public HashSet<Edge> GetCapturedBoundaryEdges()
        {
            var edges = new HashSet<Edge>();

            for (int x = 0; x < _cellW; x++)
            for (int y = 0; y < _cellH; y++)
            {
                if (_grid.Cells[x, y] != SystemEnum.eSellState.Filled) continue;

                // left
                if (IsNotFilledOrOut(x - 1, y))
                    edges.Add(new Edge(new Vector2Int(x, y), new Vector2Int(x, y + 1)));

                // right
                if (IsNotFilledOrOut(x + 1, y))
                    edges.Add(new Edge(new Vector2Int(x + 1, y), new Vector2Int(x + 1, y + 1)));

                // bottom
                if (IsNotFilledOrOut(x, y - 1))
                    edges.Add(new Edge(new Vector2Int(x, y), new Vector2Int(x + 1, y)));

                // top
                if (IsNotFilledOrOut(x, y + 1))
                    edges.Add(new Edge(new Vector2Int(x, y + 1), new Vector2Int(x + 1, y + 1)));
            }

            return edges;
        }
        private bool IsNotFilledOrOut(int x, int y)
        {
            if (x < 0 || y < 0 || x >= _cellW || y >= _cellH) return true;
            return _grid.Cells[x, y] != SystemEnum.eSellState.Filled;
        }
        // 점령구역의 테두리 선분 계산
        public List<(Vector2 a, Vector2 b)> GetCapturedBoundaryWorldSegments()
        {
            var edges = GetCapturedBoundaryEdges();
            var segs = new List<(Vector2 a, Vector2 b)>(edges.Count);

            foreach (var e in edges)
            {
                var a = GetNodeWorld(e.A.x, e.A.y);
                var b = GetNodeWorld(e.B.x, e.B.y);
                segs.Add((a, b));
            }

            return segs;
        }


        public void RebuildCapturedBoundaryEdgeColliders()
        {
            EnsureBoundaryRoot();
            ClearBoundaryColliders();
            
             var boundaryEdges = GetCapturedBoundaryEdges();
             if (boundaryEdges == null || boundaryEdges.Count == 0) return;
             
             var loops = BuildLoopsFromEdges(boundaryEdges);
             if (loops.Count == 0) return;
             
             for (int i = 0; i < loops.Count; i++)
             {
                 var nodeLoop = loops[i];

                 // EdgeCollider2D는 Vector2[] (로컬좌표) 필요
                 var localPoints = new Vector2[nodeLoop.Count + 1]; // 닫기 위해 +1
                 for (int p = 0; p < nodeLoop.Count; p++)
                 {
                     var n = nodeLoop[p];
                     var world = GetNodeWorld(n.x, n.y);
                     var local3 = boundaryColliderRoot.InverseTransformPoint(new Vector3(world.x, world.y, 0f));
                     localPoints[p] = new Vector2(local3.x, local3.y);
                 }
                 localPoints[nodeLoop.Count] = localPoints[0]; // 폐곡선 닫기

                 var go = new GameObject($"CapturedBoundaryCollider_{i}");
                 go.transform.SetParent(boundaryColliderRoot, false);

                 var col = go.AddComponent<EdgeCollider2D>();
                 col.edgeRadius = 0f;
                 col.points = localPoints;

                 boundaryColliders.Add(go);
             }
        }
        private void EnsureBoundaryRoot()
        {
            if (boundaryColliderRoot != null) return;

            var root = new GameObject("CapturedBoundaryColliders");
            root.transform.SetParent(transform, false);
            boundaryColliderRoot = root.transform;
        }
        private void ClearBoundaryColliders()
        {
            for (int i = 0; i < boundaryColliders.Count; i++)
            {
                var go = boundaryColliders[i];
                if (go == null) continue;
                Destroy(go);
            }
            boundaryColliders.Clear();
        }
        // HashSet<Edge> -> 여러 개의 루프(List<Vector2Int>)로 정렬
        private List<List<Vector2Int>> BuildLoopsFromEdges(HashSet<Edge> edges)
        {
            var remaining = new HashSet<Edge>(edges);

            // 인접 리스트
            var neighbors = new Dictionary<Vector2Int, List<Vector2Int>>(remaining.Count * 2);
            foreach (var e in remaining)
            {
                AddNeighbor(neighbors, e.A, e.B);
                AddNeighbor(neighbors, e.B, e.A);
            }

            var loops = new List<List<Vector2Int>>();

            while (remaining.Count > 0)
            {
                // 남은 엣지 아무거나 하나 잡고 시작
                var first = remaining.First();
                remaining.Remove(first);

                var start = first.A;
                var prev = first.A;
                var curr = first.B;

                var loop = new List<Vector2Int>(256) { start, curr };

                // 안전장치 (무한루프 방지)
                int guard = edges.Count + 10;

                while (guard-- > 0)
                {
                    if (curr == start) break;

                    if (!neighbors.TryGetValue(curr, out var nextList) || nextList.Count == 0)
                        break;

                    Vector2Int next = default;
                    bool found = false;

                    // 보통 degree=2 라서 "prev가 아닌 쪽"이 다음
                    for (int i = 0; i < nextList.Count; i++)
                    {
                        var cand = nextList[i];
                        if (cand == prev) continue;

                        var candEdge = new Edge(curr, cand);
                        if (!remaining.Contains(candEdge)) continue;

                        next = cand;
                        found = true;
                        break;
                    }

                    // 혹시 위에서 못 찾으면(희귀 케이스): prev로도 이어지는지 체크
                    if (!found)
                    {
                        for (int i = 0; i < nextList.Count; i++)
                        {
                            var cand = nextList[i];
                            var candEdge = new Edge(curr, cand);
                            if (!remaining.Contains(candEdge)) continue;

                            next = cand;
                            found = true;
                            break;
                        }
                    }

                    if (!found) break;

                    remaining.Remove(new Edge(curr, next));
                    prev = curr;
                    curr = next;
                    loop.Add(curr);
                }

                // 루프가 최소 3점 이상이어야 의미있는 폐곡선
                if (loop.Count >= 3)
                {
                    // 마지막이 start면 중복 start 제거(우린 collider 세팅에서 닫으니까)
                    if (loop[loop.Count - 1] == start)
                        loop.RemoveAt(loop.Count - 1);

                    loops.Add(loop);
                }
            }

            return loops;

            static void AddNeighbor(Dictionary<Vector2Int, List<Vector2Int>> map, Vector2Int a, Vector2Int b)
            {
                if (!map.TryGetValue(a, out var list))
                {
                    list = new List<Vector2Int>(2);
                    map.Add(a, list);
                }
                list.Add(b);
            }
        }
        #endregion
        
        
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

            foreach (var p2 in from enemy in enemies where enemy != null && enemy.CurrentNode != null select enemy.CurrentNode into node select GridMath.NodeToWorld(node.x, node.y, _origin, _cellWorldSize))
            {
                Gizmos.DrawSphere(new Vector3(p2.x, p2.y, 0f), _cellWorldSize * 0.15f);
            }
        }
    }
}
