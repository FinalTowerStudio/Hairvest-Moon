using HairvestMoon.Core;
using UnityEngine;

namespace HairvestMoon.Player
{
    [SelectionBase]
    public class Player_Controller : MonoBehaviour, IBusListener, ITickable
    {
        public Vector3 Position => transform.position;

        [Header("Physics")]
        [SerializeField] private Rigidbody2D _rb;

        // Cached managers
        private PlayerFacingController _facingController;
        private PlayerStateController _stateController;
        private InputController _inputController;
        private GameEventBus _eventBus;

        // Animation, movement, state
        private bool _isInitialized = false;
        private bool _canMove = true;
        private bool _isFrozen = false; // Used for cutscenes/menus
        private Vector2 _moveDir = Vector2.zero;

        // Stamina (example stat for Tick logic)
        private int _stamina = 100;
        private const int MaxStamina = 100;

        // Animation and rendering
        private Animator _animator => ServiceLocator.Get<PlayerStateController>().CurrentAnimator;
        private SpriteRenderer _spriteRenderer => ServiceLocator.Get<PlayerStateController>().CurrentSpriteRenderer;
        private float MoveSpeed => ServiceLocator.Get<PlayerStateController>().MoveSpeed;

        private int _currentAnimHash = 0;

        // Animation hash cache
        private static readonly int _animeIdleSide = Animator.StringToHash("AN_Character_Farmer_Idle_Side");
        private static readonly int _animeIdleUp = Animator.StringToHash("AN_Character_Farmer_Idle_Up");
        private static readonly int _animeIdleDown = Animator.StringToHash("AN_Character_Farmer_Idle_Down");
        private static readonly int _animeMoveSide = Animator.StringToHash("AN_Character_Farmer_Walk_Side");
        private static readonly int _animeMoveUp = Animator.StringToHash("AN_Character_Farmer_Walk_Up");
        private static readonly int _animeMoveDown = Animator.StringToHash("AN_Character_Farmer_Walk_Down");

        // --- Initialization and System Wiring ---

        public void RegisterBusListeners()
        {
            _eventBus = ServiceLocator.Get<GameEventBus>();
            _eventBus.GameStateChanged += OnGameStateChanged;
            _eventBus.ControlModeChanged += OnControlModeChanged;
            _eventBus.LookInputDetected += OnLookInputDetected;
            _eventBus.GlobalSystemsInitialized += OnGlobalSystemsInitialized;
        }

        private void OnGlobalSystemsInitialized()
        {
            _facingController = ServiceLocator.Get<PlayerFacingController>();
            _stateController = ServiceLocator.Get<PlayerStateController>();
            _inputController = ServiceLocator.Get<InputController>();

            _isInitialized = true;
            ServiceLocator.Get<GameTimeManager>().RegisterTickable(this);
        }

        // --- Core Update Cycle ---

        public void Tick(GameTimeChangedArgs args)
        {
            if (_stamina > 0)
            {
                _stamina = Mathf.Max(0, _stamina - 1);
                Debug.Log($"[Player_Controller] Stamina updated: {_stamina}/{MaxStamina}");
                // TODO: Raise event for stamina update/UI if needed
            }
        }

        private void Update()
        {
            if (!_isInitialized || _isFrozen) return;

            _moveDir = _canMove ? _inputController.MoveInput : Vector2.zero;

            if (_facingController == null) return;

            _facingController.UpdateFacing(
                _moveDir,
                _inputController.LookInput,
                _inputController.CurrentMode
            );

            if (_stateController.IsFormInitialized)
                UpdateAnimation(_facingController.CurrentFacing);
        }

        private void FixedUpdate()
        {
            if (!_isInitialized || _isFrozen) return;
            if (_stateController.IsFormInitialized && _canMove)
            {
                Vector2 move = _moveDir.normalized * MoveSpeed * Time.fixedDeltaTime;
                _rb.MovePosition(_rb.position + move);
            }
        }

        // --- Event Handling ---

        private void OnGameStateChanged(GameStateChangedArgs args)
        {
            var newState = args.State;
            _canMove = newState == GameState.FreeRoam;
            _isFrozen = (newState == GameState.Paused || newState == GameState.Cutscene || newState == GameState.Dialogue);
        }

        private void OnControlModeChanged(ControlModeChangedArgs args)
        {
            // TODO: Visual feedback for switching controller/mouse if desired
        }

        private void OnLookInputDetected()
        {
            // Optional: Haptic feedback, aim highlight, etc.
        }

        // --- Animation Logic ---

        private void UpdateAnimation(PlayerFacingController.FacingDirection facing)
        {
            if (_animator == null || _spriteRenderer == null) return;

            switch (facing)
            {
                case PlayerFacingController.FacingDirection.Left: _spriteRenderer.flipX = true; break;
                case PlayerFacingController.FacingDirection.Right: _spriteRenderer.flipX = false; break;
            }

            bool isMoving = _moveDir.sqrMagnitude > 0.01f;
            int anim = _animeIdleSide;

            switch (facing)
            {
                case PlayerFacingController.FacingDirection.Up:
                    anim = isMoving ? _animeMoveUp : _animeIdleUp; break;
                case PlayerFacingController.FacingDirection.Down:
                    anim = isMoving ? _animeMoveDown : _animeIdleDown; break;
                case PlayerFacingController.FacingDirection.Left:
                case PlayerFacingController.FacingDirection.Right:
                    anim = isMoving ? _animeMoveSide : _animeIdleSide; break;
            }

            if (anim != _currentAnimHash)
            {
                _animator.CrossFade(anim, 0);
                _currentAnimHash = anim;
            }
        }

        // --- Expansion Points ---

        // public void FreezePlayer(bool freeze) { _isFrozen = freeze; }
        // public void RestoreStamina() { _stamina = MaxStamina; }
        // public void OnPlayerDamaged() { /* Add event logic */ }
        // public void OnItemPickup(ItemData item) { /* Add event logic */ }
    }
}
