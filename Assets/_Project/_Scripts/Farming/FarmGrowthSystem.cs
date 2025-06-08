using HairvestMoon.Core;

namespace HairvestMoon.Farming
{
    /// <summary>
    /// Handles all crop growth logic. Advances planted crops each minute (if watered).
    /// </summary>
    public class FarmGrowthSystem : IBusListener
    {
        private bool isInitialized = false;
        private FarmTileDataManager _farmTileDataManager;
        private GameEventBus _eventBus;
        private CropVisualSystem _cropVisualSystem;

        public void RegisterBusListeners()
        {
            _eventBus = ServiceLocator.Get<GameEventBus>();
            _eventBus.GlobalSystemsInitialized += OnGlobalSystemsInitialized;
            _eventBus.TimeChanged += OnMinuteGrowthTick;
        }

        private void OnGlobalSystemsInitialized()
        {
            _farmTileDataManager = ServiceLocator.Get<FarmTileDataManager>();
            _cropVisualSystem = ServiceLocator.Get<CropVisualSystem>();
            isInitialized = true;
        }

        /// <summary>
        /// Advance growth for all planted, watered crops by one minute.
        /// </summary>
        private void OnMinuteGrowthTick(GameTimeChangedArgs args)
        {
            if (!isInitialized) return;

            foreach (var entry in _farmTileDataManager.AllTileData)
            {
                var data = entry.Value;
                if (data.plantedCrop == null) continue;
                if (!data.isWatered) continue;

                data.wateredMinutesAccumulated++;
                if (data.wateredMinutesAccumulated > data.plantedCrop.growthDurationMinutes)
                    data.wateredMinutesAccumulated = data.plantedCrop.growthDurationMinutes;

                // Check for just completed
                if (data.IsJustFullyGrown())
                {
                    _cropVisualSystem.TriggerCropCompletedVFX(entry.Key);
                    // Optional: _eventBus.RaiseCropFullyGrown(entry.Key);
                }
            }
        }

    }
}
