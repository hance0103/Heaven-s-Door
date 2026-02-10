using System;
using Cysharp.Threading.Tasks;
using System.Threading;
using GamePlay.GridMap;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GamePlay.Player
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float speed = 1f;
        [SerializeField] private float moveDelay = 0.2f;
        [SerializeField] private Vector2Int currentPos;
        [SerializeField] private GridManager gridManager;
        private PlayerInput _playerInput;
        private InputAction _occupy;
    
        private Vector2Int _moveDirection = Vector2Int.zero;

        [SerializeField] private bool isMoving = false;
        [SerializeField] private bool occupying = false;

        private bool _canMove = true;
        
        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
            _occupy = _playerInput.actions["Occupy"];
        }

        private void Start()
        {
            SetPlayerPosition(gridManager.startPos.x - 1,  gridManager.startPos.y - 1);
        }

        private void Update()
        {
            if (isMoving && _canMove)
            {
                MoveApply().Forget();
            }
        }

        private void OnEnable()
        {
            _occupy.started += OnOccupyPressed;
            _occupy.canceled += OnOccupyReleased;
        }

        private void OnDisable()
        {
            _occupy.started -= OnOccupyPressed;
            _occupy.canceled -= OnOccupyReleased;
        }

        #region MoveOnGrid

        private async UniTask MoveApply()
        {
            _canMove = false;
            
            var x = _moveDirection.x;
            var y = _moveDirection.y;

            // 대각선 입력
            if (x != 0 && y != 0)
            {
                // 상하 먼저 이동
                await TryMoveOneStep(new Vector2Int(0, y));
                
                // 좌우 이동
                await TryMoveOneStep(new Vector2Int(x, 0));
            }
            else
            {
                await TryMoveOneStep(_moveDirection);
            }

            _canMove = true;

        }

        private async UniTask<bool> TryMoveOneStep(Vector2Int direction)
        {
            int nextX = currentPos.x + direction.x;
            int nextY = currentPos.y + direction.y;

            // TODO 유효성 검사

            if (!gridManager.IsValidCell(nextX, nextY))
            {
                // 한칸 위 판정
                if (!gridManager.IsValidCell(nextX, nextY + 1))
                {
                    
                }
            }
            
            var start = transform.position;
            var dest2D = gridManager.GetNodePosition(nextX, nextY);
            var dest = new Vector3(dest2D.x, dest2D.y, 0f);
            
            currentPos = new Vector2Int(nextX, nextY);
            
            var t = 0f;
            var duration = moveDelay * speed;
            
            while (t < 1f)
            {
                t += Time.deltaTime / duration;
                transform.position = Vector3.Lerp(start, dest, t);
                await UniTask.Yield();
            }
            
            transform.position = dest;
            return true;
        }
        
        // 지금은 초기 위치 설정에만 사용함
        private void SetPlayerPosition(int x, int y)
        {
            if (gridManager == null) return;

            if (gridManager.IsValidCell(x, y))
            {
                var pos = gridManager.GetNodePosition(x, y);
                transform.position = pos;
                currentPos = new Vector2Int(x, y);
            }
            
        }
        
        #endregion
    
        #region InputAction

        private void OnMove(InputValue value)
        {
            // 움직일수 없을 경우 return;
            var raw = value.Get<Vector2>();
            
            _moveDirection = new Vector2Int((int)raw.x, (int)raw.y);

            if (_moveDirection == Vector2.zero)
            {
                isMoving = false;
                return;
            }
            isMoving = true;
        }
        private void OnOccupyPressed(InputAction.CallbackContext context)
        {
            occupying = true;
        }
        private void OnOccupyReleased(InputAction.CallbackContext context)
        {
            occupying = false;
        }

        #endregion

    }
}
