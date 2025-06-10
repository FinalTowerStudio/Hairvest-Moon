using HairvestMoon.Core;
using UnityEngine;
using System;

namespace HairvestMoon.Player
{
    /// <summary>
    /// Determines the player's current facing direction based on movement or look input.
    /// Publishes an event on the GameEventBus whenever facing changes.
    /// </summary>
    public class PlayerFacingController : IBusListener
    {
        public enum FacingDirection { Up, Down, Left, Right }

        private FacingDirection _currentFacing = FacingDirection.Right;
        public FacingDirection CurrentFacing => _currentFacing;

        private FacingDirection _lastMoveFacing = FacingDirection.Right;
        private bool _isInLookMode = false;
        private float _lookStickyTimer = 0f;
        private const float LookStickyTime = 0.18f;

        // Dependencies
        private InputController _inputController;
        private GameEventBus _eventBus;
        private bool _isInitialized = false;

        public void RegisterBusListeners()
        {
            _eventBus = ServiceLocator.Get<GameEventBus>();
            _eventBus.GlobalSystemsInitialized += OnGlobalSystemsInitialized;
        }

        private void OnGlobalSystemsInitialized()
        {
            _inputController = ServiceLocator.Get<InputController>();
            _isInitialized = true;
        }

        /// <summary>
        /// Updates facing each frame. Stays in look direction until player actually moves.
        /// Publishes to the event bus if facing changes.
        /// </summary>
        public void UpdateFacing(Vector2 moveInput, Vector2 lookInput, ControlMode mode, Vector2 playerPosition)
        {
            if (!_isInitialized) return;

            bool isMoving = moveInput.sqrMagnitude > 0.01f;
            bool lookInputActive = _inputController != null && _inputController.LookInputThisFrame;
            bool lookInputHeld = lookInput.sqrMagnitude > 0.1f;

            FacingDirection newFacing = _currentFacing;

            if (isMoving)
            {
                _isInLookMode = false;
                _lookStickyTimer = 0f;
                newFacing = FromVector(moveInput);
                _lastMoveFacing = newFacing;
            }
            else if (lookInputHeld || lookInputActive)
            {
                _isInLookMode = true;
                _lookStickyTimer = LookStickyTime;
                Vector2 dir = (mode == ControlMode.Mouse)
                    ? ((Vector2)Camera.main.ScreenToWorldPoint(lookInput) - playerPosition)
                    : lookInput;
                if (dir.sqrMagnitude > 0.01f)
                    newFacing = FromVector(dir);
            }
            else if (_isInLookMode && _lookStickyTimer > 0f)
            {
                _lookStickyTimer -= Time.deltaTime;
                // Keep facing as is during sticky look
            }
            else
            {
                _isInLookMode = false;
                newFacing = _lastMoveFacing;
            }

            // If facing actually changed, update and publish to the event bus
            if (newFacing != _currentFacing)
            {
                _currentFacing = newFacing;
                _eventBus?.RaiseFacingChanged(_currentFacing);
            }
        }

        /// <summary>
        /// Forces the player's facing for scripted/cutscene use.
        /// </summary>
        public void ForceFacing(FacingDirection direction)
        {
            if (_currentFacing != direction)
            {
                _currentFacing = direction;
                _eventBus?.RaiseFacingChanged(_currentFacing);
            }
            _lastMoveFacing = direction;
            _isInLookMode = false;
            _lookStickyTimer = 0f;
        }

        private FacingDirection FromVector(Vector2 input)
        {
            if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
                return input.x > 0 ? FacingDirection.Right : FacingDirection.Left;
            else
                return input.y > 0 ? FacingDirection.Up : FacingDirection.Down;
        }

        public Vector3Int GetFacingOffset()
        {
            return _currentFacing switch
            {
                FacingDirection.Up => Vector3Int.up,
                FacingDirection.Down => Vector3Int.down,
                FacingDirection.Left => Vector3Int.left,
                FacingDirection.Right => Vector3Int.right,
                _ => Vector3Int.zero
            };
        }
    }
}
