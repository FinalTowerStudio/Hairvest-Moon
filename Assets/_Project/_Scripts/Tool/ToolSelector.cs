using HairvestMoon.Core;
using HairvestMoon.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HairvestMoon.Tool
{

    /// <summary>
    /// Handles tool selection via keyboard hotkeys and input actions for next/previous.
    /// Updates ToolSystem and notifies UI.
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

        public void Initialize()
        {
            SetTool(toolOrder[currentIndex]);
        }

        public void RegisterBusListeners()
        {
            var bus = ServiceLocator.Get<GameEventBus>();
            bus.ToolNext += HandleNext;
            bus.ToolPrevious += HandlePrevious;
        }

        private void Update()
        {
            // Number hotkeys (1–4)
            if (Keyboard.current.digit1Key.wasPressedThisFrame) SetToolByIndex(0);
            if (Keyboard.current.digit2Key.wasPressedThisFrame) SetToolByIndex(1);
            if (Keyboard.current.digit3Key.wasPressedThisFrame) SetToolByIndex(2);
            if (Keyboard.current.digit4Key.wasPressedThisFrame) SetToolByIndex(3);
        }

        public void HandleNext() => CycleTool(1);
        public void HandlePrevious() => CycleTool(-1);

        private void SetToolByIndex(int index)
        {
            currentIndex = Mathf.Clamp(index, 0, toolOrder.Length - 1);
            SetTool(toolOrder[currentIndex]);
        }

        private void CycleTool(int direction)
        {
            currentIndex = (currentIndex + direction + toolOrder.Length) % toolOrder.Length;
            SetTool(toolOrder[currentIndex]);
        }

        private void SetTool(ToolType tool)
        {
            ServiceLocator.Get<ToolSystem>().SetTool(tool);
            toolHotbar?.HighlightTool(tool);
        }

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


    }
}