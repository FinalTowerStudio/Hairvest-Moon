using System.Collections.Generic;
using UnityEngine;
using HairvestMoon.Inventory;
using HairvestMoon.Farming;
using HairvestMoon.UI;
using HairvestMoon.Core;

namespace HairvestMoon.UI
{
    public class HarvestSelectionUI : MonoBehaviour, IBusListener
    {
        [Header("UI References")]
        [SerializeField] private GameObject harvestSelectionSlotPrefab;
        [SerializeField] private Transform gridParent;

        private List<UpgradeSelectionSlot> slots = new();
        private ItemData currentSelectedHarvestOption;
        private CanvasGroup harvestSelectionCanvasGroup;

        public void InitializeUI()
        {
            harvestSelectionCanvasGroup = GetComponent<CanvasGroup>();
            BuildUI();
        }

        public void RegisterBusListeners()
        {
            var bus = ServiceLocator.Get<GameEventBus>();
            bus.BackpackChanged += RefreshUI;
        }

        public void OpenHarvestMenu()
        {
            gameObject.SetActive(true);
            BuildUI();
            harvestSelectionCanvasGroup.alpha = 1f;
            harvestSelectionCanvasGroup.interactable = true;
            harvestSelectionCanvasGroup.blocksRaycasts = true;
        }

        public void CloseHarvestMenu()
        {
            harvestSelectionCanvasGroup.alpha = 0f;
            harvestSelectionCanvasGroup.interactable = false;
            harvestSelectionCanvasGroup.blocksRaycasts = false;
        }

        private void BuildUI()
        {
            foreach (Transform child in gridParent)
                Destroy(child.gameObject);
            slots.Clear();

            // Always add Normal Harvest option
            var slotGO = Instantiate(harvestSelectionSlotPrefab, gridParent);
            var slotUI = slotGO.GetComponent<UpgradeSelectionSlot>();
            slotUI.Initialize(null, OnHarvestOptionSelected);
            slotUI.SetSelected(currentSelectedHarvestOption == null);
            slots.Add(slotUI);

            // If we have Harvest Upgrade equipped, enable selection
            var harvestUpgrade = ServiceLocator.Get<BackpackEquipSystem>().harvestUpgrade;
            if (harvestUpgrade != null)
            {
                var upgradeGO = Instantiate(harvestSelectionSlotPrefab, gridParent);
                var upgradeSlotUI = upgradeGO.GetComponent<UpgradeSelectionSlot>();
                upgradeSlotUI.Initialize(harvestUpgrade, OnHarvestOptionSelected);
                upgradeSlotUI.SetSelected(harvestUpgrade == currentSelectedHarvestOption);
                slots.Add(upgradeSlotUI);
            }
        }

        private void RefreshUI()
        {
            BuildUI();
        }

        private void OnHarvestOptionSelected(ItemData selectedItem)
        {
            currentSelectedHarvestOption = selectedItem;

            foreach (var slot in slots)
                slot.SetSelected(slot.Item == currentSelectedHarvestOption);
        }

        public ItemData GetCurrentSelectedItem()
        {
            return currentSelectedHarvestOption;
        }
    }
}
