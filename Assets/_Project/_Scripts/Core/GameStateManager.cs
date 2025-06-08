using System;
using UnityEngine;
namespace HairvestMoon.Core
{
    // Enum-driven state machine (Gameplay, Dialogue, Pause, etc.)
    // Broadcasts changes via OnGameStateChanged
    // Used to lock input, pause systems, manage cutscenes or tasks
    public class GameStateManager : IBusListener
    {
        public GameState CurrentState { get; private set; }
        public bool IsInputLocked { get; private set; } = false;

        private bool isInitialized = false;
        private GameEventBus _eventBus;
        private GameTimeManager _timeManager;

        public void RegisterBusListeners()
        {
            _eventBus = ServiceLocator.Get<GameEventBus>();
            _eventBus.GlobalSystemsInitialized += OnGlobalSystemsInitialized;
        }

        private void OnGlobalSystemsInitialized()
        {
            Initialize();
            isInitialized = true;
        }

        public void Initialize()
        {
            _timeManager = ServiceLocator.Get<GameTimeManager>();
            // Set the initial game state to FreeRoam for now, if we want to change it later here is the place.
            SetStateInternal(GameState.FreeRoam);
        }

        public bool RequestStateChange(GameState newState)
        {
            // Insert guards, conditions, or debugging here as we expand!
            return SetStateInternal(newState);
        }

        private bool SetStateInternal(GameState newState)
        {
            if (newState == CurrentState) return false;

            CurrentState = newState;
            _eventBus.RaiseGameStateChanged(CurrentState);

            HandleTimeControl();
            HandleInputLock();
            return true;
        }

        private void HandleTimeControl()
        {
            if (CurrentState == GameState.Menu || CurrentState == GameState.Dialogue || CurrentState == GameState.Cutscene)
                _timeManager?.FreezeTime();
            else
                _timeManager?.ResumeTime();
        }

        private void HandleInputLock()
        {
            bool shouldLock = (CurrentState != GameState.FreeRoam);
            if (shouldLock != IsInputLocked)
            {
                IsInputLocked = shouldLock;
                _eventBus.RaiseInputLockChanged(IsInputLocked);
            }
        }

        public bool IsFreeRoam() => CurrentState == GameState.FreeRoam;
    }
}
