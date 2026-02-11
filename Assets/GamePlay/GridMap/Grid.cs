using UnityEngine;
using GamePlay;

namespace GamePlay.GridMap
{
    public class Grid
    {
        public int Width { get; }
        public int Height { get; }

        public readonly SystemEnum.eSellState[,] Cells;

        public Grid(int w, int h, Vector2Int startPos, Vector2Int startSize)
        {
            Width = Mathf.Max(1, w);
            Height = Mathf.Max(1, h);

            Cells = new SystemEnum.eSellState[Width, Height];

            for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
                Cells[x, y] = SystemEnum.eSellState.Empty;

            FillRectClamped(startPos, startSize);
        }

        public bool InBoundsCell(int x, int y) => x >= 0 && y >= 0 && x < Width && y < Height;

        public void FillRectClamped(Vector2Int start, Vector2Int size)
        {
            int x0 = Mathf.Clamp(start.x, 0, Width - 1);
            int y0 = Mathf.Clamp(start.y, 0, Height - 1);
            int x1 = Mathf.Clamp(start.x + size.x - 1, 0, Width - 1);
            int y1 = Mathf.Clamp(start.y + size.y - 1, 0, Height - 1);

            for (int x = x0; x <= x1; x++)
            for (int y = y0; y <= y1; y++)
                Cells[x, y] = SystemEnum.eSellState.Filled;
        }
    }
}