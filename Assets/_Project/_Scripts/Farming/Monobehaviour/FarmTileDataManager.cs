using HairvestMoon.Core;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

namespace HairvestMoon.Farming
{
    public class FarmTileDataManager : MonoBehaviour, IBusListener
    {
        [Header("Tilemaps")]
        [SerializeField] private Tilemap baseFarmTilemap;
        [SerializeField] private Tilemap tilledOverlayTilemap;
        [SerializeField] private Tilemap overlayFarmTilemap;  

        [Header("Tiles")]
        [SerializeField] private TileBase tilledOverlayTile;
        [SerializeField] private TileBase wateredOverlayTile;

        private readonly Dictionary<Vector3Int, FarmTileData> tileData = new();
        public IReadOnlyDictionary<Vector3Int, FarmTileData> AllTileData => tileData;

        private GameEventBus _eventBus;
        private bool isInitialized = false;

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
            tileData.Clear();
            tilledOverlayTilemap.ClearAllTiles();
            overlayFarmTilemap.ClearAllTiles();
        }

        public FarmTileData GetTileData(Vector3Int pos)
        {
            if (!tileData.ContainsKey(pos))
                tileData[pos] = new FarmTileData(pos);
            return tileData[pos];
        }

        public bool IsFarmTile(Vector3Int cell)
        {
            return baseFarmTilemap != null && baseFarmTilemap.HasTile(cell);
        }

        public bool CanTill(Vector3Int cell)
        {
            return IsFarmTile(cell) && !IsTilled(cell) && !HasCrop(cell);
        }
        public bool IsTilled(Vector3Int cell) => GetTileData(cell).isTilled;
        public bool HasCrop(Vector3Int cell) => GetTileData(cell).plantedCrop != null;

        /// <summary>
        /// Tills or untils a tile: overlays the tilled visual, updates data, and fires event.
        /// </summary>
        public void SetTilled(Vector3Int pos, bool isTilled)
        {
            var data = GetTileData(pos);
            data.isTilled = isTilled;

            if (!isTilled)
            {
                data.plantedCrop = null;
                data.wateredMinutesAccumulated = 0f;
                data.isWatered = false;
                data.waterMinutesRemaining = 0;
            }

            // Overlay tile: shows/hides "tilled" without altering baseFarmTilemap
            tilledOverlayTilemap.SetTile(pos, isTilled ? tilledOverlayTile : null);

            UpdateWaterVisual(pos, data);
            // Optional: _eventBus.RaiseTileTilled(pos);
        }

        public bool CanWater(Vector3Int cell)
        {
            var data = GetTileData(cell);
            return IsFarmTile(cell) && data.isTilled && !data.isWatered;
        }
        public bool CanPlant(Vector3Int cell, SeedData seed)
        {
            var data = GetTileData(cell);
            return IsFarmTile(cell) && data.isTilled && !data.isWatered && data.plantedCrop == null && seed != null;
        }
        public bool CanHarvest(Vector3Int cell)
        {
            var data = GetTileData(cell);
            return IsFarmTile(cell) && data.HasRipeCrop();
        }

        public void PlantSeed(Vector3Int cell, SeedData seed)
        {
            var data = GetTileData(cell);
            data.plantedCrop = seed?.cropData;
            data.wateredMinutesAccumulated = 0f;
            // Optionally: trigger visuals/event
        }

        public void HarvestCrop(Vector3Int cell)
        {
            var data = GetTileData(cell);

            data.plantedCrop = null;
            data.wateredMinutesAccumulated = 0f;
            data.isWatered = false;

            // New: Reset to untilled
            data.isTilled = false;
            SetTilled(cell, false);
        }

        public void SetWatered(Vector3Int pos, bool watered)
        {
            var data = GetTileData(pos);
            if (!data.isTilled && watered)
            {
                Debug.LogWarning($"Tried to water untilled tile at {pos}.");
                return;
            }

            data.isWatered = watered;
            data.waterMinutesRemaining = watered ? FarmTileData.MinutesPerWatering : 0;
            UpdateWaterVisual(pos, data);
            // Optional: _eventBus.RaiseTileWatered(pos);
        }

        public void UpdateWaterVisual(Vector3Int pos, FarmTileData data)
        {
            if (!data.isTilled)
            {
                overlayFarmTilemap.SetTile(pos, null);
                return;
            }
            overlayFarmTilemap.SetTile(pos, data.isWatered ? wateredOverlayTile : null);
        }

        public void RemoveTile(Vector3Int pos)
        {
            tileData.Remove(pos);
            tilledOverlayTilemap.SetTile(pos, null);
            overlayFarmTilemap.SetTile(pos, null);
            // baseFarmTilemap.SetTile(pos, null); // Only if you want to permanently remove ground!
        }
    }

    [System.Serializable]
    public class FarmTileData
    {
        public Vector3Int position;
        public bool isTilled;
        public bool isWatered;
        public float waterMinutesRemaining;
        public CropData plantedCrop;
        public float wateredMinutesAccumulated;
        public bool wasFullyGrownLastTick = false;

        public static readonly int MinutesPerWatering = 600;

        public FarmTileData() : this(Vector3Int.zero) { }

        public FarmTileData(Vector3Int pos)
        {
            position = pos;
            isTilled = false;
            isWatered = false;
            waterMinutesRemaining = 0;
            plantedCrop = null;
            wateredMinutesAccumulated = 0f;
        }

        public float GetGrowthProgressPercent()
        {
            return plantedCrop == null ? 0f : Mathf.Clamp01(wateredMinutesAccumulated / plantedCrop.growthDurationMinutes);
        }

        public bool HasRipeCrop()
        {
            return plantedCrop != null && wateredMinutesAccumulated >= plantedCrop.growthDurationMinutes;
        }

        public void Clear()
        {
            isTilled = false;
            isWatered = false;
            waterMinutesRemaining = 0;
            plantedCrop = null;
            wateredMinutesAccumulated = 0f;
        }

        public bool IsJustFullyGrown()
        {
            bool now = HasRipeCrop();
            bool was = wasFullyGrownLastTick;
            wasFullyGrownLastTick = now;
            return now && !was;
        }
    }
}
