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
        [SerializeField] private Tilemap overlayFarmTilemap;

        [Header("Tiles")]
        [SerializeField] private TileBase baseTilledTile;
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
            overlayFarmTilemap.ClearAllTiles();
        }

        /// <summary>
        /// Returns the FarmTileData for the given position. Instantiates if not present.
        /// </summary>
        public FarmTileData GetTileData(Vector3Int pos)
        {
            if (!tileData.ContainsKey(pos))
                tileData[pos] = new FarmTileData(pos);
            return tileData[pos];
        }

        /// <summary>
        /// Tills or untils a tile, updates visual and fires event.
        /// </summary>
        public void SetTilled(Vector3Int pos, bool isTilled)
        {
            var data = GetTileData(pos);
            data.isTilled = isTilled;

            if (!isTilled)
            {
                // Untilling resets all crop and water state for the tile!
                data.plantedCrop = null;
                data.wateredMinutesAccumulated = 0f;
                data.isWatered = false;
                data.waterMinutesRemaining = 0;
            }

            baseFarmTilemap.SetTile(pos, isTilled ? baseTilledTile : null);
            UpdateWaterVisual(pos, data);

            // We could add: _eventBus.RaiseTileTilled(pos);
        }

        /// <summary>
        /// Waters or unwaters a tile, updates visual and fires event.
        /// </summary>
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

            // We could add: _eventBus.RaiseTileWatered(pos);
        }

        /// <summary>
        /// Ensures correct overlay for water state.
        /// </summary>
        public void UpdateWaterVisual(Vector3Int pos, FarmTileData data)
        {
            if (!data.isTilled)
            {
                overlayFarmTilemap.SetTile(pos, null);
                return;
            }

            overlayFarmTilemap.SetTile(pos, data.isWatered ? wateredOverlayTile : null);
        }

        /// <summary>
        /// Removes a tile (e.g., for farm expansion/removal).
        /// </summary>
        public void RemoveTile(Vector3Int pos)
        {
            tileData.Remove(pos);
            baseFarmTilemap.SetTile(pos, null);
            overlayFarmTilemap.SetTile(pos, null);
            // Could fire a custom event for tile removal.
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

        // Add more state here as needed (fertilizer, pest, soil quality, etc.)
        // public FertilizerType fertilizerType;
        // public bool isWithered;
        // public bool hasPests;

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
            // fertilizerType = FertilizerType.None;
            // isWithered = false;
        }

        /// <summary>
        /// Returns how grown the crop is (0 to 1), or 0 if none.
        /// </summary>
        public float GetGrowthProgressPercent()
        {
            return plantedCrop == null ? 0f : Mathf.Clamp01(wateredMinutesAccumulated / plantedCrop.growthDurationMinutes);
        }

        /// <summary>
        /// Returns true if the crop is fully grown and ready to harvest.
        /// </summary>
        public bool HasRipeCrop()
        {
            return plantedCrop != null && wateredMinutesAccumulated >= plantedCrop.growthDurationMinutes;
        }

        /// <summary>
        /// Optionally, clear crop and water state (for untilling or crop removal).
        /// </summary>
        public void Clear()
        {
            isTilled = false;
            isWatered = false;
            waterMinutesRemaining = 0;
            plantedCrop = null;
            wateredMinutesAccumulated = 0f;
        }

        /// <summary>
        /// Returns true if the crop just finished growing this tick.
        /// </summary>
        public bool IsJustFullyGrown()
        {
            bool now = HasRipeCrop();
            bool was = wasFullyGrownLastTick;
            wasFullyGrownLastTick = now;
            return now && !was;
        }
    }
}
