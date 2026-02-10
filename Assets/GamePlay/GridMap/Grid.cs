using UnityEngine;

namespace GamePlay.GridMap
{
    public class Grid
    {
        // 그리드의 가로 세로 셀 개수
        private readonly int _width;
        private readonly int _height;
        public readonly eSystemEnum.eSellState[,] Cells;
        public readonly eSystemEnum.eNodeState[,] Nodes;

        public Grid(int w, int h, Vector2Int startPos)
        {
            _width = w;
            _height = h;
            Cells = new eSystemEnum.eSellState[w, h];
            Nodes = new eSystemEnum.eNodeState[w - 1, h - 1];
            InitGrid();
            SetInitRegion(startPos, 3, 3);
        }

        private void InitGrid()
        {
            for (int i = 0; i < _width; i++)
            {
                for (int j = 0; j < _height; j++)
                {
                    if (i == 0 || i == _width - 1 || j == 0 || j == _height - 1)
                    {
                        Cells[i, j] = eSystemEnum.eSellState.Wall;
                    }
                    else
                    {
                        Cells[i, j] = eSystemEnum.eSellState.Empty;
                    }
                }
            }
        }

        private void SetInitRegion(Vector2Int start, int width, int height)
        {
            for (int i = start.x; i < start.x + width; i++)
            {
                for (int j = start.y; j < start.y + height; j++)
                {
                    Cells[i, j] = eSystemEnum.eSellState.Filled;
                }
            }
        }
    }
}
