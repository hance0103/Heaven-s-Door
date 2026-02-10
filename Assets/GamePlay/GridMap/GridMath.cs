using UnityEngine;

namespace GamePlay.GridMap
{
    public static class GridMath
    {
        public static Vector2 GridToWorld(int x, int y, Vector3 origin, int cellSize)
        {
            return origin + new Vector3(
                x * cellSize / 100f,
                y * cellSize / 100f,
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

        public static Vector2Int WorldToGrid(Vector2 worldPosition, Vector3 origin, int cellSize)
        {
            var local = worldPosition - (Vector2)origin;
            var gx = Mathf.RoundToInt(local.x * 100f / cellSize);
            var gy = Mathf.RoundToInt(local.y * 100f / cellSize);
            return new Vector2Int(gx, gy);
        }

        public static Vector2Int WorldToNode(Vector2 worldPosition, Vector3 origin, int cellSize)
        {
            var local = worldPosition - (Vector2)origin;
            var nx = Mathf.RoundToInt(local.x * 100f / cellSize - 0.5f);
            var ny = Mathf.RoundToInt(local.y * 100f / cellSize - 0.5f);
            return new Vector2Int(nx, ny);
        }
    }
}