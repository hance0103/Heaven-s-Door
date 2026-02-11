using UnityEngine;

namespace GamePlay.GridMap
{
    public static class GridMath
    {
        public static Vector2 CellCenterToWorld(int x, int y, Vector3 origin, float cellWorldSize)
        {
            return origin + new Vector3((x + 0.5f) * cellWorldSize, (y + 0.5f) * cellWorldSize, 0f);
        }

        public static Vector2 NodeToWorld(int x, int y, Vector3 origin, float cellWorldSize)
        {
            return origin + new Vector3(x * cellWorldSize, y * cellWorldSize, 0f);
        }

        public static Vector2Int WorldToCell(Vector2 world, Vector3 origin, float cellWorldSize)
        {
            var local = world - (Vector2)origin;
            int x = Mathf.FloorToInt(local.x / cellWorldSize);
            int y = Mathf.FloorToInt(local.y / cellWorldSize);
            return new Vector2Int(x, y);
        }

        public static Vector2Int WorldToNode(Vector2 world, Vector3 origin, float cellWorldSize)
        {
            var local = world - (Vector2)origin;
            int x = Mathf.RoundToInt(local.x / cellWorldSize);
            int y = Mathf.RoundToInt(local.y / cellWorldSize);
            return new Vector2Int(x, y);
        }
    }
}