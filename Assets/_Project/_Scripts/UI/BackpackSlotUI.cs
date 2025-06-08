using UnityEngine;
using UnityEngine.UI;
using HairvestMoon.Inventory;

namespace HairvestMoon.UI
{
    /// <summary>
    /// A UI slot for a backpack inventory item.
    /// Shows icon, stack count, highlight, and selection.
    /// </summary>
    public class BackpackSlotUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Text stackText;
        [SerializeField] private GameObject highlightObj;
        [SerializeField] private Button button;

        public ItemData Item { get; private set; }
        private int _stack;
        private System.Action<ItemData> _onSelected;

        /// <summary>
        /// Initialize with item, stack, and selection callback.
        /// </summary>
        public void Initialize(ItemData item, int stack, System.Action<ItemData> onSelected)
        {
            Item = item;
            _stack = stack;
            _onSelected = onSelected;

            iconImage.sprite = item ? item.itemIcon : null;
            stackText.text = stack > 1 ? stack.ToString() : "";
            highlightObj.SetActive(false);

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClicked);
        }

        private void OnClicked()
        {
            _onSelected?.Invoke(Item);
        }

        /// <summary>
        /// Visually highlights if selected.
        /// </summary>
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
