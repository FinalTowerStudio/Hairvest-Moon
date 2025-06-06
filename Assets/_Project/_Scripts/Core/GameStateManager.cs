using System;
using UnityEngine;
namespace HairvestMoon.Core
{
    // Enum-driven state machine (Gameplay, Dialogue, Pause, etc.)
    // Broadcasts changes via OnGameStateChanged
    // Used to lock input, pause systems, manage cutscenes or tasks
    public class GameStateManager : MonoBehaviour, IBusListener
    {
        public GameState CurrentState { get; private set; }

        public bool IsInputLocked { get; private set; } = false;

        public void RegisterBusListeners()
        {
            var bus = ServiceLocator.Get<GameEventBus>();
            bus.GlobalSystemsInitialized += OnGlobalSystemsInitialized;
        }

        private void OnGlobalSystemsInitialized()
        {
            Initialize();
        }

        public void Initialize()
        {
            SetState(GameState.FreeRoam);
        }

        public void SetState(GameState newState)
        {
            if (newState == CurrentState) return;

            CurrentState = newState;
            ServiceLocator.Get<GameEventBus>().RaiseGameStateChanged(CurrentState);

            HandleTimeControl();
            HandleInputLock();
        }

        private void HandleTimeControl()
        {
            if (CurrentState == GameState.Menu || CurrentState == GameState.Dialogue || CurrentState == GameState.Cutscene)
                ServiceLocator.Get<GameTimeManager>().FreezeTime();
            else
                ServiceLocator.Get<GameTimeManager>().ResumeTime();
        }

        private void HandleInputLock()
        {
            bool shouldLock = (CurrentState != GameState.FreeRoam);
            if (shouldLock != IsInputLocked)
            {
                IsInputLocked = shouldLock;
                ServiceLocator.Get<GameEventBus>().RaiseInputLockChanged(IsInputLocked);
            }
        }

        public bool IsFreeRoam() => CurrentState == GameState.FreeRoam;
    }
}
