using HairvestMoon.Core;
using HairvestMoon.Farming;
using HairvestMoon.Interaction;
using HairvestMoon.Inventory;
using HairvestMoon.Player;
using HairvestMoon.Tool;
using HairvestMoon.UI;
using HairvestMoon.Utility;
using System;
using UnityEngine;

namespace HairvestMoon.Core
{
    public class GameInitializer : MonoBehaviour
    {
        [Header("Core Scene-Assigned Systems")]
        [SerializeField] private GameTimeDriver _gameTimeDriver;
        [SerializeField] private FarmTileDataManager _farmTileDataManager;
        [SerializeField] private FarmToolHandler _farmToolHandler;
        [SerializeField] private WaterVisualSystem _waterVisualSystem;
        [SerializeField] private CropVisualSystem _cropVisualSystem;
        [SerializeField] private TileTargetingSystem _tileTargetingSystem;
        [SerializeField] private SeedDatabase _seedDatabase;
        [SerializeField] private ToolSystem _toolSystem;
        [SerializeField] private ToolSelector _toolSelector;

        [Header("Player + Input")]
        [SerializeField] private InputController _inputController;
        [SerializeField] private Player_Controller _playerController;
        [SerializeField] private PlayerStateController _playerStateController;

        [Header("UI Panels")]
        [SerializeField] private MainMenuUIManager _mainMenuUI;
        [SerializeField] private BackpackInventoryUI _backpackInventoryUI;
        [SerializeField] private ResourceInventoryUI _resourceInventoryUI;
        [SerializeField] private InstallConfirmUI _installConfirmUI;
        [SerializeField] private SeedSelectionUI _seedSelectionUI;
        [SerializeField] private HoeSelectionUI _hoeSelectionUI;
        [SerializeField] private WateringSelectionUI _wateringSelectionUI;
        [SerializeField] private HarvestSelectionUI _harvestSelectionUI;
        [SerializeField] private ToolHotbarUI _toolHotbarUI;
        [SerializeField] private SelectionTooltipUI _selectionTooltipUI;
        [SerializeField] private BackpackCapacityBarUI _backpackCapacityBarUI;
        [SerializeField] private DebugUIOverlay _debugOverlayUI;
        

        private void Awake()
        {
            ServiceLocator.Clear();

            RegisterAllPureSystems();
            RegisterAllSceneAssignedMonoBehaviours();
            InitializeUI();
            RegisterAllBusListeners();
            RaiseGlobalSystemsInitialized();
        }

        /// <summary>
        /// Registers all pure code-instantiated systems in the ServiceLocator.
        /// </summary>
        private void RegisterAllPureSystems()
        {
            // Code-instantiated systems:
            ServiceLocator.Register(new GameEventBus());
            ServiceLocator.Register(new PlayerFacingController());
            ServiceLocator.Register(new GameStateManager());
            ServiceLocator.Register(new GameTimeManager());
            ServiceLocator.Register(new GameManager());
            ServiceLocator.Register(new ResourceInventorySystem());
            ServiceLocator.Register(new BackpackInventorySystem());
            ServiceLocator.Register(new BackpackEquipSystem());
            ServiceLocator.Register(new BackpackEquipInstallManager());
            ServiceLocator.Register(new BackpackUpgradeManager());
            ServiceLocator.Register(new EquipManager());
            ServiceLocator.Register(new FarmGrowthSystem());
            ServiceLocator.Register(new WaterDecaySystem());
        }

        /// <summary>
        /// Registers all inspector-assigned MonoBehaviours in the ServiceLocator.
        /// </summary>
        private void RegisterAllSceneAssignedMonoBehaviours()
        {
            ServiceLocator.Register(_farmTileDataManager);
            ServiceLocator.Register(_farmToolHandler);
            ServiceLocator.Register(_toolSystem);
            ServiceLocator.Register(_toolSelector);
            ServiceLocator.Register(_waterVisualSystem);
            ServiceLocator.Register(_cropVisualSystem);
            ServiceLocator.Register(_tileTargetingSystem);
            ServiceLocator.Register(_seedDatabase);
            ServiceLocator.Register(_inputController);
            ServiceLocator.Register(_playerController);
            ServiceLocator.Register(_gameTimeDriver);
            ServiceLocator.Register(_playerStateController);
            ServiceLocator.Register(_installConfirmUI);
            ServiceLocator.Register(_mainMenuUI);
            ServiceLocator.Register(_backpackInventoryUI);
            ServiceLocator.Register(_resourceInventoryUI);
            ServiceLocator.Register(_seedSelectionUI);
            ServiceLocator.Register(_wateringSelectionUI);
            ServiceLocator.Register(_hoeSelectionUI);
            ServiceLocator.Register(_harvestSelectionUI);
            ServiceLocator.Register(_selectionTooltipUI);
            ServiceLocator.Register(_backpackCapacityBarUI);
            ServiceLocator.Register(_toolHotbarUI);
            ServiceLocator.Register(_debugOverlayUI);
        }

        /// <summary>
        /// Calls InitializeUI() for all UI systems that need it.
        /// </summary>
        private void InitializeUI()
        {
            _backpackInventoryUI.InitializeUI();
            _resourceInventoryUI.InitializeUI();
            _mainMenuUI.InitializeUI();
            _backpackCapacityBarUI.InitializeUI();
            _debugOverlayUI.InitializeUI();
        }

        /// <summary>
        /// Registers all IBusListener systems and UIs to the event bus.
        /// </summary>
        private void RegisterAllBusListeners()
        {
            _gameTimeDriver.RegisterBusListeners();
            _backpackInventoryUI.RegisterBusListeners();
            _resourceInventoryUI.RegisterBusListeners();
            _mainMenuUI.RegisterBusListeners();
            _seedSelectionUI.RegisterBusListeners();
            _wateringSelectionUI.RegisterBusListeners();
            _hoeSelectionUI.RegisterBusListeners();
            _harvestSelectionUI.RegisterBusListeners();
            _backpackCapacityBarUI.RegisterBusListeners();
            _inputController.RegisterBusListeners();
            _playerController.RegisterBusListeners();
            _playerStateController.RegisterBusListeners();
            _farmTileDataManager.RegisterBusListeners();
            _farmToolHandler.RegisterBusListeners();
            _waterVisualSystem.RegisterBusListeners();
            _cropVisualSystem.RegisterBusListeners();
            _seedDatabase.RegisterBusListeners();
            _installConfirmUI.RegisterBusListeners();
            _tileTargetingSystem.RegisterBusListeners();
            _toolSystem.RegisterBusListeners();
            _toolSelector.RegisterBusListeners();
            //_selectionTooltipUI.RegisterBusListeners();
            ServiceLocator.Get<PlayerFacingController>().RegisterBusListeners();
            ServiceLocator.Get<GameStateManager>().RegisterBusListeners();
            ServiceLocator.Get<GameTimeManager>().RegisterBusListeners();
            ServiceLocator.Get<GameManager>().RegisterBusListeners();
            ServiceLocator.Get<ResourceInventorySystem>().RegisterBusListeners();
            ServiceLocator.Get<BackpackInventorySystem>().RegisterBusListeners();
            ServiceLocator.Get<BackpackEquipSystem>().RegisterBusListeners();
            ServiceLocator.Get<BackpackUpgradeManager>().RegisterBusListeners();
            ServiceLocator.Get<BackpackEquipInstallManager>().RegisterBusListeners();
            ServiceLocator.Get<EquipManager>().RegisterBusListeners();
            ServiceLocator.Get<FarmGrowthSystem>().RegisterBusListeners();
            ServiceLocator.Get<WaterDecaySystem>().RegisterBusListeners();
        }

        /// <summary>
        /// Triggers the global system initialized event for all bus listeners.
        /// </summary>
        private void RaiseGlobalSystemsInitialized()
        {
            ServiceLocator.Get<GameEventBus>().RaiseGlobalSystemsInitialized();
        }
    }
}
