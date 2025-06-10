using UnityEngine;
using HairvestMoon.Core;
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
        [Header("Debug Controls")]
        [Tooltip("Tick is auto-paused in menu/cutscene unless forced running by this flag.")]
        [SerializeField] private bool _forceTick = false;

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
            if (_autoPaused && !_forceTick && !_hasStepped) return;

            _gameTimeManager.Tick(Time.deltaTime);

            if (_hasStepped)
                _hasStepped = false; // Reset one-step after ticking once
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

        public bool IsTicking => !_autoPaused || _forceTick;

        // Optionally expose Pause/Resume methods for runtime scripting or debug overlay UI.
    }
}
