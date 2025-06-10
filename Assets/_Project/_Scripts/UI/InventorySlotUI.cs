using UnityEngine;
using UnityEngine.UI;
using HairvestMoon.Inventory;
using HairvestMoon.Core;
using TMPro;

namespace HairvestMoon.UI
{
    /// <summary>
    /// UI slot for resource inventory (seeds, crops, materials).
    /// Displays icon, quantity, and optional selection.
    /// </summary>
    public class InventorySlotUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _quantityText;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private GameObject _highlightObj;
        [SerializeField] private Button _button;

        public ItemData Item { get; private set; }

        /// <summary>
        /// Initialize the slot with item and optional selection callback.
        /// </summary>
        public void Initialize(ItemData item, System.Action<ItemData> onSelected = null)
        {
            Item = item;
            _iconImage.sprite = item ? item.itemIcon : null;
            _highlightObj.SetActive(false);

            _button.onClick.RemoveAllListeners();
            if (onSelected != null)
                _button.onClick.AddListener(() => onSelected(Item));
        }

        /// <summary>
        /// Sets the quantity text.
        /// </summary>
        public void UpdateDisplay()
        {
            int qty = 0;
            if (Item != null)
                qty = ServiceLocator.Get<ResourceInventorySystem>().GetQuantity(Item);
            _quantityText.text = qty > 1 ? qty.ToString() : "";
            _nameText.text = Item != null ? Item.itemName : "";

        }

        public void SetSelected(bool selected)
        {
            _highlightObj.SetActive(selected);
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
