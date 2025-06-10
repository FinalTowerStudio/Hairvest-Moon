using HairvestMoon.Core;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace HairvestMoon.Farming
{
    /// <summary>
    /// Updates the visuals of all crops on the farm grid as crops grow.
    /// </summary>
    public class CropVisualSystem : MonoBehaviour, IBusListener
    {
        [SerializeField] private Tilemap cropTilemap;
        [SerializeField] private GameObject cropReadyParticlePrefab;
        [SerializeField] private GameObject readyHighlightPrefab; //using prefab right now for simple overlay or particle effect

        private bool isInitialized = false;
        private FarmTileDataManager _farmTileDataManager;

        public void RegisterBusListeners()
        {
            var bus = ServiceLocator.Get<GameEventBus>();
            bus.GlobalSystemsInitialized += OnGlobalSystemsInitialized;
            bus.TimeChanged += OnRefreshCropVisuals;
        }

        private void OnGlobalSystemsInitialized()
        {
            _farmTileDataManager = ServiceLocator.Get<FarmTileDataManager>();
            isInitialized = true;
            RefreshAllCrops();
        }

        public void TriggerCropCompletedVFX(Vector3Int pos)
        {
            // Place a particle at the crop's tile
            Vector3 worldPos = cropTilemap.CellToWorld(pos) + new Vector3(0.5f, 0.5f, 0f);
            if (cropReadyParticlePrefab != null)
                Instantiate(cropReadyParticlePrefab, worldPos, Quaternion.identity);

            // Optional: place highlight overlay
            if (readyHighlightPrefab != null)
                Instantiate(readyHighlightPrefab, worldPos, Quaternion.identity, cropTilemap.transform);
        }

        private void OnRefreshCropVisuals(GameTimeChangedArgs args)
        {
            if (!isInitialized) return;
            RefreshAllCrops();
        }

        private void RefreshAllCrops()
        {
            foreach (var entry in _farmTileDataManager.AllTileData)
            {
                var pos = entry.Key;
                var data = entry.Value;

                if (data.plantedCrop == null)
                {
                    cropTilemap.SetTile(pos, null);
                    // Optionally remove highlight/particle here if we pool them
                    continue;
                }

                var growthStages = data.plantedCrop.growthStages;
                if (growthStages == null || growthStages.Length == 0)
                {
                    cropTilemap.SetTile(pos, null);
                    continue;
                }

                // If future withered
                // if (data.isWithered && data.plantedCrop.witheredSprite != null)
                // {
                //     // set to withered tile
                //     continue;
                // }

                float growthPercent = data.GetGrowthProgressPercent();
                int stage = Mathf.Clamp(
                    Mathf.FloorToInt(growthPercent * growthStages.Length),
                    0, growthStages.Length - 1
                );

                Tile tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = growthStages[stage];
                cropTilemap.SetTile(pos, tile);

                // Add highlight if crop is fully grown
                if (data.HasRipeCrop() && readyHighlightPrefab != null)
                {
                    Vector3 worldPos = cropTilemap.CellToWorld(pos) + new Vector3(0.5f, 0.5f, 0f);
                    Instantiate(readyHighlightPrefab, worldPos, Quaternion.identity, cropTilemap.transform);
                }
            }
        }
    }
}
