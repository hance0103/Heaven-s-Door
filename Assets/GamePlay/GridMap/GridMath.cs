using UnityEngine;

namespace GamePlay.GridMap
{
    public static class GridMath
    {
        public static Vector2 GridToWorld(int x, int y, Vector3 origin, int cellSize)
        {
            return origin + new Vector3(
                (x) * cellSize / 100f,
                (y) * cellSize / 100f,
                0f
            );
        }
        public static Vector2 NodeToWorld(int x, int y, Vector3 origin, int cellSize)
        {
            return origin + new Vector3(
                (x + 0.5f) * cellSize / 100f,
                (y + 0.5f) * cellSize / 100f,
                0f
            );
        }
        public static Vector2Int WorldToGrid(Vector2 worldPosition)
        {
            return new Vector2Int(
                
            );
        }
    }
}
