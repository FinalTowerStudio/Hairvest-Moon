using System;
using UnityEngine;
using UnityEngine.InputSystem;
namespace HairvestMoon.Core
{
    /// <summary>
    /// Centralized input manager that handles movement, look direction, and control mode switching.
    /// Supports automatic detection between Mouse and Gamepad.
    /// </summary>
    public enum ControlMode { Mouse, Gamepad }

    public class InputController : MonoBehaviour, InputSystem_Actions.IPlayerActions, IBusListener
    {
        [SerializeField] private PlayerInput playerInput;

        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }

        /// <summary> True if the look input was actively moved this frame. </summary>
        public bool LookInputThisFrame { get; private set; }

        public ControlMode CurrentMode { get; private set; } = ControlMode.Mouse;

        private InputSystem_Actions _input;

        private Vector2 _mouseLook;
        private Vector2 _gamepadLook;
        private Vector2 _lastGamepadLookDir = Vector2.right;

        private bool _inputLocked = false;

        private bool isInitialized = false;
        private GameEventBus _eventBus;

        public void RegisterBusListeners()
        {
            _eventBus = ServiceLocator.Get<GameEventBus>();
            _eventBus.GlobalSystemsInitialized += OnGlobalSystemsInitialized;
            _eventBus.InputLockChanged += OnInputLockChanged;
        }

        private void OnGlobalSystemsInitialized()
        {
            Initialize();
        }

        public void Initialize()
        {
            _input = new InputSystem_Actions();
            _input.Player.SetCallbacks(this);
            _input.Player.Enable();
            _input.Player.Pause.performed += OnPause;
            playerInput.onControlsChanged += HandleControlsChanged;
            isInitialized = true;
        }

        private void HandleControlsChanged(PlayerInput input)
        {
            var scheme = input.currentControlScheme;
            var newMode = scheme == "Gamepad" ? ControlMode.Gamepad : ControlMode.Mouse;

            if (newMode != CurrentMode)
            {
                Debug.Log($"[InputController] Switched control scheme: {scheme} → {newMode}");
                CurrentMode = newMode;
                _eventBus.RaiseControlModeChanged(CurrentMode);
            }
        }

        private void Update()
        {
            if (!isInitialized) return;

            LookInputThisFrame = false;

            if (_inputLocked) return;

            if (CurrentMode == ControlMode.Gamepad)
            {
                LookInput = _lastGamepadLookDir;

                if (_gamepadLook.sqrMagnitude > 0.1f)
                {
                    _lastGamepadLookDir = _gamepadLook;
                    LookInputThisFrame = true;
                }
            }
            else if (CurrentMode == ControlMode.Mouse)
            {
                LookInput = _mouseLook;

                if (Mouse.current != null && Mouse.current.delta.ReadValue().sqrMagnitude > 0.01f)
                {
                    LookInputThisFrame = true;
                    _eventBus.RaiseLookInputDetected();
                }
            }
        }

        private void OnInputLockChanged(InputLockChangedArgs args)
        {
            _inputLocked = args.Locked;
            Debug.Log($"[InputController] InputLockChanged: {args.Locked}");
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            MoveInput = context.ReadValue<Vector2>();
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            if (_inputLocked) return;

            if (Mouse.current != null)
            {
                _mouseLook = context.ReadValue<Vector2>();

                if (Mouse.current.delta.ReadValue().sqrMagnitude > 0.01f && CurrentMode != ControlMode.Mouse)
                {
                    Debug.Log("[InputController] Switching to Mouse due to movement.");
                    CurrentMode = ControlMode.Mouse;
                    _eventBus.RaiseControlModeChanged(CurrentMode);
                }
            }
        }

        public void OnGamepadLook(InputAction.CallbackContext context)
        {
            if (_inputLocked) return;

            Vector2 input = context.ReadValue<Vector2>();
            _gamepadLook = input;

            if (input.sqrMagnitude > 0.1f)
            {
                _lastGamepadLookDir = input;

                if (CurrentMode != ControlMode.Gamepad)
                {
                    Debug.Log("[InputController] Switching to Controller due to movement.");
                    CurrentMode = ControlMode.Gamepad;
                    _eventBus.RaiseControlModeChanged(CurrentMode);
                }
            }
        }

        public void OnAttack(InputAction.CallbackContext context) { }
        public void OnInteract(InputAction.CallbackContext context) { }
        public void OnCrouch(InputAction.CallbackContext context) { }
        public void OnJump(InputAction.CallbackContext context) { }
        public void OnNext(InputAction.CallbackContext context)
        {
            if (context.performed && !_inputLocked)
                _eventBus.RaiseToolNext();
        }

        public void OnPrevious(InputAction.CallbackContext context)
        {
            if (context.performed && !_inputLocked)
                _eventBus.RaiseToolPrevious();
        }

        public void OnSprint(InputAction.CallbackContext context) { }
        public void OnPause(InputAction.CallbackContext context)
        {
            if (context.performed)
                _eventBus.RaiseMenuToggle();
        }

    }
}