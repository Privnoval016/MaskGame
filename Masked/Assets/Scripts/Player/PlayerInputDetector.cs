using System;
using Extensions.EventBus;
using Extensions.Patterns;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputDetector : Singleton<PlayerInputDetector>, PlayerInput.IPlayingActions
{
    private PlayerInput playerInput;
    private bool inputEnabled = true;

    protected override void Awake()
    {
        base.Awake();
        playerInput = new PlayerInput();
        playerInput.Playing.SetCallbacks(this);
    }

    private void OnEnable()
    {
        playerInput.Playing.Enable();
        inputEnabled = true;
    }
    
    private void OnDisable()
    {
        playerInput.Playing.Disable();
    }
    
    /// <summary>
    /// Disable all input (e.g., during game over)
    /// </summary>
    public void DisableInput()
    {
        inputEnabled = false;
        playerInput.Playing.Disable();
    }
    
    /// <summary>
    /// Enable input
    /// </summary>
    public void EnableInput()
    {
        inputEnabled = true;
        playerInput.Playing.Enable();
    }

    #region IPlayingActions Implementation

    public void OnTile1(InputAction.CallbackContext context)
    {
        if (!inputEnabled) return;
        
        Debug.Log(context.performed);
        if (context.performed)
        {
            Vector2 direction = context.ReadValue<Vector2>();
            
            // Always play sound effect
            SoundEffectManager.Instance.Play(SoundEffectManager.Instance.soundEffectAtlas.buttonHit);
            
            // Only raise event if autoplay is not enabled
            if (GameManager.Instance?.livePlayData?.autoPlayEnabled != true)
            {
                EventBus<ButtonPressedEvent>.Raise(new ButtonPressedEvent(0, direction));
            }
        }
    }
    
    public void OnTile2(InputAction.CallbackContext context)
    {
        if (!inputEnabled) return;
        
        if (context.performed)
        {
            Vector2 direction = context.ReadValue<Vector2>();
            
            // Always play sound effect
            SoundEffectManager.Instance.Play(SoundEffectManager.Instance.soundEffectAtlas.buttonHit);
            
            // Only raise event if autoplay is not enabled
            if (GameManager.Instance?.livePlayData?.autoPlayEnabled != true)
            {
                EventBus<ButtonPressedEvent>.Raise(new ButtonPressedEvent(1, direction));
            }
        }
    }
    
    public void OnTile3(InputAction.CallbackContext context)
    {
        if (!inputEnabled) return;
        
        if (context.performed)
        {
            Vector2 direction = context.ReadValue<Vector2>();
            
            // Always play sound effect
            SoundEffectManager.Instance.Play(SoundEffectManager.Instance.soundEffectAtlas.buttonHit);
            
            // Only raise event if autoplay is not enabled
            if (GameManager.Instance?.livePlayData?.autoPlayEnabled != true)
            {
                EventBus<ButtonPressedEvent>.Raise(new ButtonPressedEvent(2, direction));
            }
        }
    }
    
    public void OnTile4(InputAction.CallbackContext context)
    {
        if (!inputEnabled) return;
        
        if (context.performed)
        {
            Vector2 direction = context.ReadValue<Vector2>();
            
            // Always play sound effect
            SoundEffectManager.Instance.Play(SoundEffectManager.Instance.soundEffectAtlas.buttonHit);
            
            // Only raise event if autoplay is not enabled
            if (GameManager.Instance?.livePlayData?.autoPlayEnabled != true)
            {
                EventBus<ButtonPressedEvent>.Raise(new ButtonPressedEvent(3, direction));
            }
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