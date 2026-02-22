using System;
using GamePlay;
using Managers;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class StageClearUI : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;

    private InputAction _selectAction;

    private void Awake()
    {
        _selectAction = playerInput.actions["Decide"];
    }

    private void OnEnable()
    {
        _selectAction.performed += OnSelectPressed;
    }

    private void OnDisable()
    {
        _selectAction.performed -= OnSelectPressed;
    }

    private void OnSelectPressed(InputAction.CallbackContext context)
    {
        GameManager.Instance.Scene.StartCharacterNovel(GameManager.Instance.currentCharacter, SystemEnum.NovelScriptType.after);
    }
}
