using HairvestMoon.Core;
using UnityEngine;

namespace HairvestMoon.Player
{
    /// <summary>
    /// Determines the player's current facing direction based on movement or look input.
    /// Prioritizes movement, and defers to look input only after intentional use.
    /// </summary>
    public class PlayerFacingController : MonoBehaviour, IBusListener
    {
        public enum FacingDirection { Up, Down, Left, Right }
        public FacingDirection CurrentFacing { get; private set; } = FacingDirection.Right;

        private FacingDirection _lastMoveFacing = FacingDirection.Right;
        private FacingSource _currentSource = FacingSource.Movement;

        private InputController _inputController;
        private bool _isInitialized = false;

        // --- System Initialization ---
        public void RegisterBusListeners()
        {
            ServiceLocator.Get<GameEventBus>().GlobalSystemsInitialized += OnGlobalSystemsInitialized;
        }

        private void OnGlobalSystemsInitialized()
        {
            _inputController = ServiceLocator.Get<InputController>();
            _isInitialized = true;
        }

        /// <summary>
        /// Updates the facing direction each frame based on input and control mode.
        /// Should be called by Player_Controller in Update().
        /// </summary>
        public void UpdateFacing(Vector2 moveInput, Vector2 lookInput, ControlMode mode)
        {
            if (!_isInitialized) return;

            bool isMoving = moveInput.sqrMagnitude > 0.01f;
            bool lookInputActive = _inputController != null && _inputController.LookInputThisFrame;

            if (isMoving)
            {
                _currentSource = FacingSource.Movement;
                CurrentFacing = FromVector(moveInput);
                _lastMoveFacing = CurrentFacing;
                return;
            }

            if (_currentSource == FacingSource.Movement && lookInputActive)
                _currentSource = FacingSource.Look;

            if (_currentSource == FacingSource.Look && lookInputActive)
            {
                Vector2 direction;
                if (mode == ControlMode.Mouse)
                {
                    Vector2 worldMouse = Camera.main.ScreenToWorldPoint(lookInput);
                    direction = worldMouse - (Vector2)transform.position;
                }
                else
                {
                    direction = lookInput;
                }

                if (direction.sqrMagnitude > 0.01f)
                    CurrentFacing = FromVector(direction);
            }
            else
            {
                CurrentFacing = _lastMoveFacing;
            }
        }

        /// <summary>
        /// Instantly set the player's facing for cutscenes, scripted events, etc.
        /// </summary>
        public void ForceFacing(FacingDirection direction)
        {
            CurrentFacing = direction;
            _lastMoveFacing = direction;
            _currentSource = FacingSource.Movement;
        }

        /// <summary>
        /// Converts a vector2 to one of the four cardinal facings.
        /// </summary>
        private FacingDirection FromVector(Vector2 input)
        {
            if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
                return input.x > 0 ? FacingDirection.Right : FacingDirection.Left;
            else
                return input.y > 0 ? FacingDirection.Up : FacingDirection.Down;
        }

        /// <summary>
        /// Gets an integer grid offset for use in target selection or interactions.
        /// </summary>
        public Vector3Int GetFacingOffset()
        {
            return CurrentFacing switch
            {
                FacingDirection.Up => Vector3Int.up,
                FacingDirection.Down => Vector3Int.down,
                FacingDirection.Left => Vector3Int.left,
                FacingDirection.Right => Vector3Int.right,
                _ => Vector3Int.zero
            };
        }

        private enum FacingSource
        {
            Movement,
            Look
        }
    }
}