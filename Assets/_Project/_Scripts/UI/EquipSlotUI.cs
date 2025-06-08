using UnityEngine;
using UnityEngine.UI;
using HairvestMoon.Inventory;

namespace HairvestMoon.UI
{
    /// <summary>
    /// Displays a single equipped tool or upgrade slot in the UI.
    /// Handles icon, highlight, tooltip, and state.
    /// </summary>
    public class EquipSlotUI : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private GameObject highlightObj;
        [SerializeField] private Button button;

        public ItemData Item { get; private set; }
        private System.Action<ItemData> _onClicked;

        /// <summary>
        /// Sets the slot icon, equipped highlight, and click/tooltip behavior.
        /// </summary>
        public void SetItem(ItemData item, bool isEquipped, System.Action<ItemData> onClicked = null)
        {
            Item = item;
            iconImage.sprite = item ? item.itemIcon : null;
            highlightObj.SetActive(isEquipped);
            gameObject.SetActive(item != null);

            _onClicked = onClicked;
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                if (_onClicked != null)
                    button.onClick.AddListener(() => _onClicked(Item));
            }
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
