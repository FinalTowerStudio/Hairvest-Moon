using HairvestMoon.Core;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace HairvestMoon.Farming
{
    /// <summary>
    /// Stores logical gameplay state for each farm tile. Tracks tilling, watering, and crop growth.
    /// Interfaces with tilemaps and farming tools to apply gameplay effects.
    /// </summary>
    public class FarmTileDataManager : MonoBehaviour, IBusListener
    {
        private Dictionary<Vector3Int, FarmTileData> _tileDataMap = new();

        [Header("References")]
        [SerializeField] private Tilemap tillableTilemap;
        [SerializeField] private Tilemap tilledTilemap;
        [SerializeField] private Tilemap wateredTilemap;
        [SerializeField] private Tilemap cropTilemap;

        [SerializeField] private Tile tilledTile;
        [SerializeField] private Tile wateredOverlayTile;

        [SerializeField] private WaterVisualSystem _waterVisualSystem;

        public Tilemap CropTilemap => cropTilemap;
        public Dictionary<Vector3Int, FarmTileData> AllTileData => _tileDataMap;

        public const float HoursPerWatering = 12f;

        public void RegisterBusListeners()
        {
            var bus = ServiceLocator.Get<GameEventBus>();
            bus.GlobalSystemsInitialized += OnGlobalSystemsInitialized;
        }

        private void OnGlobalSystemsInitialized()
        {
            Initialize();
        }

        private void Initialize(){}

        public bool IsTileTillable(Vector3Int pos)
        {
            return tillableTilemap.GetTile(pos) != null;
        }

        public FarmTileData GetTileData(Vector3Int pos)
        {
            if (!_tileDataMap.TryGetValue(pos, out var data))
            {
                data = new FarmTileData();
                _tileDataMap[pos] = data;
            }
            return data;
        }

        public void SetTilled(Vector3Int pos, bool value)
        {
            var data = GetTileData(pos);
            data.isTilled = value;

            tilledTilemap.SetTile(pos, value ? tilledTile : null);
            UpdateWaterVisual(pos, data);

            ServiceLocator.Get<GameEventBus>().RaiseTileTilled(pos);
        }

        public void SetWatered(Vector3Int pos, bool value)
        {
            var data = GetTileData(pos);
            data.isWatered = value;

            if (value)
                data.waterMinutesRemaining = FarmTileData.MinutesPerWatering;
            else
                data.waterMinutesRemaining = 0f;

            UpdateWaterVisual(pos, data);
            _waterVisualSystem.HandleWateredTile(pos, data);

            ServiceLocator.Get<GameEventBus>().RaiseTileWatered(pos);
        }


        public void UpdateWaterVisual(Vector3Int pos, FarmTileData data)
        {
            if (data.isTilled && data.isWatered)
                wateredTilemap.SetTile(pos, wateredOverlayTile);
            else
                wateredTilemap.SetTile(pos, null);
        }

        public bool IsTilled(Vector3Int pos) => GetTileData(pos).isTilled;
        public bool IsWatered(Vector3Int pos) => GetTileData(pos).isWatered;
    }

    /// <summary>
    /// Struct for storing tile-specific farm state.
    /// </summary>
    public class FarmTileData
    {
        public bool isTilled = false;
        public bool isWatered = false;

        public float waterMinutesRemaining = 0f;
        public const float MinutesPerWatering = 720f;

        public CropData plantedCrop = null;
        public float wateredMinutesAccumulated = 0f;

        public float GetGrowthProgressPercent()
        {
            if (plantedCrop == null || plantedCrop.growthDurationMinutes == 0f)
                return 0f;

            return Mathf.Clamp01(wateredMinutesAccumulated / plantedCrop.growthDurationMinutes);
        }

        public bool HasRipeCrop()
        {
            return plantedCrop != null && wateredMinutesAccumulated >= plantedCrop.growthDurationMinutes;
        }
    }
}

