using System;
using GamePlay.GridMap;
using UnityEngine;
using Grid = GamePlay.GridMap.Grid;

namespace GamePlay
{
    public class Test : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] public int cellSize;
        [SerializeField] private GameObject Controller;
        
        private Vector2Int spriteSize;
        private Vector2Int gridSize;
    
        // 그리드 (0, 0)의 월드좌표
        private Vector3 mapOrigin;
    
        private Grid grid;
        

        private void Start()
        {
            Sprite sprite = spriteRenderer.sprite;
        
            int pixelWidth = (int)sprite.rect.width;
            int pixelHeight = (int)sprite.rect.height;
        
            spriteSize = new Vector2Int(pixelWidth, pixelHeight);
        
            Debug.Log(spriteSize);
            
            int gridWidth = pixelWidth / cellSize;
            int gridHeight = pixelHeight / cellSize;
            gridSize = new Vector2Int(gridWidth, gridHeight);
            Debug.Log(gridSize);
            
            grid = new Grid(gridWidth,  gridHeight);

            mapOrigin = spriteRenderer.bounds.min + Vector3.one * cellSize / 200f;
            
            Debug.Log(mapOrigin);
            Debug.Log(GridMath.GridToWorld(0, 0, mapOrigin, cellSize));
        }

        void OnDrawGizmos()
        {
            if (grid == null) return;

            Gizmos.color = Color.green;
            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    Vector3 pos = mapOrigin + new Vector3((x) * cellSize, (y) * cellSize, 0)/100f;
                    
                    Gizmos.DrawWireCube(
                        pos,
                        Vector3.one * cellSize / 100f
                    );
                }
            }
        }
    }
}
