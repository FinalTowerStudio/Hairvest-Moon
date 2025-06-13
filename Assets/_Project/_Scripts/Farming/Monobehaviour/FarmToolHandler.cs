using HairvestMoon.Core;
using HairvestMoon.Interaction;
using HairvestMoon.Tool;
using HairvestMoon.Farming;
using HairvestMoon.Utility;
using UnityEngine;

namespace HairvestMoon.Farming
{
    public class FarmToolHandler : MonoBehaviour, IBusListener, ITickable
    {
        [Header("Tool Settings")]
        [SerializeField] private float interactionHoldDuration = 0.5f;
        [SerializeField] private Transform progressSlider;

        private TileTargetingSystem _targetingSystem;
        private FarmTileDataManager _tileDataManager;
        private ToolSystem _toolSystem;
        private SeedDatabase _seedDatabase;

        private float _currentHoldTime = 0f;
        private bool _isInteracting = false;
        private Vector3Int? _activeTile = null;
        private ToolType _activeToolTypeAtHoldStart;

        private GameEventBus _eventBus;
        private bool _isInitialized = false;

        public void RegisterBusListeners()
        {
            _eventBus = ServiceLocator.Get<GameEventBus>();
            _eventBus.GlobalSystemsInitialized += OnGlobalSystemsInitialized;
            _eventBus.InteractPressed += OnInteractPressed;
            _eventBus.InteractReleased += OnInteractReleased;
        }

        private void OnGlobalSystemsInitialized()
        {
            _targetingSystem = ServiceLocator.Get<TileTargetingSystem>();
            _tileDataManager = ServiceLocator.Get<FarmTileDataManager>();
            _toolSystem = ServiceLocator.Get<ToolSystem>();
            _seedDatabase = ServiceLocator.Get<SeedDatabase>();
            _isInitialized = true;
        }

        public void Tick(GameTimeChangedArgs args) { }

        private void Update()
        {
            if (!_isInitialized) return;

            if (_isInteracting && _activeTile.HasValue)
            {
                // Cancel if tile or tool changes.
                var currentTarget = _targetingSystem.CurrentTargetedTile;
                if (!_activeTile.Equals(currentTarget) || _toolSystem.CurrentTool != _activeToolTypeAtHoldStart)
                {
                    CancelInteraction("Moved/canceled/interrupted");
                    return;
                }

                _currentHoldTime += Time.deltaTime;
                UpdateSliderVisual();

                if (_currentHoldTime >= interactionHoldDuration)
                {
                    CompleteInteraction();
                }
            }
        }

        private void OnInteractPressed()
        {
            if (!ServiceLocator.Get<GameStateManager>().IsFreeRoam()) return;

            var targetTile = _targetingSystem.CurrentTargetedTile;
            if (!targetTile.HasValue) return;

            ToolType tool = _toolSystem.CurrentTool;
            bool canInteract = false;

            switch (tool)
            {
                case ToolType.Hoe:
                    canInteract = _tileDataManager.CanTill(targetTile.Value);
                    break;
                case ToolType.WateringCan:
                    canInteract = _tileDataManager.CanWater(targetTile.Value);
                    break;
                case ToolType.Seed:
                    canInteract = _tileDataManager.CanPlant(targetTile.Value, _toolSystem.GetCurrentSelectedSeed());
                    break;
                case ToolType.Harvest:
                    canInteract = _tileDataManager.CanHarvest(targetTile.Value);
                    break;
            }

            if (!canInteract) return;

            _isInteracting = true;
            _currentHoldTime = 0f;
            _activeTile = targetTile;
            _activeToolTypeAtHoldStart = tool;

            ShowSliderAtTile(_activeTile.Value);
            SetSliderFill(0f);
        }

        private void OnInteractReleased()
        {
            if (_isInteracting)
                CancelInteraction("Interaction released/canceled");
        }

        private void CancelInteraction(string reason)
        {
            _isInteracting = false;
            _currentHoldTime = 0f;
            _activeTile = null;
            HideSlider();
            ShowDebug(reason);
        }

        private void CompleteInteraction()
        {
            _isInteracting = false;
            _currentHoldTime = 0f;
            HideSlider();

            if (!_activeTile.HasValue) return;

<<<<<<< HEAD
            var tile = _activeTile.Value;
            var tool = _toolSystem.CurrentTool;
=======
            var tile = targetTile.Value;
            var data = ServiceLocator.Get<FarmTileDataManager>().GetTileData(tile);
            var equipSystem = ServiceLocator.Get<BackpackEquipSystem>();
>>>>>>> 87c96bf6d5dda68fa5bbfb48168eacf15bb85af1

            switch (tool)
            {
                case ToolType.Hoe:
<<<<<<< HEAD
                    if (_tileDataManager.CanTill(tile))
                    {
                        _tileDataManager.SetTilled(tile, true);
                        ShowDebug("Tile tilled!");
                    }
                    else ShowDebug("Can't till here.");
=======
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
>>>>>>> 87c96bf6d5dda68fa5bbfb48168eacf15bb85af1
                    break;
                case ToolType.WateringCan:
<<<<<<< HEAD
                    if (_tileDataManager.CanWater(tile))
                    {
                        _tileDataManager.SetWatered(tile, true);
                        _toolSystem.ConsumeWaterFromCan();
                        ShowDebug("Tile watered!");
                    }
                    else ShowDebug("Can't water here.");
=======
                    if (equipSystem.wateringTool == null)
                    {
                        ShowDebug("You have not equipped a watering can!");
                        // TODO: Play error sound
                        return; // Block action
                    }
                    ShowDebug("Watering with can.");
                    TryWater(tile, data);
>>>>>>> 87c96bf6d5dda68fa5bbfb48168eacf15bb85af1
                    break;
                case ToolType.Seed:
<<<<<<< HEAD
                    var selectedSeed = _toolSystem.GetCurrentSelectedSeed();
                    if (_tileDataManager.CanPlant(tile, selectedSeed))
                    {
                        _tileDataManager.PlantSeed(tile, selectedSeed);
                        // TODO: Remove seed from inventory, etc.
                        ShowDebug($"Planted {selectedSeed?.cropData.cropName ?? "seed"}!");
                    }
                    else ShowDebug("Can't plant here.");
=======
                    TryPlantSeed(tile, data); // Handles feedback internally
>>>>>>> 87c96bf6d5dda68fa5bbfb48168eacf15bb85af1
                    break;
                case ToolType.Harvest:
<<<<<<< HEAD
                    if (_tileDataManager.CanHarvest(tile))
                    {
                        _tileDataManager.HarvestCrop(tile);
                        // TODO: Add crop to inventory.
                        ShowDebug("Crop harvested!");
                    }
                    else ShowDebug("Nothing to harvest.");
                    break;
            }
=======
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
>>>>>>> 87c96bf6d5dda68fa5bbfb48168eacf15bb85af1
        }

        private void ShowSliderAtTile(Vector3Int tile)
        {
            if (progressSlider == null) return;
            var worldPos = _targetingSystem.Grid.CellToWorld(tile);
            progressSlider.position = worldPos + Vector3.up; // Offset if desired
            progressSlider.gameObject.SetActive(true);
        }

        private void HideSlider()
        {
            if (progressSlider == null) return;
            progressSlider.gameObject.SetActive(false);
        }

        private void SetSliderFill(float t)
        {
            if (progressSlider == null) return;
            progressSlider.localScale = new Vector3(Mathf.Clamp01(t), 1f, 1f);
        }

        private void UpdateSliderVisual()
        {
            SetSliderFill(_currentHoldTime / interactionHoldDuration);
        }

        private void ShowDebug(string msg)
        {
            ServiceLocator.Get<DebugUIOverlay>()?.ShowLastAction(msg);
        }
    }
}
