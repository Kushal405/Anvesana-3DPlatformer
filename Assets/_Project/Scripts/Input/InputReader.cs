using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static PlayerInputActions; // ← this already gives you IPlayerActions

// the manual IPlayerActions interface 
// public interface IPlayerActions
    // {
    //     void OnMove(InputAction.CallbackContext context);
    //     void OnLook(InputAction.CallbackContext context);
    //     void OnMouseControlCamera(InputAction.CallbackContext context);
    //     void OnJump(InputAction.CallbackContext context);
    //     void OnFire(InputAction.CallbackContext context);
    //     void OnRun(InputAction.CallbackContext context);
    // }

[CreateAssetMenu(fileName = "InputReader", menuName = "Platformer/Input Reader")]
public class InputReader : ScriptableObject, PlayerInputActions.IPlayerActions // ← use full name
{
    public event Action<Vector2> Move = delegate { };
    public event Action<Vector2, bool> Look = delegate { };
    public event Action EnableMouseControlCamera = delegate { };
    public event Action DisableMouseControlCamera = delegate { };
    public event Action<bool> Jump = delegate { };

    PlayerInputActions inputActions;

    public Vector2 Direction => inputActions.Player.Move.ReadValue<Vector2>();

    void OnEnable()
    {
        Debug.Log("InputReader OnEnable fired");//temp
        if (inputActions == null)
        {
            inputActions = new PlayerInputActions();
            inputActions.Player.SetCallbacks(this);
        }
        inputActions.Player.Enable();
    }

    void OnDisable()
    {
        inputActions.Player.Disable();
    }

    public void OnMove(InputAction.CallbackContext context)
        => Move.Invoke(context.ReadValue<Vector2>());

    public void OnLook(InputAction.CallbackContext context)
        => Look.Invoke(context.ReadValue<Vector2>(), IsDeviceMouse(context));

    public void OnMouseControlCamera(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Started:
                EnableMouseControlCamera.Invoke();
                break;
            case InputActionPhase.Canceled:
                DisableMouseControlCamera.Invoke();
                break;
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Started:
                Jump.Invoke(true);
                break;
            case InputActionPhase.Canceled:
                Jump.Invoke(false);
                break;
        }
    }

    public void OnFire(InputAction.CallbackContext context) { }
    public void OnRun(InputAction.CallbackContext context) { }

    bool IsDeviceMouse(InputAction.CallbackContext context)
        => context.control.device is Mouse;
}