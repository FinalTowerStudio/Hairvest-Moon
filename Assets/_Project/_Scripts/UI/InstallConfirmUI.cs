using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HairvestMoon.Core;
using HairvestMoon.Inventory;

namespace HairvestMoon.UI
{
    public class InstallConfirmUI : MonoBehaviour
    {
        [SerializeField] private GameObject rootPanel;
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private Button installButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private CanvasGroup rootPanelCanvasGroup;

        private ItemData currentItem;

        public void InitializeUI()
        {
            Hide();
            installButton.onClick.AddListener(OnInstallClicked);
            cancelButton.onClick.AddListener(Hide);
        }

        public void Show(ItemData item)
        {
            currentItem = item;
            iconImage.sprite = item.itemIcon;
            itemNameText.text = item.itemName;
            SetPanelVisible(true);
        }

        private void OnInstallClicked()
        {
            var equipManager = ServiceLocator.Get<EquipManager>();

            if (equipManager.TryEquip(currentItem))
            {
                // Safely remove installed item from correct inventory:
                TryRemoveFromInventories(currentItem);
            }

            Hide();
        }

        private void TryRemoveFromInventories(ItemData item)
        {
            // Attempt both resource and backpack inventories
            var resourceInventory = ServiceLocator.Get<ResourceInventorySystem>();
            if (resourceInventory.RemoveItem(item, 1))
                return;

            var backpackInventory = ServiceLocator.Get<BackpackInventorySystem>();
            backpackInventory.RemoveItem(item, 1);
        }

        public void Hide()
        {
            SetPanelVisible(false);
        }
        private void SetPanelVisible(bool visible)
        {
            rootPanelCanvasGroup.alpha = visible ? 1f : 0f;
            rootPanelCanvasGroup.interactable = visible;
            rootPanelCanvasGroup.blocksRaycasts = visible;
        }
    }
}
