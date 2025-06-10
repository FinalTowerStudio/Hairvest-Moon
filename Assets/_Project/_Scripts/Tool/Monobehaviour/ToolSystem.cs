using HairvestMoon.Utility;
using UnityEngine;
using HairvestMoon.Farming;
using HairvestMoon.UI;
using HairvestMoon.Core;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HairvestMoon.Tool
{
    /// <summary>
    /// Core tool system: holds the current active tool, manages watering can state,
    /// notifies selection UIs, and provides feedback.
    /// </summary>
    public partial class ToolSystem : MonoBehaviour, IBusListener
    {
        [Header("Watering Can Settings")]
        public float waterCanCapacity = 100f;
        public float maxWaterCapacity = 100f;
        public float waterPerUse = 1f;

        public ToolType CurrentTool { get; private set; } = ToolType.None;

        private static GameEventBus _eventBus;
        private static SeedSelectionUI _seedUI;
        private static WateringSelectionUI _wateringUI;
        private static HoeSelectionUI _hoeUI;
        private static HarvestSelectionUI _harvestUI;
        private static DebugUIOverlay _debugUI;

        public void RegisterBusListeners()
        {
            _eventBus = ServiceLocator.Get<GameEventBus>();
            _eventBus.GlobalSystemsInitialized += OnGlobalSystemsInitialized;
        }

        private void OnGlobalSystemsInitialized()
        {
            Initialize();
        }

        /// <summary>
        /// Sets up UI and EventBus references for efficiency and safety.
        /// </summary>
        public void Initialize()
        {
            _seedUI = ServiceLocator.Get<SeedSelectionUI>();
            _wateringUI = ServiceLocator.Get<WateringSelectionUI>();
            _hoeUI = ServiceLocator.Get<HoeSelectionUI>();
            _harvestUI = ServiceLocator.Get<HarvestSelectionUI>();
            _debugUI = ServiceLocator.Get<DebugUIOverlay>();
        }

        /// <summary>
        /// Set the currently active tool, notify UIs, fire change events.
        /// </summary>
        public void SetTool(ToolType tool)
        {
            CurrentTool = tool;
            _debugUI?.ShowLastAction($"Tool: {CurrentTool}");

            // Close all selection UIs first
            _seedUI?.CloseSeedMenu();
            _wateringUI?.CloseWateringMenu();
            _hoeUI?.CloseHoeMenu();
            _harvestUI?.CloseHarvestMenu();

            // Open only the active tool's selection UI
            switch (tool)
            {
                case ToolType.Seed:
                    _seedUI?.OpenSeedMenu();
                    break;
                case ToolType.WateringCan:
                    _wateringUI?.OpenWateringMenu();
                    break;
                case ToolType.Hoe:
                    _hoeUI?.OpenHoeMenu();
                    break;
                case ToolType.Harvest:
                    _harvestUI?.OpenHarvestMenu();
                    break;
            }

            // Broadcast event bus ToolChanged for all listeners
            _eventBus?.RaiseToolChanged(tool);
        }

        /// <summary>
        /// Consume water from the can when used. Enforces min/max.
        /// </summary>
        public void ConsumeWaterFromCan()
        {
            waterCanCapacity -= waterPerUse;
            waterCanCapacity = Mathf.Clamp(waterCanCapacity, 0f, maxWaterCapacity);
            _debugUI.ShowLastAction($"Water Remaining: {waterCanCapacity}");
            // TODO: Play "water used" sound, check for empty state
        }

        /// <summary>
        /// Refill can by amount, up to max capacity.
        /// </summary>
        public void RefillWaterCan(float refillAmount)
        {
            waterCanCapacity += refillAmount;
            waterCanCapacity = Mathf.Clamp(waterCanCapacity, 0f, maxWaterCapacity);
            // TODO: Play refill sound, feedback
        }

        /// <summary>
        /// Fully refill water can.
        /// </summary>
        public void RefillWaterToFull()
        {
            waterCanCapacity = maxWaterCapacity;
            // TODO: Play full refill effect
        }



#if UNITY_EDITOR
        [CustomEditor(typeof(ToolSystem))]
        public class ToolSystemEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                DrawDefaultInspector();

                ToolSystem toolSystem = (ToolSystem)target;
                if (GUILayout.Button("Refill Water (Dev Only)"))
                {
                    toolSystem.RefillWaterToFull();
                }
            }
        }
        #endif

    }
}
