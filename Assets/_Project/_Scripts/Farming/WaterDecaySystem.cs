using HairvestMoon.Core;
using UnityEngine;

namespace HairvestMoon.Farming
{
    /// <summary>
    /// Handles decay of water on farm tiles. Unwaters tiles as time passes.
    /// </summary>
    public class WaterDecaySystem : IBusListener
    {
        private bool isInitialized = false;
        private FarmTileDataManager _farmTileDataManager;
        private WaterVisualSystem _waterVisualSystem;
        private GameEventBus _eventBus;

        public void RegisterBusListeners()
        {
            _eventBus = ServiceLocator.Get<GameEventBus>();
            _eventBus.GlobalSystemsInitialized += OnGlobalSystemsInitialized;
            _eventBus.TimeChanged += OnWaterDecayTick;
        }

        private void OnGlobalSystemsInitialized()
        {
            _farmTileDataManager = ServiceLocator.Get<FarmTileDataManager>();
            _waterVisualSystem = ServiceLocator.Get<WaterVisualSystem>();
            isInitialized = true;
        }

        private void OnWaterDecayTick(GameTimeChangedArgs args)
        {
            if (!isInitialized) return;

            foreach (var entry in _farmTileDataManager.AllTileData)
            {
                var pos = entry.Key;
                var data = entry.Value;

                if (!data.isWatered) continue;

                data.waterMinutesRemaining--;

                if (data.waterMinutesRemaining <= 0)
                {
                    data.isWatered = false;
                    data.waterMinutesRemaining = 0;
                    _farmTileDataManager.UpdateWaterVisual(pos, data);
                    _waterVisualSystem.HandleWateredTile(pos, data);

                    // // Optional: If you want crops to wither after drying out
                    // if (data.plantedCrop != null && !data.HasRipeCrop())
                    // {
                    //     data.isWithered = true;
                    //     // You could fire a wither event or update visuals here.
                    // }
                }
            }
        }
    }
}
