using TMPro;
using UnityEngine;
using UnityEngine.UI;
using HairvestMoon.UI;
using HairvestMoon.Inventory;
using HairvestMoon.Core;

namespace HairvestMoon.UI
{
    public class BackpackSlotUI : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI quantityText;
        [SerializeField] private Image highlightImage;
        [SerializeField] private Button selectButton;

        private ItemData item;
        private System.Action<ItemData> onClick;
        private InstallConfirmUI installConfirmUI;

        public void Initialize(ItemData itemData, int quantity, System.Action<ItemData> clickCallback)
        {
            item = itemData;
            onClick = clickCallback;

            iconImage.sprite = item.itemIcon;
            iconImage.color = Color.white;
            quantityText.text = quantity > 1 ? quantity.ToString() : "";

            selectButton.onClick.AddListener(OnClicked);
            SetSelected(false);

            installConfirmUI = ServiceLocator.Get<InstallConfirmUI>();
        }

        private void OnClicked()
        {
            installConfirmUI.Show(item);
        }

        public void SetSelected(bool isSelected)
        {
            highlightImage.gameObject.SetActive(isSelected);
        }
    }
}
