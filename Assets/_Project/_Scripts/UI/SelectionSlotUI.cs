using UnityEngine;
using UnityEngine.UI;
using HairvestMoon.Inventory;

namespace HairvestMoon.UI
{
    /// <summary>
    /// Generic UI slot for selection menus (tools, seeds, upgrades).
    /// Handles highlighting and click events.
    /// </summary>
    public class SelectionSlotUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image iconImage;
        [SerializeField] private GameObject highlightObj;
        [SerializeField] private Button button;

        public ItemData Item { get; private set; }
        public bool IsSelected { get; private set; }

        private System.Action<ItemData> _onSelected;

        /// <summary>
        /// Initialize the slot with its item and callback.
        /// </summary>
        public void Initialize(ItemData item, System.Action<ItemData> onSelected)
        {
            Item = item;
            _onSelected = onSelected;
            iconImage.sprite = item ? item.itemIcon : null;
            highlightObj.SetActive(false);
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClicked);
        }

        private void OnClicked()
        {
            _onSelected?.Invoke(Item);
        }

        /// <summary>
        /// Sets the selected highlight state.
        /// </summary>
        public void SetSelected(bool selected)
        {
            IsSelected = selected;
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
