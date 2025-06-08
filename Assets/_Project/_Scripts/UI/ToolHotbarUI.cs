using System.Collections.Generic;
using UnityEngine;
using HairvestMoon.Tool;
using HairvestMoon.Core;

namespace HairvestMoon.UI
{
    /// <summary>
    /// Displays current tool hotbar and highlights selected tool.
    /// </summary>
    public class ToolHotbarUI : MonoBehaviour, IBusListener
    {
        [SerializeField] private List<ToolSlot> toolSlots; // Each slot knows its ToolType

        private ToolType _currentTool;
        private GameEventBus _eventBus;

        public void RegisterBusListeners()
        {
            _eventBus = ServiceLocator.Get<GameEventBus>();
            _eventBus.ToolChanged += OnToolChanged;
            _eventBus.GlobalSystemsInitialized += OnGlobalSystemsInitialized;
        }

        private void OnGlobalSystemsInitialized()
        {
            // Optionally: refresh highlight at start
            HighlightTool(_currentTool);
        }

        private void OnToolChanged(ToolType tool)
        {
            _currentTool = tool;
            HighlightTool(tool);
            // TODO: Play feedback animation or SFX for tool swap
        }

        public void HighlightTool(ToolType tool)
        {
            foreach (var slot in toolSlots)
                slot.SetSelected(slot.ToolType == tool);
        }
    }
}
