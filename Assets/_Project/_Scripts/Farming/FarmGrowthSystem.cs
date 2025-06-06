using HairvestMoon.Core;
using UnityEngine;

namespace HairvestMoon.Farming
{
    /// <summary>
    /// Handles daily crop growth only.
    /// Subscribes to GameTimeManager.OnDawn to tick crops once per day.
    /// </summary>
    public class FarmGrowthSystem : MonoBehaviour, IBusListener
    {
        private bool isInitialized = false;

        public void RegisterBusListeners()
        {
            var bus = ServiceLocator.Get<GameEventBus>();
            bus.GlobalSystemsInitialized += OnGlobalSystemsInitialized;
            bus.TimeChanged += OnMinuteGrowthTick;
        }

        private void OnGlobalSystemsInitialized()
        {
            isInitialized = true;
        }

        private void OnMinuteGrowthTick(TimeChangedArgs args)
        {
            if (!isInitialized) return;

            foreach (var entry in ServiceLocator.Get<FarmTileDataManager>().AllTileData)
            {
                var pos = entry.Key;
                var data = entry.Value;

                if (data.plantedCrop != null)
                {
                    data.wateredMinutesAccumulated += 1;

                    if (data.wateredMinutesAccumulated >= data.plantedCrop.growthDurationMinutes)
                    {
                        data.wateredMinutesAccumulated = data.plantedCrop.growthDurationMinutes;
                    }
                }
            }
        }

    }

}