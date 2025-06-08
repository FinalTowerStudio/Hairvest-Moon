using HairvestMoon.Core;
using HairvestMoon.Farming;
using HairvestMoon.Interaction;
using HairvestMoon.Inventory;
using HairvestMoon.Player;
using HairvestMoon.UI;
using HairvestMoon.Utility;
using System;
using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    [Header("Core Scene-Assigned Systems")]
    [SerializeField] private FarmTileDataManager farmTileDataManager;
    [SerializeField] private FarmToolHandler farmToolHandler;
    [SerializeField] private WaterVisualSystem waterVisualSystem;
    [SerializeField] private CropVisualSystem cropVisualSystem;
    [SerializeField] private TileTargetingSystem tileTargetingSystem;
    [SerializeField] private SeedDatabase seedDatabase;

    [Header("Player + Input")]
    [SerializeField] private InputController inputController;
    [SerializeField] private Player_Controller playerController;
    [SerializeField] private PlayerFacingController playerFacingController;
    [SerializeField] private PlayerStateController playerStateController;

    [Header("UI Panels")]
    [SerializeField] private MainMenuUIManager mainMenuUI;
    [SerializeField] private BackpackInventoryUI backpackInventoryUI;
    [SerializeField] private ResourceInventoryUI inventoryOverviewUI;
    [SerializeField] private InstallConfirmUI installConfirmUI;
    [SerializeField] private SeedSelectionUI seedSelectionUI;
    [SerializeField] private HoeSelectionUI hoeSelectionUI;
    [SerializeField] private WateringSelectionUI wateringSelectionUI;
    [SerializeField] private HarvestSelectionUI harvestSelectionUI;
    [SerializeField] private ToolHotbarUI toolHotbarUI;
    [SerializeField] private SelectionTooltipUI selectionTooltipUI;
    [SerializeField] private BackpackCapacityBarUI backpackCapacityBarUI;   
    [SerializeField] private DebugUIOverlay debugOverlayUI;

    private void Awake()
    {
        ServiceLocator.Clear();

        RegisterAllPureSystems();
        RegisterAllSceneAssignedMonoBehaviours();
        InitializeUI();
        RegisterAllBusListeners();
        RaiseGlobalSystemsInitialized();
    }

    private void RegisterAllPureSystems()
    {
        // Code-instantiated systems:
        ServiceLocator.Register(new GameEventBus());
        ServiceLocator.Register(new GameStateManager());
        ServiceLocator.Register(new GameTimeManager());
        ServiceLocator.Register(new GameManager());
        ServiceLocator.Register(new ResourceInventorySystem());
        ServiceLocator.Register(new BackpackInventorySystem());
        ServiceLocator.Register(new BackpackEquipSystem());
        ServiceLocator.Register(new BackpackUpgradeManager());
        ServiceLocator.Register(new EquipManager());
        ServiceLocator.Register(new FarmGrowthSystem());
        ServiceLocator.Register(new WaterDecaySystem());
    }

    private void RegisterAllSceneAssignedMonoBehaviours()
    {
        ServiceLocator.Register(farmTileDataManager);
        ServiceLocator.Register(farmToolHandler);
        ServiceLocator.Register(waterVisualSystem);
        ServiceLocator.Register(cropVisualSystem);
        ServiceLocator.Register(tileTargetingSystem);
        ServiceLocator.Register(seedDatabase);
        ServiceLocator.Register(inputController);
        ServiceLocator.Register(playerController);
        ServiceLocator.Register(playerFacingController);
        ServiceLocator.Register(playerStateController);
        ServiceLocator.Register(installConfirmUI);
        ServiceLocator.Register(mainMenuUI);
        ServiceLocator.Register(backpackInventoryUI);
        ServiceLocator.Register(inventoryOverviewUI);
        ServiceLocator.Register(seedSelectionUI);
        ServiceLocator.Register(wateringSelectionUI);
        ServiceLocator.Register(hoeSelectionUI);
        ServiceLocator.Register(harvestSelectionUI);
        ServiceLocator.Register(selectionTooltipUI);
        ServiceLocator.Register(backpackCapacityBarUI);
        ServiceLocator.Register(toolHotbarUI);
        ServiceLocator.Register(debugOverlayUI);
    }

    private void InitializeUI()
    {
        backpackInventoryUI.InitializeUI();
        inventoryOverviewUI.InitializeUI();
        mainMenuUI.InitializeUI();
        backpackCapacityBarUI.InitializeUI();
        debugOverlayUI.InitializeUI();
    }

    private void RegisterAllBusListeners()
    {
        backpackInventoryUI.RegisterBusListeners();
        inventoryOverviewUI.RegisterBusListeners();
        mainMenuUI.RegisterBusListeners();
        seedSelectionUI.RegisterBusListeners();
        wateringSelectionUI.RegisterBusListeners();
        hoeSelectionUI.RegisterBusListeners();
        harvestSelectionUI.RegisterBusListeners();
        backpackCapacityBarUI.RegisterBusListeners();
        inputController.RegisterBusListeners();
        playerController.RegisterBusListeners();
        farmTileDataManager.RegisterBusListeners();
        farmToolHandler.RegisterBusListeners();
        waterVisualSystem.RegisterBusListeners();
        cropVisualSystem.RegisterBusListeners();
        seedDatabase.RegisterBusListeners();
        //playerFacingController.RegisterBusListeners();
        //selectionTooltipUI.RegisterBusListeners();
        //tileTargetingSystem.RegisterBusListeners();
        //installConfirmUI.RegisterBusListeners();
        ServiceLocator.Get<GameStateManager>().RegisterBusListeners();
        ServiceLocator.Get<GameTimeManager>().RegisterBusListeners();
        ServiceLocator.Get<GameManager>().RegisterBusListeners();
        ServiceLocator.Get<ResourceInventorySystem>().RegisterBusListeners();
        ServiceLocator.Get<BackpackInventorySystem>().RegisterBusListeners();
        ServiceLocator.Get<BackpackEquipSystem>().RegisterBusListeners();
        ServiceLocator.Get<BackpackUpgradeManager>().RegisterBusListeners();
        ServiceLocator.Get<EquipManager>().RegisterBusListeners();
        ServiceLocator.Get<FarmGrowthSystem>().RegisterBusListeners();
        ServiceLocator.Get<WaterDecaySystem>().RegisterBusListeners();
    }

    private void RaiseGlobalSystemsInitialized()
    {
        ServiceLocator.Get<GameEventBus>().RaiseGlobalSystemsInitialized();
    }
}
