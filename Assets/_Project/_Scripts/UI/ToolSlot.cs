using HairvestMoon.Utility;
using UnityEngine;
using HairvestMoon.Tool;
using UnityEngine.UI;
using HairvestMoon.Core;

namespace HairvestMoon.UI
{
    /// <summary>
    /// A UI button representing a tool in the hotbar.
    /// Handles selection, highlighting, and tooltip.
    /// </summary>
    public class ToolSlot : MonoBehaviour
    {
        [SerializeField] private ToolType tool;
        [SerializeField] private Image highlightImage;
        [SerializeField] private Image toolIconImage; // Optional: if you want icons per tool

        public ToolType ToolType => tool;

        private void Awake()
        {
            var button = GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() =>
                {
                    ServiceLocator.Get<ToolSelector>().SelectToolExternally(tool);
                    ServiceLocator.Get<DebugUIOverlay>().ShowLastAction($"Tool: {tool}");
                });
            }
        }

        /// <summary>
        /// Call to highlight the slot (called by ToolHotbarUI).
        /// </summary>
        public void SetSelected(bool isSelected)
        {
            if (highlightImage != null)
                highlightImage.enabled = isSelected;
            // (Optional: change icon color, play animation, etc.)
        }

        // Tooltip support (could use ToolType info or icon as desired)
        public void OnPointerEnter()
        {
            // Show simple tooltip for ToolType
            SelectionTooltipUI.Show(new ItemData { itemName = tool.ToString(), description = $"Switch to {tool}" });
        }
        public void OnPointerExit()
        {
            SelectionTooltipUI.Hide();
        }
    }
}
