using System;
using UnityEngine;

namespace GamePlay.GridMap
{
    public class GridManager : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] public int cellSize;
        [SerializeField] public Vector2Int startPos = new Vector2Int(1, 1);
        // 그리드 가로 세로 칸수
        private Vector2Int _gridSize;
        
        // 그리드 (0, 0)의 월드좌표
        private Vector3 _mapOrigin;
        
        // 게임에 사용할 그리드
        private Grid _grid;

        private void Awake()
        {
            var sprite = spriteRenderer.sprite;

            var gridWidth = (int)sprite.rect.width / cellSize;
            var gridHeight = (int)sprite.rect.height / cellSize;
            
            _gridSize = new Vector2Int(gridWidth, gridHeight);
            _grid = new Grid(gridWidth,  gridHeight, startPos);
            _mapOrigin = spriteRenderer.bounds.min + Vector3.one * cellSize / 200f;
        }

        private void Start()
        {


        }

        public Vector2 GetGridPosition(int x, int y)
        {
            return GridMath.GridToWorld(x, y, _mapOrigin, cellSize);
        }

        public Vector2 GetNodePosition(int x, int y)
        {
            return GridMath.NodeToWorld(x, y, _mapOrigin, cellSize);
        }

        // public bool CanMoveNode(Vector2Int nodePosition, Vector2Int direction)
        // {
        //     var nextX =  nodePosition.x + direction.x;
        //     var nextY =  nodePosition.y + direction.y;
        //
        //     if (nextX < 0 || nextY < 0 || nextX > _grid.Nodes.GetLength(0) || nextY > _grid.Nodes.GetLength(1))
        //         return false;
        //     
        //     var cellA = new Vector2Int(nextX, nextY);
        //     var cellB = new Vector2Int(nextX + 1, nextY);
        //     var cellC = new Vector2Int(nextX, nextY + 1);
        //     var cellD = new Vector2Int(nextX + 1, nextY + 1);
        //     
        //     if (_grid.Cells[cellA.x, cellA.y] == eSystemEnum.eSellState.Wall ||
        //         _grid.Cells[cellB.x, cellB.y] == eSystemEnum.eSellState.Wall ||
        //         _grid.Cells[cellC.x, cellC.y] == eSystemEnum.eSellState.Wall ||
        //         _grid.Cells[cellD.x, cellD.y] == eSystemEnum.eSellState.Wall)
        //         return false;
        //     
        //     if (_grid.Cells[cellA.x, cellA.y] == eSystemEnum.eSellState.Filled ||
        //         _grid.Cells[cellB.x, cellB.y] == eSystemEnum.eSellState.Filled ||
        //         _grid.Cells[cellC.x, cellC.y] == eSystemEnum.eSellState.Filled ||
        //         _grid.Cells[cellD.x, cellD.y] == eSystemEnum.eSellState.Filled)
        //         return false;
        //
        //     return true;
        // }
        
        
        // 움직일수 있는 칸인지 체크
        public bool IsValidCell(int x, int y)
        {
            if (x >= 0 && x < _gridSize.x - 1 && y >= 0 && y < _gridSize.y - 1)
            {
                return true;
            }
            
            
            

            return false;
        }

        public bool IsMovableNode(int x, int y)
        {
            
            
            return false;
        }

        public bool IsOccupiableCell(int x, int y)
        {
            
            
            return false;
        }
        public void OnDrawGizmos()
        {
            if (_grid == null) return;
            
            for (var x = 0; x < _gridSize.x; x++)
            {
                for (var y = 0; y < _gridSize.y; y++)
                {
                    var pos = _mapOrigin + new Vector3((x) * cellSize, (y) * cellSize, 0)/100f;

                    if (_grid.Cells[x, y] == eSystemEnum.eSellState.Empty)
                    {
                        Gizmos.color = Color.green;
                    }
                    else if (_grid.Cells[x,y] == eSystemEnum.eSellState.Wall)
                    {
                        Gizmos.color = Color.red;
                    }
                    else if (_grid.Cells[x, y] == eSystemEnum.eSellState.Filled)
                    {
                        Gizmos.color = Color.blue;
                    }
                    Gizmos.DrawWireCube(
                        pos,
                        Vector3.one * cellSize / 100f
                    );
                }
            }
        }
    }
}