using HairvestMoon.Core;
using HairvestMoon.Interaction;
using HairvestMoon.Inventory;
using HairvestMoon.Tool;
using HairvestMoon.UI;
using HairvestMoon.Utility;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HairvestMoon.Farming
{
    /// <summary>
    /// Handles farm tool interactions (Hoe, Water, Plant, Harvest) using the new input system.
    /// </summary>
    public class FarmToolHandler : MonoBehaviour, IBusListener
    {
        public enum ToolSlot { None, Hoe = 1, Water = 2, Plant = 3, Harvest = 4 }

        [Header("Tool Settings")]
        [SerializeField] private float interactionHoldDuration = 0.1f;
        [SerializeField] private Transform progressSlider;
        [SerializeField] private SeedData selectedSeed;

        [Header("References")]
        [SerializeField] private InputActionReference interactAction;

        private TileTargetingSystem targetingSystem;
        private float currentHoldTime;
        private bool isInteracting;
        private bool isInitialized = false;
        private Vector3Int? targetTile;

        public void RegisterBusListeners()
        {
            var bus = ServiceLocator.Get<GameEventBus>();
            bus.GlobalSystemsInitialized += OnGlobalSystemsInitialized;
        }

        private void OnGlobalSystemsInitialized()
        {
            Initialize();
        }

        public void Initialize()
        {
            targetingSystem = ServiceLocator.Get<TileTargetingSystem>();
            interactAction.action.performed += OnInteractPerformed;
            interactAction.action.canceled += OnInteractCanceled;
            isInitialized = true;
        }

        private void Update()
        {
            if (!isInteracting || !isInitialized) return;

            targetTile = targetingSystem.CurrentTargetedTile;
            if (!targetTile.HasValue) return;

            currentHoldTime += Time.deltaTime;
            UpdateSliderVisual();

            if (currentHoldTime >= interactionHoldDuration)
            {
                CompleteInteraction();
            }
        }

        private void OnInteractPerformed(InputAction.CallbackContext context)
        {
            if (!ServiceLocator.Get<GameStateManager>().IsFreeRoam()) return;

            targetTile = targetingSystem.CurrentTargetedTile;
            if (!targetTile.HasValue)
            {
                ServiceLocator.Get<DebugUIOverlay>().ShowLastAction("No valid tile");
                return;
            }

            isInteracting = true;
            currentHoldTime = 0f;
            progressSlider.gameObject.SetActive(true);
            PositionSliderAtTarget();
        }

        private void OnInteractCanceled(InputAction.CallbackContext context)
        {
            if (!isInteracting) return;

            isInteracting = false;
            progressSlider.gameObject.SetActive(false);

            if (currentHoldTime < interactionHoldDuration)
            {
                ServiceLocator.Get<DebugUIOverlay>().ShowLastAction("Interaction cancelled");
            }
        }

        private void CompleteInteraction()
        {
            isInteracting = false;
            progressSlider.gameObject.SetActive(false);

            if (!targetTile.HasValue) return;

            var tile = targetTile.Value;
            var data = ServiceLocator.Get<FarmTileDataManager>().GetTileData(tile);

            switch (ServiceLocator.Get<ToolSystem>().CurrentTool)
            {
                case ToolType.Hoe:
                    TryTill(tile, data);
                    break;

                case ToolType.WateringCan:
                    TryWater(tile, data);
                    break;

                case ToolType.Seed:
                    TryPlantSeed(tile, data);
                    break;

                case ToolType.Harvest:
                    TryHarvest(tile, data);
                    break;

                default:
                    ServiceLocator.Get<DebugUIOverlay>().ShowLastAction("No tool selected");
                    break;
            }
        }

        private void TryTill(Vector3Int tile, FarmTileData data)
        {   
            ServiceLocator.Get<FarmTileDataManager>().SetTilled(tile, true);
            ServiceLocator.Get<DebugUIOverlay>().ShowLastAction("Tile tilled");

            // Apply Upgrade Behavior if selected
            var hoeUpgrade = ServiceLocator.Get<BackpackEquipSystem>().hoeUpgrade;
            var selectedOption = ServiceLocator.Get<HoeSelectionUI>().GetCurrentSelectedItem();

            if (selectedOption != null && hoeUpgrade != null)
            {
                ServiceLocator.Get<DebugUIOverlay>().ShowLastAction($"Hoe Upgrade Used: {selectedOption.itemName}");

                // Apply bonus tilling logic here:
                ApplyExtraTilling(tile);
            }
        }

        private void TryWater(Vector3Int tile, FarmTileData data)
        {
            ServiceLocator.Get<ToolSystem>().ConsumeWaterFromCan();

            if (!data.isTilled)
            {
                ServiceLocator.Get<DebugUIOverlay>().ShowLastAction("Water wasted — not tilled");
                return;
            }

            // Always apply water normally
            ServiceLocator.Get<FarmTileDataManager>().SetWatered(tile, true);
            ServiceLocator.Get<DebugUIOverlay>().ShowLastAction("Water applied");

            // Check if Fertilizer Sprayer is equipped
            var wateringUpgrade = ServiceLocator.Get<BackpackEquipSystem>().wateringUpgrade;

            if (wateringUpgrade != null)
            {
                // Read selected fertilizer from WateringSelectionUI
                ItemData selectedFertilizer = ServiceLocator.Get<WateringSelectionUI>().GetCurrentSelectedItem();

                if (selectedFertilizer != null)
                {
                    bool removed = ServiceLocator.Get<BackpackInventorySystem>().RemoveItem(selectedFertilizer, 1);
                    if (removed)
                    {
                        ServiceLocator.Get<DebugUIOverlay>().ShowLastAction($"Fertilizer applied: {selectedFertilizer.itemName}");
                        // You can expand here to actually apply fertilizer effects to tile data later.
                    }
                    else
                    {
                        ServiceLocator.Get<DebugUIOverlay>().ShowLastAction("No fertilizer available.");
                    }
                }
            }
        }


        private void TryPlantSeed(Vector3Int tile, FarmTileData data)
        {
            if (data.isTilled && data.plantedCrop == null && selectedSeed != null)
            {
                int seedCount = ServiceLocator.Get<ResourceInventorySystem>().GetQuantity(selectedSeed.seedItem);
                if (seedCount <= 0)
                {
                    ServiceLocator.Get<DebugUIOverlay>().ShowLastAction("No seeds available");
                    return;
                }

                bool removed = ServiceLocator.Get<ResourceInventorySystem>().RemoveItem(selectedSeed.seedItem, 1);
                if (!removed)
                {
                    ServiceLocator.Get<DebugUIOverlay>().ShowLastAction("Failed to consume seed");
                    return;
                }

                data.plantedCrop = selectedSeed.cropData;
                data.wateredMinutesAccumulated = 0f;
                ServiceLocator.Get<DebugUIOverlay>().ShowLastAction($"Planted {selectedSeed.cropData.cropName}");
            }
            else
            {
                ServiceLocator.Get<DebugUIOverlay>().ShowLastAction("Can't plant here");
            }
        }

        private void TryHarvest(Vector3Int tile, FarmTileData data)
        {
            if (data.HasRipeCrop())
            {
                var harvestedItem = data.plantedCrop.harvestedItem;
                var yield = data.plantedCrop.harvestYield;

                bool added = ServiceLocator.Get<ResourceInventorySystem>().AddItem(harvestedItem, yield);

                if (added)
                {
                    ServiceLocator.Get<DebugUIOverlay>().ShowLastAction($"Harvested {yield}x {harvestedItem.itemName}");
                    data.plantedCrop = null;
                    data.wateredMinutesAccumulated = 0f;
                }
                else
                {
                    ServiceLocator.Get<DebugUIOverlay>().ShowLastAction("Inventory Full - Harvest Failed");
                }
            }
            else
            {
                ServiceLocator.Get<DebugUIOverlay>().ShowLastAction("Nothing to harvest");
            }
        }

        private void ApplyExtraTilling(Vector3Int centerTile)
        {
            // Simple 3x3 till radius as an example:
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    Vector3Int nearbyTile = new Vector3Int(centerTile.x + dx, centerTile.y + dy, 0);
                    var tileData = ServiceLocator.Get<FarmTileDataManager>().GetTileData(nearbyTile);
                    if (!tileData.isTilled)
                    {
                        ServiceLocator.Get<FarmTileDataManager>().SetTilled(nearbyTile, true);
                    }
                }
            }
        }

        public void SetSelectedSeed(SeedData newSeed)
        {
            selectedSeed = newSeed;
        }

        private void PositionSliderAtTarget()
        {
            if (!targetTile.HasValue) return;
            Vector3 worldPos = targetingSystem.Grid.CellToWorld(targetTile.Value);
            progressSlider.position = worldPos + Vector3.up;
        }

        private void UpdateSliderVisual()
        {
            float progress = Mathf.Clamp01(currentHoldTime / interactionHoldDuration);
            progressSlider.localScale = new Vector3(progress, 1f, 1f);
        }
    }
}