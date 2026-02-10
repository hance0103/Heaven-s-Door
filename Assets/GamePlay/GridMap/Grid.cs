using UnityEngine;
using GamePlay;

namespace GamePlay.GridMap
{
    public class Grid
    {
        private readonly int _width;
        private readonly int _height;

        public int Width => _width;
        public int Height => _height;

        public readonly SystemEnum.eSellState[,] Cells;
        public readonly SystemEnum.eNodeState[,] Nodes;

        public Grid(int w, int h, Vector2Int startPos)
        {
            _width = w;
            _height = h;

            Cells = new SystemEnum.eSellState[w, h];
            Nodes = new SystemEnum.eNodeState[w - 1, h - 1];

            InitGrid();
            SetInitRegion(startPos, 3, 3);
            RecalculateNodes();
        }

        private void InitGrid()
        {
            for (var x = 0; x < _width; x++)
            {
                for (var y = 0; y < _height; y++)
                {
                    if (x == 0 || x == _width - 1 || y == 0 || y == _height - 1)
                        Cells[x, y] = SystemEnum.eSellState.Wall;
                    else
                        Cells[x, y] = SystemEnum.eSellState.Empty;
                }
            }
        }

        private void SetInitRegion(Vector2Int start, int width, int height)
        {
            for (var x = start.x; x < start.x + width; x++)
            {
                for (var y = start.y; y < start.y + height; y++)
                {
                    Cells[x, y] = SystemEnum.eSellState.Filled;
                }
            }
        }

        private static bool IsCaptured(SystemEnum.eSellState s)
        {
            return s == SystemEnum.eSellState.Filled;
        }

        private static bool IsHardSolid(SystemEnum.eSellState s)
        {
            return s == SystemEnum.eSellState.Filled || s == SystemEnum.eSellState.Wall;
        }

        public void RecalculateNodes()
        {
            for (var x = 0; x < _width - 1; x++)
            {
                for (var y = 0; y < _height - 1; y++)
                {
                    var a = Cells[x, y];
                    var b = Cells[x + 1, y];
                    var c = Cells[x, y + 1];
                    var d = Cells[x + 1, y + 1];

                    var ca = IsCaptured(a);
                    var cb = IsCaptured(b);
                    var cc = IsCaptured(c);
                    var cd = IsCaptured(d);

                    var anyCaptured = ca || cb || cc || cd;
                    var allCaptured = ca && cb && cc && cd;

                    if (allCaptured)
                    {
                        Nodes[x, y] = SystemEnum.eNodeState.CannotMove;
                        continue;
                    }

                    Nodes[x, y] = anyCaptured
                        ? SystemEnum.eNodeState.Moveable   // “점령된 영역 경계” 노드
                        : SystemEnum.eNodeState.Drawable;  // 그냥 빈 공간 노드
                }
            }
        }
    }
}
