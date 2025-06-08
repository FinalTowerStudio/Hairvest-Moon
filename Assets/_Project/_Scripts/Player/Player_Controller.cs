using HairvestMoon.Core;
using UnityEngine;

namespace HairvestMoon.Player
{
    [SelectionBase]
    public class Player_Controller : MonoBehaviour, IBusListener, ITickable
    {
        public Vector3 Position => transform.position;

        [Header("Dependencies")]
        [SerializeField] private Rigidbody2D _rb;

        private bool _isInitialized = false;
        private bool _canMove = true;
        private bool _isFrozen = false; // For cutscenes, menus, etc.
        private Vector2 _moveDir = Vector2.zero;
        private PlayerFacingController _facingController;
        private PlayerStateController _stateController;
        private InputController _inputController;
        private GameEventBus _eventBus;

        private int _stamina = 100;
        private const int MaxStamina = 100;

        private Animator _animator => ServiceLocator.Get<PlayerStateController>().CurrentAnimator;
        private SpriteRenderer _spriteRenderer => ServiceLocator.Get<PlayerStateController>().CurrentSpriteRenderer;
        private float MoveSpeed => ServiceLocator.Get<PlayerStateController>().MoveSpeed;

        private int _currentAnimHash = 0;

        // Anim hashes
        private readonly int _animeIdleSide = Animator.StringToHash("AN_Character_Farmer_Idle_Side");
        private readonly int _animeIdleUp = Animator.StringToHash("AN_Character_Farmer_Idle_Up");
        private readonly int _animeIdleDown = Animator.StringToHash("AN_Character_Farmer_Idle_Down");
        private readonly int _animeMoveSide = Animator.StringToHash("AN_Character_Farmer_Walk_Side");
        private readonly int _animeMoveUp = Animator.StringToHash("AN_Character_Farmer_Walk_Up");
        private readonly int _animeMoveDown = Animator.StringToHash("AN_Character_Farmer_Walk_Down");

        public void Initialize()
        {
            _facingController = ServiceLocator.Get<PlayerFacingController>();
            _stateController = ServiceLocator.Get<PlayerStateController>();
            _inputController = ServiceLocator.Get<InputController>();
            _isInitialized = true;
        }

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
            Initialize();
            ServiceLocator.Get<GameTimeManager>().RegisterTickable(this);
        }

        public void Tick(GameTimeChangedArgs args)
        {
            // Example: Drain stamina (expand as needed)
            if (_stamina > 0)
            {
                _stamina = Mathf.Max(0, _stamina - 1);
                // Fire stamina update event, trigger low stamina, etc.
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

        private void OnGameStateChanged(GameStateChangedArgs args)
        {
            var newState = args.State;
            _canMove = newState == GameState.FreeRoam;
            _isFrozen = newState == GameState.Paused || newState == GameState.Cutscene || newState == GameState.Dialogue;
        }

        private void OnControlModeChanged(ControlModeChangedArgs args) { /* Add feedback if needed */ }

        private void OnLookInputDetected() { /* Optional: additional facing/feedback logic */ }

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
    }
}
