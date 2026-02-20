using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GamePlay.UI.SelectObjects
{
    public class SelectObjectController : MonoBehaviour
    {
        [SerializeField] private List<SelectObject> selectObjects = new List<SelectObject>();
        [SerializeField] private PlayerInput playerInput;
        private InputAction _selectAction;
        private InputAction _decideAction;
        
        [SerializeField] private int currentIndex = 0;

        private void Awake()
        {
            playerInput = GetComponent<PlayerInput>();
            _selectAction =  playerInput.actions["Select"];
            _decideAction = playerInput.actions["Decide"];
        }

        private void Start()
        {
            selectObjects[currentIndex].Select();
        }

        private void OnEnable()
        {
            currentIndex = 0;

            
            _selectAction.performed += OnSelectPressed;
            _decideAction.performed += OnDecidePressed;
        }

        private void OnDisable()
        {
            _selectAction.performed -= OnSelectPressed;
            _decideAction.performed -= OnDecidePressed;
        }

        private void OnSelectPressed(InputAction.CallbackContext context)
        {
            var select = context.action.ReadValue<float>();
            MoveSelect(select);
        }

        private void OnDecidePressed(InputAction.CallbackContext context)
        {
            Debug.Log("지금거 실행");
            //selectObjects[currentIndex].Execute();
        }

        private void MoveSelect(float axis)
        {
            Debug.Log(axis);
            switch (axis)
            {
                case > 0:
                {
                    if (currentIndex < selectObjects.Count - 1)
                    {
                        selectObjects[currentIndex].Deselect();
                        currentIndex++;
                        selectObjects[currentIndex].Select();
                    }
                }
                break;
                case < 0:
                {
                    if (currentIndex > 0)
                    {
                        selectObjects[currentIndex].Deselect();
                        currentIndex--;
                        selectObjects[currentIndex].Select();
                    }
                }
                break;
            }
        }
        
        private void Select(int index)
        {
            
        }

        private void Decide()
        {
            // 현재 인덱스 선택
        }
    }
}
