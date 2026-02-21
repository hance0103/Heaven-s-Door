using UnityEngine;

namespace GamePlay.GridMap
{
    public class Grid
    {
        private int Width { get; }
        private int Height { get; }

        public readonly SystemEnum.eSellState[,] Cells;

        public Grid(int w, int h, Vector2Int startPos, Vector2Int startSize)
        {
            Width = Mathf.Max(1, w);
            Height = Mathf.Max(1, h);

            Cells = new SystemEnum.eSellState[Width, Height];

            for (var x = 0; x < Width; x++)
            for (var y = 0; y < Height; y++)
                Cells[x, y] = SystemEnum.eSellState.Empty;
            
            
            //FillBorderWalls(); 
            FillRectClamped(startPos, startSize);
        }

        public bool InBoundsCell(int x, int y) => x >= 0 && y >= 0 && x < Width && y < Height;

        private void FillRectClamped(Vector2Int start, Vector2Int size)
        {
            var x0 = Mathf.Clamp(start.x, 0, Width - 1);
            var y0 = Mathf.Clamp(start.y, 0, Height - 1);
            var x1 = Mathf.Clamp(start.x + size.x - 1, 0, Width - 1);
            var y1 = Mathf.Clamp(start.y + size.y - 1, 0, Height - 1);

            for (var x = x0; x <= x1; x++)
            for (var y = y0; y <= y1; y++)
                Cells[x, y] = SystemEnum.eSellState.Filled;
        }
        
        private void FillBorderWalls()
        {
            for (var x = 0; x < Width; x++)
            {
                Cells[x, 0] = SystemEnum.eSellState.Wall;
                Cells[x, Height - 1] = SystemEnum.eSellState.Wall;
            }

            for (var y = 0; y < Height; y++)
            {
                Cells[0, y] = SystemEnum.eSellState.Wall;
                Cells[Width - 1, y] = SystemEnum.eSellState.Wall;
            }
        }
    }
}