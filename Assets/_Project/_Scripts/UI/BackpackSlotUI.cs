using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HairvestMoon.UI
{
    /// <summary>
    /// A UI slot for a backpack inventory item.
    /// Shows icon, stack count, highlight, and selection.
    /// </summary>
    public class BackpackSlotUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _stackText;
        [SerializeField] private GameObject _highlightObj;
        [SerializeField] private GameObject _lockOverlay;
        [SerializeField] private Button _button;

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

            _iconImage.sprite = item ? item.itemIcon : null;
            _iconImage.enabled = item != null;
            _stackText.text = stack > 1 ? stack.ToString() : "";
            _stackText.enabled = item != null && stack > 1;
            _highlightObj.SetActive(false);

            _button.onClick.RemoveAllListeners();
            if (onSelected != null && item != null)
                _button.onClick.AddListener(OnClicked);
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

        /// <summary>
        /// Locks or unlocks the slot visually and functionally.  
        /// Will always hide visuals if locked.
        /// </summary>
        public void SetLocked(bool locked)
        {
            _lockOverlay.SetActive(locked);
            _button.interactable = !locked;
            // Also clear visuals if locked:
            if (locked)
            {
                _iconImage.enabled = false;
                _stackText.enabled = false;
            }
        }
    }
}
