using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GamePlay.UI.SelectObjects.Controller
{
    public class SelectObjectController : MonoBehaviour
    {
        //TODO : 나중에 행/열 추가해서 위 아래로도 이동 가능하도록 만들것
        
        [SerializeField] protected List<SelectObject> selectObjects = new List<SelectObject>();
        [SerializeField] protected PlayerInput playerInput;

        private InputAction selectAction;
        private InputAction decideAction;
        
        [SerializeField] protected int currentIndex = 0;

        protected virtual void Awake()
        {
            playerInput = GetComponent<PlayerInput>();
            selectAction =  playerInput.actions["Select"];
            decideAction = playerInput.actions["Decide"];
        }

        protected virtual void Start()
        {
            for (var i = 0; i < selectObjects.Count; i++)
            {
                if (i == 0)
                {
                    selectObjects[i].Select();
                }
                else
                {
                    selectObjects[i].Deselect();
                }
            }

        }

        protected virtual void OnEnable()
        {
            currentIndex = 0;

            
            selectAction.performed += OnSelectPressed;
            decideAction.performed += OnDecidePressed;
        }

        protected virtual void OnDisable()
        {
            selectAction.performed -= OnSelectPressed;
            decideAction.performed -= OnDecidePressed;
        }
        
        
        // 좌우 이동
        protected virtual void OnSelectPressed(InputAction.CallbackContext context)
        {
            var select = context.action.ReadValue<float>();
            MoveSelect(select);
        }
        
        
        // 엔터/스페이스바
        protected virtual void OnDecidePressed(InputAction.CallbackContext context)
        {
            selectObjects[currentIndex].Execute();
        }

        private void MoveSelect(float axis)
        {
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
    }
}
