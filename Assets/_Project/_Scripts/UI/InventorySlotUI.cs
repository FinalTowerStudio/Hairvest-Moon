using UnityEngine;
using UnityEngine.UI;
using HairvestMoon.Inventory;
using HairvestMoon.Core;

namespace HairvestMoon.UI
{
    /// <summary>
    /// UI slot for resource inventory (seeds, crops, materials).
    /// Displays icon, quantity, and optional selection.
    /// </summary>
    public class InventorySlotUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Text quantityText;
        [SerializeField] private GameObject highlightObj;
        [SerializeField] private Button button;

        public ItemData Item { get; private set; }

        /// <summary>
        /// Initialize the slot with item and optional selection callback.
        /// </summary>
        public void Initialize(ItemData item, System.Action<ItemData> onSelected = null)
        {
            Item = item;
            iconImage.sprite = item ? item.itemIcon : null;
            highlightObj.SetActive(false);

            button.onClick.RemoveAllListeners();
            if (onSelected != null)
                button.onClick.AddListener(() => onSelected(Item));
        }

        /// <summary>
        /// Sets the quantity text.
        /// </summary>
        public void UpdateDisplay()
        {
            int qty = 0;
            if (Item != null)
                qty = ServiceLocator.Get<ResourceInventorySystem>().GetQuantity(Item);
            quantityText.text = qty > 1 ? qty.ToString() : "";
        }

        public void SetSelected(bool selected)
        {
            highlightObj.SetActive(selected);
        }

        public void OnPointerEnter()
        {
            if (Item != null)
                SelectionTooltipUI.Show(Item);
        }
        public void OnPointerExit()
        {
            SelectionTooltipUI.Hide();
        }
    }
}
