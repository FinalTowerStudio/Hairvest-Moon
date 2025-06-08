using TMPro;
using UnityEngine;
using HairvestMoon.Inventory;

namespace HairvestMoon.UI
{
    /// <summary>
    /// Shows a tooltip with name/description for an item (tool, upgrade, seed, etc).
    /// Call ShowTooltip/HideTooltip as needed.
    /// </summary>
    public class SelectionTooltipUI : MonoBehaviour
    {
        [SerializeField] private GameObject tooltipPanel;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text descriptionText;

        private static SelectionTooltipUI _instance;

        private void Awake()
        {
            // Singleton pattern for easy global access
            if (_instance == null)
                _instance = this;
            else
                Destroy(gameObject);
            HideTooltip();
        }

        /// <summary>
        /// Shows the tooltip at current mouse position for a given item.
        /// </summary>
        public static void Show(ItemData item)
        {
            if (_instance != null)
                _instance.ShowTooltip(item);
        }

        /// <summary>
        /// Hides the tooltip.
        /// </summary>
        public static void Hide()
        {
            if (_instance != null)
                _instance.HideTooltip();
        }

        /// <summary>
        /// Internal: Sets panel text and shows tooltip panel.
        /// </summary>
        public void ShowTooltip(ItemData item)
        {
            if (item == null)
            {
                nameText.text = "Normal Water";
                descriptionText.text = "";
            }
            else
            {
                nameText.text = item.itemName;
                descriptionText.text = item.description;
            }
            tooltipPanel.SetActive(true);

            // Optionally position near mouse
            Vector3 mousePos = Input.mousePosition;
            tooltipPanel.transform.position = mousePos + new Vector3(12f, -12f, 0f);
        }

        public void HideTooltip()
        {
            tooltipPanel.SetActive(false);
        }
    }
}
