using System;
using System.Collections.Generic;
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
            InitPlayerPosition(gridManager.startPos.x - 1,  gridManager.startPos.y - 1);
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

        [ContextMenu("이동 가능성 판정")]
        public void JudgeDirection()
        {
            var moveCandidates = new List<(Vector2Int, Vector2Int)>();
            
            moveCandidates.Add((new Vector2Int(0, 1), Vector2Int.zero));
            moveCandidates.Add((new Vector2Int(0, 1), new Vector2Int(1, 0)));
            moveCandidates.Add((new Vector2Int(0, 1), new Vector2Int(-1, 0)));
            
            moveCandidates.Add((new Vector2Int(0, -1), Vector2Int.zero));
            moveCandidates.Add((new Vector2Int(0, -1), new Vector2Int(1, 0)));
            moveCandidates.Add((new Vector2Int(0, -1), new Vector2Int(-1, 0)));
            
            moveCandidates.Add((new Vector2Int(1, 0), Vector2Int.zero));
            moveCandidates.Add((new Vector2Int(1, 0), new Vector2Int(0, 1)));
            moveCandidates.Add((new Vector2Int(1, 0), new Vector2Int(0, -1)));
            
            moveCandidates.Add((new Vector2Int(-1, 0), Vector2Int.zero));
            moveCandidates.Add((new Vector2Int(-1, 0), new Vector2Int(0, 1)));
            moveCandidates.Add((new Vector2Int(-1, 0), new Vector2Int(0, -1)));

            foreach (var move in moveCandidates)
            {
                if (gridManager.CanMoveNode(currentPos + move.Item2, move.Item1))
                {
                    Debug.Log(move.Item1 + ", " +  move.Item2);
                }
            }
            
        }
        
        
        #region MoveOnGrid

        private async UniTask MoveApply()
        {
            _canMove = false;
            
            var x = _moveDirection.x;
            var y = _moveDirection.y;
            
            // 기존 입력 방향, 보정 방향
            var moveCandidates = new List<(Vector2Int, Vector2Int)>();

            if (y != 0)
            {
                moveCandidates.Add((new Vector2Int(0, y), Vector2Int.zero));
                moveCandidates.Add((new Vector2Int(0, y), new Vector2Int(1, 0)));
                moveCandidates.Add((new Vector2Int(0, y), new Vector2Int(-1, 0)));
            }

            if (x != 0)
            {
                moveCandidates.Add((new Vector2Int(x, 0), Vector2Int.zero));
                moveCandidates.Add((new Vector2Int(x, 0), new Vector2Int(0, 1)));
                moveCandidates.Add((new Vector2Int(x, 0), new Vector2Int(0, -1)));
            }

            foreach (var candidate in moveCandidates)
            {
                if (await TryMoveOneStep(candidate.Item1, candidate.Item2))
                    break;
            }
            
            _canMove = true;

        }

        private async UniTask<bool> TryMoveOneStep(Vector2Int direction, Vector2Int cor)
        {
            var nextX = currentPos.x + direction.x + cor.x;
            var nextY = currentPos.y + direction.y + cor.y;

            // 유효성 검사
            if (!gridManager.CanMoveNode(currentPos + cor, direction)) return false;
            
            
            // 실제 이동 방향
            var realDirection = direction + cor;
            
            var start = transform.position;
            
            if (realDirection.x == 0 || realDirection.y == 0)
            {

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
            }
            // 대각선 이동
            else
            {
                var transit2D = gridManager.GetNodePosition(currentPos.x + cor.x, currentPos.y + cor.y);
                var transit = new Vector3(transit2D.x, transit2D.y, 0f);

                var dest2D = gridManager.GetNodePosition(nextX, nextY);
                var dest = new Vector3(dest2D.x, dest2D.y, 0f);
                
                currentPos = new Vector2Int(nextX, nextY);
                
                
                var t = 0f;
                var duration = moveDelay * speed / 2;
                while (t < 1f)
                {
                    t += Time.deltaTime / duration;
                    transform.position = Vector3.Lerp(start, transit, t);
                    await UniTask.Yield();
                }
                transform.position = transit;

                t = 0f;
                while (t < 1f)
                {
                    t += Time.deltaTime / duration;
                    transform.position = Vector3.Lerp(transit, dest, t);
                    await UniTask.Yield();
                }
                transform.position = dest;
            }
            
            

            return true;
        }
        
        // 지금은 초기 위치 설정에만 사용함
        private void InitPlayerPosition(int x, int y)
        {
            if (gridManager == null) return;

            var pos = gridManager.GetNodePosition(x, y);
            transform.position = pos;
            currentPos = new Vector2Int(x, y);
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
