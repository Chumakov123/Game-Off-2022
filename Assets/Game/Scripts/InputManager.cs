using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager instance;
    private CharacterControl _playerControl;
    private PlayerInput _playerInput;
    private Secondclasshero _controls;
    void Subscribe(UnityEngine.InputSystem.InputAction action, System.Action<UnityEngine.InputSystem.InputAction.CallbackContext> func)
    {
        action.started += func;
        action.performed += func;
        action.canceled += func;
    }
    void Unscribe(UnityEngine.InputSystem.InputAction action, System.Action<UnityEngine.InputSystem.InputAction.CallbackContext> func)
    {
        action.started -= func;
        action.performed -= func;
        action.canceled -= func;
    }
    void OnEnable()
    {
        _controls.Enable();
    }

    void OnDisable()
    {
        _controls.Disable();
        _playerControl.Move(new UnityEngine.InputSystem.InputAction.CallbackContext());
    }
    void SubscribeAll(CharacterControl control)
    {
        Subscribe(_controls.Player.Fire,control.Attack);
        Subscribe(_controls.Player.Move,control.Move);
        Subscribe(_controls.Player.Jump,control.Jump);
        Subscribe(_controls.Player.GetDown,control.GetDown);
        Subscribe(_controls.Player.Dash,control.Dash);
        Subscribe(_controls.Player.SecondaryFire,control.SecondaryAttack);
        Subscribe(_controls.Player.Scroll,control.Scroll);
        Subscribe(_controls.Player.Look,control.Look);
    }
    void UnscribeAll(CharacterControl control)
    {
        Unscribe(_controls.Player.Fire,control.Attack);
        Unscribe(_controls.Player.Move,control.Move);
        Unscribe(_controls.Player.Jump,control.Jump);
        Unscribe(_controls.Player.GetDown,control.GetDown);
        Unscribe(_controls.Player.Dash,control.Dash);
        Unscribe(_controls.Player.SecondaryFire,control.SecondaryAttack);
        Unscribe(_controls.Player.Scroll,control.Scroll);
        Unscribe(_controls.Player.Look,control.Look);
    }
    public void SubscribePause(PauseGame pause)
    {
        _controls.Player.Pause.started += pause.KeyPressed;
        _controls.Player.Pause.performed += pause.KeyPressed;
        _controls.Player.Pause.canceled += pause.KeyPressed;
    }
    public void SubscribeRespawn(RespawnManager respawn)
    {
        _controls.Player.Respawn.started += respawn.KeyPressed;
        _controls.Player.Respawn.performed += respawn.KeyPressed;
        _controls.Player.Respawn.canceled += respawn.KeyPressed;
    }
    public void UpdateControl(CharacterControl control)
    {
        if (_playerControl != null)
            UnscribeAll(_playerControl);
        _playerControl = control;
        SubscribeAll(_playerControl);
    }
    private void Awake() {
        instance = this;
        _controls = new Secondclasshero();
        _playerInput = GetComponent<PlayerInput>();
    }
}
