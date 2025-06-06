using HairvestMoon.Core;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace HairvestMoon.Farming
{
    /// <summary>
    /// Handles rendering crop sprites based on growth stages after growth tick occurs.
    /// </summary>
    public class CropVisualSystem : MonoBehaviour, IBusListener
    {
        private Tilemap cropTilemap;
        private bool isInitialized = false;

        public void RegisterBusListeners()
        {
            var bus = ServiceLocator.Get<GameEventBus>();
            bus.GlobalSystemsInitialized += OnGlobalSystemsInitialized;
            bus.TimeChanged += OnRefreshCropVisuals;
        }

        private void OnGlobalSystemsInitialized()
        {
            Initialize();
            isInitialized = true;
        }

        public void Initialize()
        {
            cropTilemap = ServiceLocator.Get<FarmTileDataManager>().CropTilemap;
            RefreshAllCrops();
        }

        private void OnRefreshCropVisuals(TimeChangedArgs args)
        {
            if (!isInitialized) return;
            RefreshAllCrops();
        }

        private void RefreshAllCrops()
        {
            foreach (var entry in ServiceLocator.Get<FarmTileDataManager>().AllTileData)
            {
                var pos = entry.Key;
                var data = entry.Value;

                if (data.plantedCrop == null)
                {
                    cropTilemap.SetTile(pos, null);
                    continue;
                }

                float growthPercent = data.GetGrowthProgressPercent();
                int stage = Mathf.Clamp(
                    (int)(growthPercent * data.plantedCrop.growthStages.Length),
                    0, data.plantedCrop.growthStages.Length - 1
                );

                Tile tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = data.plantedCrop.growthStages[stage];
                cropTilemap.SetTile(pos, tile);
            }
        }
    }
}
