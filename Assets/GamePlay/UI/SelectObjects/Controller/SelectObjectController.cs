using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GamePlay.UI.SelectObjects.Controller
{
    [RequireComponent(typeof(AudioSource))]
    public class SelectObjectController : MonoBehaviour
    {
        [SerializeField] private Vector2 outlineWeight = new Vector2(10f, 10f);
        [SerializeField] protected Color selectedColor = Color.yellow;
        [SerializeField] protected Color deselectedColor = Color.white;
        
        
        [SerializeField] protected List<SelectObject> selectObjects = new List<SelectObject>();
        [SerializeField] protected PlayerInput playerInput;

        [SerializeField] protected AudioSource audioSource;
        [SerializeField] protected AudioClip selectSound;
        [SerializeField] protected AudioClip decideSound;
        
        
        private InputAction selectAction;
        private InputAction decideAction;
        
        [SerializeField] protected int currentIndex = 0;

        protected virtual void Awake()
        {
            playerInput = GetComponent<PlayerInput>();
            selectAction =  playerInput.actions["Select"];
            decideAction = playerInput.actions["Decide"];
            
            audioSource = GetComponent<AudioSource>();
        }

        protected virtual void Start()
        {
            
            for (var i = 0; i < selectObjects.Count; i++)
            {
                // 선 굵기 동일적용
                selectObjects[i].SetOutlineWeight(outlineWeight);
                
                if (i == 0)
                {
                    selectObjects[i].Select(selectedColor);
                }
                else
                {
                    selectObjects[i].Deselect(deselectedColor);
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
        protected virtual async void OnDecidePressed(InputAction.CallbackContext context)
        {
            audioSource.PlayOneShot(decideSound);
            await UniTask.Delay(TimeSpan.FromSeconds(decideSound.length));
            selectObjects[currentIndex].Execute();
        }

        private async void MoveSelect(float axis)
        {
            switch (axis)
            {
                case > 0:
                {
                    if (currentIndex < selectObjects.Count - 1)
                    {
                        selectObjects[currentIndex].Deselect(deselectedColor);
                        currentIndex++;
                        selectObjects[currentIndex].Select(selectedColor);
                    }
                }
                break;
                case < 0:
                {
                    if (currentIndex > 0)
                    {
                        selectObjects[currentIndex].Deselect(deselectedColor);
                        currentIndex--;
                        selectObjects[currentIndex].Select(selectedColor);
                    }
                }
                break;
            }
            
            
            // TODO: 이거 일단 강제 종료되도록 박아놨음 추후 수정 해야함
            audioSource.PlayOneShot(selectSound);
            await UniTask.Delay(TimeSpan.FromSeconds(1f));
            audioSource.Stop();
        }
    }
}
