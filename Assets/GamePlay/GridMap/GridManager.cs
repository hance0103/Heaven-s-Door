using System;
using System.Collections.Generic;
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
        
        
        public bool CanMoveNode(Vector2Int nodePosition, Vector2Int direction)
        {
            var cellA = _grid.Cells[nodePosition.x, nodePosition.y];
            var cellB = _grid.Cells[nodePosition.x + 1, nodePosition.y];
            var cellC = _grid.Cells[nodePosition.x, nodePosition.y + 1];
            var cellD = _grid.Cells[nodePosition.x + 1, nodePosition.y + 1];
            
            
            switch (direction.y)
            {
                // 위쪽 방향 이동
                case > 0:
                    // 상단 두칸중 적어도 한칸이 채워졌고, 서로 다른 칸일 경우
                    return (cellC == SystemEnum.eSellState.Filled || cellD == SystemEnum.eSellState.Filled) &&
                           (cellC != cellD);
                // 아래 방향 이동
                case < 0:
                {
                    // 하단 두칸중 적어도 한칸이 채워졌고, 서로 다른 칸일 경우
                    return (cellA == SystemEnum.eSellState.Filled || cellB == SystemEnum.eSellState.Filled)
                           && (cellA != cellB);
                }

                default:
                {
                    switch (direction.x)
                    {
                        // 우 방향 이동
                        case > 0:
                            //  우측 두칸중 적어도 한칸이 채워졌고, 서로 다른 칸일 경우
                            return (cellB == SystemEnum.eSellState.Filled || cellD == SystemEnum.eSellState.Filled)
                                   && (cellB != cellD);
                        // 좌 방향 이동
                        // 좌측 두칸중 적어도 한칸이 채워졌고, 서로 다른 칸일 경우
                        case < 0:
                            return (cellA == SystemEnum.eSellState.Filled || cellC == SystemEnum.eSellState.Filled)
                                   && (cellA != cellC);
                    }

                    break;
                }
            }

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

                    if (_grid.Cells[x, y] == SystemEnum.eSellState.Empty)
                    {
                        Gizmos.color = Color.green;
                    }
                    else if (_grid.Cells[x,y] == SystemEnum.eSellState.Wall)
                    {
                        Gizmos.color = Color.red;
                    }
                    else if (_grid.Cells[x, y] == SystemEnum.eSellState.Filled)
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