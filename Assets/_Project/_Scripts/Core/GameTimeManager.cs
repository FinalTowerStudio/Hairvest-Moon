using System;
using System.Collections.Generic;
using UnityEngine;

namespace HairvestMoon.Core
{
    // Manages in-game clock, fires events on dawn and dusk
    // Notifies listeners every in-game minute
    // Provides GetFormattedTime() and IsNight() helpers

    public class GameTimeManager : IBusListener
    {
        private readonly float secondsPerGameMinute = 1f;
        private readonly int dawnHour = 6;
        private readonly int duskHour = 18;

        public int CurrentHour { get; private set; } = 6;
        public int CurrentMinute { get; private set; } = 0;
        public int Day { get; private set; } = 1;
        public float TimeScale { get; private set; } = 1f;

        private float _timer;
        private bool _isNight = false;
        private bool isInitialized = false;
        private bool isTimeFrozen = false;
        private GameEventBus _eventBus;

        private List<ITickable> tickables = new List<ITickable>();

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
            //Could eventually support loading from save by overriding Initialize() with custom hour/minute/day values.
            CurrentHour = 6;
            CurrentMinute = 0;
            _timer = 0;
            _isNight = false;
            isTimeFrozen = false;
        }

        public void RegisterTickable(ITickable tickable)
        {
            if (!tickables.Contains(tickable))
                tickables.Add(tickable);
        }

        public void UnregisterTickable(ITickable tickable)
        {
            tickables.Remove(tickable);
            //consider logging when trying to remove a non existent tickable
        }

        public void Tick(float deltaTime)
        {
            if (!isInitialized || isTimeFrozen) return;

            _timer += deltaTime * TimeScale;
            while (_timer >= secondsPerGameMinute)
            {
                _timer -= secondsPerGameMinute;
                AdvanceMinute();
            }
        }

        private void AdvanceMinute()
        {
            CurrentMinute++;
            if (CurrentMinute >= 60)
            {
                CurrentMinute = 0;
                CurrentHour++;

                if (CurrentHour >= 24)
                {
                    CurrentHour = 0;
                    AdvanceDay();
                }

                CheckTimeTriggers();
            }

            var args = new GameTimeChangedArgs(CurrentHour, CurrentMinute, Day);

            foreach (var tickable in tickables)
                tickable.Tick(args);

            _eventBus.RaiseTimeChanged(args);
        }


        public void AdvanceDay()
        {
            Day++;
            _eventBus.RaiseNewDay();  
        }

        private void CheckTimeTriggers()
        {
            if (!_isNight && CurrentHour >= duskHour)
            {
                _isNight = true;
                _eventBus.RaiseDusk();
            }
            else if (_isNight && CurrentHour >= dawnHour && CurrentHour < duskHour)
            {
                _isNight = false;
                _eventBus.RaiseDawn();
            }
        }

        public void SetTimeScale(float scale)
        {
            TimeScale = Mathf.Max(0f, scale);
        }

        public void FreezeTime() => isTimeFrozen = true;
        public void ResumeTime() => isTimeFrozen = false;

        public string GetFormattedTime() => $"Day {Day} - {CurrentHour:00}:{CurrentMinute:00}";
    }
}