using HairvestMoon.Core;
using UnityEngine;

namespace HairvestMoon.Player
{
    /// <summary>
    /// Determines the player's facing direction based on *explicit* control mode:
    /// - In Keyboard mode: faces in last nonzero movement direction.
    /// - In Gamepad mode: faces in right stick direction if held, otherwise last faced.
    /// </summary>
    public class PlayerFacingController : IBusListener
    {
        public enum FacingDirection { Up, Down, Left, Right }

        private FacingDirection _currentFacing = FacingDirection.Right;
        public FacingDirection CurrentFacing => _currentFacing;

        private GameEventBus _eventBus;
        private bool _isInitialized = false;

        public void RegisterBusListeners()
        {
            _eventBus = ServiceLocator.Get<GameEventBus>();
            _eventBus.GlobalSystemsInitialized += OnGlobalSystemsInitialized;
        }

        private void OnGlobalSystemsInitialized()
        {
            _isInitialized = true;
        }

        /// <summary>
        /// Updates facing each frame using explicit ControlMode.
        /// </summary>
        public void UpdateFacing(Vector2 moveInput, Vector2 lookInput, Vector2 playerPosition)
        {
            if (!_isInitialized) return;
            FacingDirection newFacing = _currentFacing;

            if (moveInput.sqrMagnitude > 0.01f)
            {
                newFacing = FromVector(moveInput);
            }

            if (newFacing != _currentFacing)
            {
                _currentFacing = newFacing;
                _eventBus?.RaiseFacingChanged(_currentFacing);
            }
        }

        public void ForceFacing(FacingDirection direction)
        {
            if (_currentFacing != direction)
            {
                _currentFacing = direction;
                _eventBus?.RaiseFacingChanged(_currentFacing);
            }
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
