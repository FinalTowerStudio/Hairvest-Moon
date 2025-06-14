using UnityEngine;
using System;

namespace HairvestMoon.Core
{
    /// <summary>
    /// Drives the game time tick loop. 
    /// Auto-pauses in menu/cutscene states. 
    /// Can be controlled in the Inspector for debug/step.
    /// </summary>
    public class GameTimeDriver : MonoBehaviour, IBusListener
    {
#if UNITY_EDITOR
        [Header("Debug Time Controls")]
        [Range(0.1f, 100f)]
        [SerializeField] private float debugTimeScale = 1f;
        [Tooltip("Tick is auto-paused in menu/cutscene unless forced running by this flag.")]
        [SerializeField] private bool _forceTick = false;
#endif

        private bool _canTick = false;
        private GameTimeManager _gameTimeManager;
        private GameEventBus _eventBus;

        private bool _autoPaused = false;
        private bool _hasStepped = false;

        public void RegisterBusListeners()
        {
            _eventBus = ServiceLocator.Get<GameEventBus>();
            _eventBus.GlobalSystemsInitialized += OnGlobalSystemsInitialized;
            _eventBus.GameStateChanged += OnGameStateChanged;
        }

        private void OnGlobalSystemsInitialized()
        {
            _gameTimeManager = ServiceLocator.Get<GameTimeManager>();
            _canTick = true;
        }

        private void OnGameStateChanged(GameStateChangedArgs args)
        {
            // Pause in Pause/Menu/Cutscene/Dialog states, play in FreeRoam
            switch (args.State)
            {
                case GameState.Paused:
                case GameState.Cutscene:
                case GameState.Dialogue:
                    _autoPaused = true;
                    break;
                case GameState.FreeRoam:
                default:
                    _autoPaused = false;
                    break;
            }
        }

        private void Update()
        {
            if (!_canTick) return;

#if UNITY_EDITOR
            var timeManager = ServiceLocator.Get<GameTimeManager>();
            if (timeManager != null && Mathf.Abs(debugTimeScale - timeManager.TimeScale) > 0.01f)
                timeManager.SetTimeScale(debugTimeScale);
#endif

            // Only tick if not auto-paused, unless forced for debug
            if (IsTicking)
            {
                _gameTimeManager?.Tick(Time.deltaTime);
            }
            else if (_hasStepped)
            {
                _gameTimeManager?.Tick(Time.deltaTime);
                _hasStepped = false;
            }
        }

        // --- Inspector/Editor Control ---

        [ContextMenu("Pause Ticking (Inspector)")]
        public void PauseTicking() { _forceTick = false; }

        [ContextMenu("Resume Ticking (Inspector)")]
        public void ResumeTicking() { _forceTick = true; }

        [ContextMenu("Step One Tick (Inspector)")]
        public void StepOneTick()
        {
            _hasStepped = true;
        }

        public bool IsTicking => (!_autoPaused) || _forceTick;

        // Optionally expose Pause/Resume methods for runtime scripting or debug overlay UI.
    }
}
