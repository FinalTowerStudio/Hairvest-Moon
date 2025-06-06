using System;
using UnityEngine;

namespace HairvestMoon.Core
{
    // Manages in-game clock, fires events on dawn and dusk
    // Notifies listeners every in-game minute
    // Provides GetFormattedTime() and IsNight() helpers

    public class GameTimeManager : MonoBehaviour, IBusListener
    {
        [Header("Time Settings")]
        [SerializeField] private float secondsPerGameMinute = 1f;
        [SerializeField] private int dawnHour = 6;
        [SerializeField] private int duskHour = 18;

        public int CurrentHour { get; private set; } = 6;
        public int CurrentMinute { get; private set; } = 0;
        public int Day { get; private set; } = 1;
        public bool IsTimeFrozen { get; private set; } = false;
        public float TimeScale { get; private set; } = 1f;

        private float _timer;
        private bool _isNight = false;
        private bool isInitialized = false;

        public void RegisterBusListeners()
        {
            var bus = ServiceLocator.Get<GameEventBus>();
            bus.GlobalSystemsInitialized += OnGlobalSystemsInitialized;
        }

        private void OnGlobalSystemsInitialized()
        {
            isInitialized = true;
        }

        private void Update()
        {
            if (!isInitialized || IsTimeFrozen) return;

            _timer += Time.deltaTime * TimeScale;
            if (_timer >= secondsPerGameMinute)
            {
                _timer = 0f;
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

            ServiceLocator.Get<GameEventBus>().RaiseTimeChanged(CurrentHour, CurrentMinute);
        }


        public void AdvanceDay()
        {
            Day++;
            ServiceLocator.Get<GameEventBus>().RaiseDawn();
        }

        private void CheckTimeTriggers()
        {
            if (!_isNight && CurrentHour >= duskHour)
            {
                _isNight = true;
                ServiceLocator.Get<GameEventBus>().RaiseDusk();
            }
            else if (_isNight && CurrentHour >= dawnHour && CurrentHour < duskHour)
            {
                _isNight = false;
                ServiceLocator.Get<GameEventBus>().RaiseDawn();
            }
        }

        public void SetTimeScale(float scale)
        {
            TimeScale = Mathf.Max(0f, scale);
        }

        public void FastForwardToHour(int targetHour)
        {
            while (CurrentHour != targetHour)
            {
                AdvanceMinute(); // or AdvanceHour() if you make that helper
            }
        }


        public float GetCurrentHourProgress()
        {
            return (float)CurrentMinute / 60f;
        }

        public string GetFormattedTime() => $"Day {Day} - {CurrentHour:00}:{CurrentMinute:00}";

        public void FreezeTime() => IsTimeFrozen = true;
        public void ResumeTime() => IsTimeFrozen = false;
        public bool IsNight() => _isNight;
        public bool IsMorning() => CurrentHour >= 6 && CurrentHour < 12;
        public bool IsEvening() => CurrentHour >= 17 && CurrentHour < 20;


    }
}