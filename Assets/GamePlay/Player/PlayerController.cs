using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    
    private PlayerInput _playerInput;
    private InputAction _occupy;
    
    private Vector2 _moveDirection = Vector2.zero;
    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _occupy = _playerInput.actions["Occupy"];
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

    private void OnMove(InputValue value)
    {
        // 움직일수 없을 경우 return;
        _moveDirection = value.Get<Vector2>();

        if (_moveDirection == Vector2.zero) return;
        
        Debug.Log(_moveDirection);
    }
    private void OnOccupyPressed(InputAction.CallbackContext context)
    {
        Debug.Log("Occupy Pressed");
    }
    private void OnOccupyReleased(InputAction.CallbackContext context)
    {
        Debug.Log("Occupy Released");
    }
}
