using HairvestMoon.Core;
using HairvestMoon.Interaction;
using HairvestMoon.Tool;
using HairvestMoon.Farming;
using HairvestMoon.Utility;
using UnityEngine;
using HairvestMoon.Inventory;

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
        private BackpackEquipSystem _equipSystem;

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
            _equipSystem = ServiceLocator.Get<BackpackEquipSystem>();
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
                default:
                    canInteract = false;
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

            var tile = _activeTile.Value;
            var tool = _toolSystem.CurrentTool;
            var data = _tileDataManager.GetTileData(tile);

            switch (tool)
            {
                case ToolType.Hoe:
                    if (_tileDataManager.CanTill(tile))
                    {
                        _tileDataManager.SetTilled(tile, true);
                        ShowDebug("Tile tilled!");
                        if (_equipSystem.hoeTool == null)
                        {
                            ShowDebug("Tilling with hands (no hoe equipped).");
                            // TODO: Add penalty/effect for using hands
                        }
                        else
                        {
                            ShowDebug("Tilling with equipped hoe!");
                        }
                    }
                    else
                    {
                        ShowDebug("Can't till here.");
                    }
                    // TODO: SFX/VFX
                    break;
                case ToolType.WateringCan:
                    if (_tileDataManager.CanWater(tile))
                    {
                        if (_equipSystem.wateringTool == null)
                        {
                            ShowDebug("You have not equipped a watering can!");
                            // TODO: Play error sound
                            return;
                        }
                        _tileDataManager.SetWatered(tile, true);
                        _toolSystem.ConsumeWaterFromCan();
                        ShowDebug("Tile watered!");
                        ShowDebug("Watering with can.");
                    }
                    else
                    {
                        ShowDebug("Can't water here.");
                    }
                    // TODO: SFX/VFX
                    break;
                case ToolType.Seed:
                    var selectedSeed = _toolSystem.GetCurrentSelectedSeed();
                    if (_tileDataManager.CanPlant(tile, selectedSeed))
                    {
                        _tileDataManager.PlantSeed(tile, selectedSeed);
                        // TODO: Remove seed from inventory, etc.
                        ShowDebug($"Planted {selectedSeed?.cropData.cropName ?? "seed"}!");
                    }
                    else
                    {
                        ShowDebug("Can't plant here.");
                    }
                    // TODO: SFX/VFX
                    break;
                case ToolType.Harvest:
                    if (_tileDataManager.CanHarvest(tile))
                    {
                        _tileDataManager.HarvestCrop(tile);
                        // TODO: Add crop to inventory.
                        if (_equipSystem.harvestTool == null)
                        {
                            ShowDebug("Harvesting by hand (no tool equipped).");
                            // TODO: Add hands penalty/effect
                        }
                        else
                        {
                            ShowDebug("Harvesting with equipped tool!");
                        }
                        ShowDebug("Crop harvested!");
                    }
                    else
                    {
                        ShowDebug("Nothing to harvest.");
                    }
                    // TODO: SFX/VFX
                    break;
                default:
                    ShowDebug("No tool selected");
                    break;
            }
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
