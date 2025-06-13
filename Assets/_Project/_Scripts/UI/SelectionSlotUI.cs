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

        [Header("Hand Icon")]
        [SerializeField] private Sprite handIcon;

        public ItemData Item { get; private set; }
        public bool IsSelected { get; private set; }

        private System.Action<ItemData> _onSelected;
        private string _handsTitle = "Hands (Nothing Equipped)";
        private string _handsDescription = "No tool equipped for this slot.";

        /// <summary>
        /// Call before Initialize if this slot represents "hands"/empty.
        /// </summary>
        public void SetHandsTooltip(string title, string description)
        {
            _handsTitle = title;
            _handsDescription = description;
        }

        /// <summary>
        /// Initialize the slot with its item and callback.
        /// </summary>
        public void Initialize(ItemData item, System.Action<ItemData> onSelected)
        {
            Item = item;
            _onSelected = onSelected;
            iconImage.sprite = item ? item.itemIcon : handIcon;
            iconImage.color = item ? Color.white : new Color(1f, 1f, 1f, 0.66f); // faded if hands
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
            else
                SelectionTooltipUI.ShowHandsTooltip(_handsTitle, _handsDescription);
        }

        public void OnPointerExit()
        {
            SelectionTooltipUI.Hide();
        }
    }
}
