using UnityEngine;
using HairvestMoon.Inventory;
using System.Collections.Generic;
using HairvestMoon.Core;

namespace HairvestMoon.UI
{
    public class HoeSelectionUI : MonoBehaviour, IBusListener
    {
        [Header("UI References")]
        [SerializeField] private GameObject hoeSelectionSlotPrefab;
        [SerializeField] private Transform gridParent;

        private List<UpgradeSelectionSlot> slots = new();
        private ItemData currentSelectedHoeOption;
        private CanvasGroup hoeSelectionCanvasGroup;

        public void InitializeUI()
        {
            hoeSelectionCanvasGroup = GetComponent<CanvasGroup>();
            BuildUI();
        }

        public void RegisterBusListeners()
        {
            var bus = ServiceLocator.Get<GameEventBus>();
            bus.BackpackChanged += RefreshUI;
        }

        public void OpenHoeMenu()
        {
            gameObject.SetActive(true);
            BuildUI();
            hoeSelectionCanvasGroup.alpha = 1f;
            hoeSelectionCanvasGroup.interactable = true;
            hoeSelectionCanvasGroup.blocksRaycasts = true;
        }

        public void CloseHoeMenu()
        {
            gameObject.SetActive(false);
            hoeSelectionCanvasGroup.alpha = 0f;
            hoeSelectionCanvasGroup.interactable = false;
            hoeSelectionCanvasGroup.blocksRaycasts = false;
        }

        private void BuildUI()
        {
            foreach (Transform child in gridParent)
                Destroy(child.gameObject);
            slots.Clear();

            // Always add Normal Hoe option
            var slotGO = Instantiate(hoeSelectionSlotPrefab, gridParent);
            var slotUI = slotGO.GetComponent<UpgradeSelectionSlot>();
            slotUI.Initialize(null, OnHoeOptionSelected);
            slotUI.SetSelected(currentSelectedHoeOption == null);
            slots.Add(slotUI);

            // If we have Hoe Upgrade equipped, enable selection
            var hoeUpgrade = ServiceLocator.Get<BackpackEquipSystem>().hoeUpgrade;
            if (hoeUpgrade != null)
            {
                var upgradeGO = Instantiate(hoeSelectionSlotPrefab, gridParent);
                var upgradeSlotUI = upgradeGO.GetComponent<UpgradeSelectionSlot>();
                upgradeSlotUI.Initialize(hoeUpgrade, OnHoeOptionSelected);
                upgradeSlotUI.SetSelected(hoeUpgrade == currentSelectedHoeOption);
                slots.Add(upgradeSlotUI);
            }
        }

        private void RefreshUI()
        {
            BuildUI();
        }

        private void OnHoeOptionSelected(ItemData selectedItem)
        {
            currentSelectedHoeOption = selectedItem;

            foreach (var slot in slots)
                slot.SetSelected(slot.Item == currentSelectedHoeOption);
        }

        public ItemData GetCurrentSelectedItem()
        {
            return currentSelectedHoeOption;
        }
    }
}
