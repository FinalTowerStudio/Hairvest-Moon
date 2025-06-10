using HairvestMoon.Core;
using HairvestMoon.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HairvestMoon.Tool
{
    /// <summary>
    /// Handles tool selection via keyboard hotkeys and input actions for next/previous.
    /// Notifies ToolSystem and UI.
    /// </summary>
    public class ToolSelector : MonoBehaviour, IBusListener
    {
        [SerializeField] private ToolHotbarUI toolHotbar;

        private ToolType[] toolOrder = new[]
        {
            ToolType.Hoe,
            ToolType.WateringCan,
            ToolType.Seed,
            ToolType.Harvest
        };

        private int currentIndex = 0;
        private bool isInitialized = false;

        public void RegisterBusListeners()
        {
            var bus = ServiceLocator.Get<GameEventBus>();
            bus.GlobalSystemsInitialized += OnGlobalSystemsInitialized;
            bus.ToolNext += HandleNext;
            bus.ToolPrevious += HandlePrevious;
        }

        private void OnGlobalSystemsInitialized()
        {
            Initialize();

            isInitialized = true;
        }

        public void Initialize()
        {
            // Set tool at start and highlight UI
            SetTool(toolOrder[currentIndex]);
        }

        private void Update()
        {
            if (!isInitialized) return;

            // Number hotkeys (1–4)
            if (Keyboard.current.digit1Key.wasPressedThisFrame) SetToolByIndex(0);
            if (Keyboard.current.digit2Key.wasPressedThisFrame) SetToolByIndex(1);
            if (Keyboard.current.digit3Key.wasPressedThisFrame) SetToolByIndex(2);
            if (Keyboard.current.digit4Key.wasPressedThisFrame) SetToolByIndex(3);
        }

        public void HandleNext() => CycleTool(1);
        public void HandlePrevious() => CycleTool(-1);

        /// <summary>
        /// Sets tool by direct hotkey index.
        /// </summary>
        private void SetToolByIndex(int index)
        {
            currentIndex = Mathf.Clamp(index, 0, toolOrder.Length - 1);
            SetTool(toolOrder[currentIndex]);
        }

        /// <summary>
        /// Cycle forward/backward through tools (with wrap).
        /// </summary>
        private void CycleTool(int direction)
        {
            currentIndex = (currentIndex + direction + toolOrder.Length) % toolOrder.Length;
            SetTool(toolOrder[currentIndex]);
        }

        /// <summary>
        /// Sets the current tool, notifies ToolSystem and UI, triggers feedback hooks.
        /// </summary>
        private void SetTool(ToolType tool)
        {
            ServiceLocator.Get<ToolSystem>().SetTool(tool);
            toolHotbar?.HighlightTool(tool);

            // TODO: Play tool switch sound here
            // TODO: Fire UI animation or highlight here
        }

        /// <summary>
        /// Selects a tool programmatically (e.g., from UI or event).
        /// </summary>
        public void SelectToolExternally(ToolType tool)
        {
            for (int i = 0; i < toolOrder.Length; i++)
            {
                if (toolOrder[i] == tool)
                {
                    currentIndex = i;
                    SetTool(toolOrder[currentIndex]);
                    return;
                }
            }
        }

        /// <summary>
        /// Returns currently selected ToolType.
        /// </summary>
        public ToolType GetCurrentTool() => toolOrder[currentIndex];
    }
}
