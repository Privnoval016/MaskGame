using System;
using Extensions.EventBus;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputDetector : MonoBehaviour, PlayerInput.IPlayingActions
{
    private PlayerInput playerInput;

    private void Awake()
    {
        playerInput = new PlayerInput();
        playerInput.Playing.SetCallbacks(this);
    }

    private void OnEnable()
    {
        playerInput.Playing.Enable();
    }
    
    private void OnDisable()
    {
        playerInput.Playing.Disable();
    }

    #region IPlayingActions Implementation

    public void OnTile1(InputAction.CallbackContext context)
    {
        Debug.Log(context.performed);
        if (context.performed)
        {
            Debug.Log($"Tile1 pressed with context: {context}");
            Vector2 direction = context.ReadValue<Vector2>();
            EventBus<ButtonPressedEvent>.Raise(new ButtonPressedEvent(0, direction));
        }
    }
    
    public void OnTile2(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Vector2 direction = context.ReadValue<Vector2>();
            EventBus<ButtonPressedEvent>.Raise(new ButtonPressedEvent(1, direction));
        }
    }
    
    public void OnTile3(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Vector2 direction = context.ReadValue<Vector2>();
            EventBus<ButtonPressedEvent>.Raise(new ButtonPressedEvent(2, direction));
        }
    }
    
    public void OnTile4(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Vector2 direction = context.ReadValue<Vector2>();
            EventBus<ButtonPressedEvent>.Raise(new ButtonPressedEvent(3, direction));
        }
    }

    #endregion
}

public struct ButtonPressedEvent : IEvent
{
    public int buttonIndex; // 0 to number of buttons - 1
    public Vector2 direction; // Direction of the input
    
    public ButtonPressedEvent(int buttonIndex, Vector2 direction)
    {
        this.buttonIndex = buttonIndex;
        this.direction = direction;
    }
}