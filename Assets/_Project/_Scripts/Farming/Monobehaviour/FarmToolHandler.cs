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
    /// Handles farm tool interactions: till, water, plant, harvest, including upgrades and fertilization.
    /// </summary>
    public class FarmToolHandler : MonoBehaviour, IBusListener, ITickable
    {
        public enum ToolSlot { None, Hoe = 1, Water = 2, Plant = 3, Harvest = 4 }

        [Header("Tool Settings")]
        [SerializeField] private float interactionHoldDuration = 0.1f;
        [SerializeField] private Transform progressSlider;

        [Header("References")]
        [SerializeField] private InputActionReference interactAction;

        private TileTargetingSystem _targetingSystem;
        private SeedData _selectedSeed;
        private float _currentHoldTime;
        private bool _isInteracting;
        private bool _isInitialized = false;
        private Vector3Int? targetTile;

        private ToolType _activeToolAtHoldStart = ToolType.None;

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
            _targetingSystem = ServiceLocator.Get<TileTargetingSystem>();
            interactAction.action.performed += OnInteractPerformed;
            interactAction.action.canceled += OnInteractCanceled;
            _isInitialized = true;
        }

        public void Tick(GameTimeChangedArgs args)
        {
            if (!_isInteracting || !_isInitialized) return;

            // Defensive: cancel if tool was swapped during interaction
            if (ServiceLocator.Get<ToolSystem>().CurrentTool != _activeToolAtHoldStart)
            {
                CancelInteraction("Tool swapped!");
                return;
            }

            targetTile = _targetingSystem.CurrentTargetedTile;
            if (!targetTile.HasValue) return;

            _currentHoldTime += Time.deltaTime;
            UpdateSliderVisual();

            if (_currentHoldTime >= interactionHoldDuration)
            {
                CompleteInteraction();
            }
        }

        /// <summary>
        /// Begins a tool interaction on valid tile.
        /// </summary>
        private void OnInteractPerformed(InputAction.CallbackContext context)
        {
            if (!ServiceLocator.Get<GameStateManager>().IsFreeRoam()) return;

            targetTile = _targetingSystem.CurrentTargetedTile;
            if (!targetTile.HasValue)
            {
                ShowDebug("No valid tile");
                return;
            }

            _isInteracting = true;
            _currentHoldTime = 0f;
            _activeToolAtHoldStart = ServiceLocator.Get<ToolSystem>().CurrentTool;

            progressSlider.gameObject.SetActive(true);
            PositionSliderAtTarget();
        }

        /// <summary>
        /// Cancels interaction on input release or tool swap.
        /// </summary>
        private void OnInteractCanceled(InputAction.CallbackContext context)
        {
            if (!_isInteracting) return;
            CancelInteraction("Interaction cancelled");
        }

        /// <summary>
        /// Instantly cancels interaction with feedback.
        /// </summary>
        private void CancelInteraction(string reason)
        {
            _isInteracting = false;
            progressSlider.gameObject.SetActive(false);
            ShowDebug(reason);
            // TODO: Play cancel sound if desired
        }

        /// <summary>
        /// Executes the tool's action after hold completes.
        /// Add SFX/VFX hooks here for all tool actions.
        /// </summary>
        private void CompleteInteraction()
        {
            _isInteracting = false;
            progressSlider.gameObject.SetActive(false);

            if (!targetTile.HasValue) return;

            var tile = targetTile.Value;
            var data = ServiceLocator.Get<FarmTileDataManager>().GetTileData(tile);
            var equipSystem = ServiceLocator.Get<BackpackEquipSystem>();

            switch (ServiceLocator.Get<ToolSystem>().CurrentTool)
            {
                case ToolType.Hoe:
                    if (equipSystem.hoeTool == null)
                    {
                        ShowDebug("Tilling with hands (no hoe equipped).");
                        // TODO: Hands penalty here
                    }
                    else
                    {
                        ShowDebug("Tilling with equipped hoe!");
                    }
                    TryTill(tile, data);
                    break;

                case ToolType.WateringCan:
                    if (equipSystem.wateringTool == null)
                    {
                        ShowDebug("You have not equipped a watering can!");
                        // TODO: Play error sound
                        return; // Block action
                    }
                    ShowDebug("Watering with can.");
                    TryWater(tile, data);
                    break;

                case ToolType.Seed:
                    TryPlantSeed(tile, data); // Handles feedback internally
                    break;

                case ToolType.Harvest:
                    if (equipSystem.harvestTool == null)
                    {
                        ShowDebug("Harvesting by hand (no tool equipped).");
                        // TODO: Hands penalty here
                    }
                    else
                    {
                        ShowDebug("Harvesting with equipped tool!");
                    }
                    TryHarvest(tile, data);
                    break;

                default:
                    ShowDebug("No tool selected");
                    break;
            }
            // SFX/VFX TODO
        }


        /// <summary>
        /// Tills a tile, applies upgrades and bonus area if equipped.
        /// </summary>
        private void TryTill(Vector3Int tile, FarmTileData data)
        {
            ServiceLocator.Get<FarmTileDataManager>().SetTilled(tile, true);
            ShowDebug("Tile tilled");

            // --- SFX/VFX hook: play "till" sound, dust particle ---

            // Apply Upgrade Behavior if selected
            var hoeUpgrade = ServiceLocator.Get<BackpackEquipSystem>().hoeUpgrade;
            var selectedOption = ServiceLocator.Get<HoeSelectionUI>().GetCurrentSelectedItem();

            if (selectedOption != null && hoeUpgrade != null)
            {
                ShowDebug($"Hoe Upgrade Used: {selectedOption.itemName}");
                // TODO: Play "upgrade" particle or effect here
                ApplyExtraTilling(tile);
            }
        }

        /// <summary>
        /// Waters a tile, optionally applies fertilizer.
        /// </summary>
        private void TryWater(Vector3Int tile, FarmTileData data)
        {
            ServiceLocator.Get<ToolSystem>().ConsumeWaterFromCan();

            if (!data.isTilled)
            {
                ShowDebug("Water wasted — not tilled");
                // TODO: Play "fail" sound or splatter VFX
                return;
            }

            // Always apply water normally
            ServiceLocator.Get<FarmTileDataManager>().SetWatered(tile, true);
            ShowDebug("Water applied");
            // TODO: Play "watering" sound, water spray VFX

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
                        ShowDebug($"Fertilizer applied: {selectedFertilizer.itemName}");
                        // TODO: Apply actual fertilizer logic, spawn particle here
                    }
                    else
                    {
                        ShowDebug("No fertilizer available.");
                    }
                }
            }
        }

        /// <summary>
        /// Attempts to plant a seed on a tile.
        /// </summary>
        private void TryPlantSeed(Vector3Int tile, FarmTileData data)
        {
            if (data.isTilled && data.plantedCrop == null && _selectedSeed != null)
            {
                int seedCount = ServiceLocator.Get<ResourceInventorySystem>().GetQuantity(_selectedSeed.seedItem);
                if (seedCount <= 0)
                {
                    ShowDebug("No seeds available");
                    // TODO: Play "fail" sound
                    return;
                }

                bool removed = ServiceLocator.Get<ResourceInventorySystem>().RemoveItem(_selectedSeed.seedItem, 1);
                if (!removed)
                {
                    ShowDebug("Failed to consume seed");
                    // TODO: Play "fail" sound
                    return;
                }

                data.plantedCrop = _selectedSeed.cropData;
                data.wateredMinutesAccumulated = 0f;
                ShowDebug($"Planted {_selectedSeed.cropData.cropName}");
                // TODO: Play "planting" sound, dirt puff particle
            }
            else
            {
                ShowDebug("Can't plant here");
            }
        }

        /// <summary>
        /// Harvests a ripe crop, adds to inventory, clears tile.
        /// </summary>
        private void TryHarvest(Vector3Int tile, FarmTileData data)
        {
            if (data.HasRipeCrop())
            {
                var harvestedItem = data.plantedCrop.harvestedItem;
                var yield = data.plantedCrop.harvestYield;

                bool added = ServiceLocator.Get<ResourceInventorySystem>().AddItem(harvestedItem, yield);

                if (added)
                {
                    ShowDebug($"Harvested {yield}x {harvestedItem.itemName}");
                    data.plantedCrop = null;
                    data.wateredMinutesAccumulated = 0f;
                    // TODO: Play "harvest" sound, burst VFX, maybe screen shake
                }
                else
                {
                    ShowDebug("Inventory Full - Harvest Failed");
                    // TODO: Play "fail" sound
                }
            }
            else
            {
                ShowDebug("Nothing to harvest");
                // TODO: Play "fail" sound
            }
        }

        /// <summary>
        /// Applies area effect tilling for upgrades (e.g., 3x3 radius).
        /// </summary>
        private void ApplyExtraTilling(Vector3Int centerTile)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    Vector3Int nearbyTile = new Vector3Int(centerTile.x + dx, centerTile.y + dy, 0);
                    var tileData = ServiceLocator.Get<FarmTileDataManager>().GetTileData(nearbyTile);
                    if (!tileData.isTilled)
                    {
                        ServiceLocator.Get<FarmTileDataManager>().SetTilled(nearbyTile, true);
                        // TODO: Particle/SFX for bonus tiles?
                    }
                }
            }
        }

        /// <summary>
        /// Sets the currently selected seed for planting.
        /// </summary>
        public void SetSelectedSeed(SeedData newSeed)
        {
            _selectedSeed = newSeed;
        }

        /// <summary>
        /// Positions progress slider over targeted tile.
        /// </summary>
        private void PositionSliderAtTarget()
        {
            if (!targetTile.HasValue) return;
            Vector3 worldPos = _targetingSystem.Grid.CellToWorld(targetTile.Value);
            progressSlider.position = worldPos + Vector3.up;
        }

        /// <summary>
        /// Visually updates the progress slider fill.
        /// </summary>
        private void UpdateSliderVisual()
        {
            float progress = Mathf.Clamp01(_currentHoldTime / interactionHoldDuration);
            progressSlider.localScale = new Vector3(progress, 1f, 1f);
        }

        /// <summary>
        /// Shows debug feedback overlay for user actions.
        /// </summary>
        private void ShowDebug(string message)
        {
            ServiceLocator.Get<DebugUIOverlay>().ShowLastAction(message);
        }
    }
}