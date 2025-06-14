using HairvestMoon.Core;
using System.Collections.Generic;
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
        [SerializeField] private ParticleSystem cropReadyBurstPrefab;
        [SerializeField] private ParticleSystem readyPersistentPrefab; 

        private bool isInitialized = false;
        private FarmTileDataManager _farmTileDataManager;
        private Dictionary<Vector3Int, ParticleSystem> persistentReadyParticles = new();


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
            Vector3 worldPos = cropTilemap.CellToWorld(pos) + new Vector3(0.5f, 0.5f, 0f);
            // Burst effect (one-shot)
            if (cropReadyBurstPrefab != null)
                Instantiate(cropReadyBurstPrefab, worldPos, Quaternion.identity);

            // Persistent effect (only one per tile)
            if (readyPersistentPrefab != null && !persistentReadyParticles.ContainsKey(pos))
            {
                var persistentInstance = Instantiate(readyPersistentPrefab, worldPos, Quaternion.identity, cropTilemap.transform);
                persistentReadyParticles[pos] = persistentInstance;
            }
        }

        public void RemoveReadyParticle(Vector3Int pos)
        {
            if (persistentReadyParticles.TryGetValue(pos, out var ps))
            {
                if (ps != null) Destroy(ps.gameObject);
                persistentReadyParticles.Remove(pos);
            }
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
                    RemoveReadyParticle(pos);
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
                if (data.HasRipeCrop() && readyPersistentPrefab != null)
                {
                    Vector3 worldPos = cropTilemap.CellToWorld(pos) + new Vector3(0.5f, 0.5f, 0f);
                    if (!persistentReadyParticles.ContainsKey(pos))
                    {
                        var persistentInstance = Instantiate(readyPersistentPrefab, worldPos, Quaternion.identity, cropTilemap.transform);
                        persistentReadyParticles[pos] = persistentInstance;
                    }
                }
                else
                {
                    RemoveReadyParticle(pos); // Crop not ready, remove any stray persistent effect
                }
            }
        }
    }
}
