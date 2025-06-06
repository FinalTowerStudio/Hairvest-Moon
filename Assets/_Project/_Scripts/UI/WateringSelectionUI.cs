using System.Collections.Generic;
using UnityEngine;
using HairvestMoon.Inventory;
using HairvestMoon.Farming;
using HairvestMoon.Core;

namespace HairvestMoon.UI
{
    public class WateringSelectionUI : MonoBehaviour, IBusListener
    {
        [Header("UI References")]
        [SerializeField] private GameObject waterSelectionSlotPrefab;
        [SerializeField] private Transform gridParent;
        [SerializeField] private FarmToolHandler farmToolHandler;

        private List<UpgradeSelectionSlot> slots = new();
        private ItemData currentSelectedWatering;
        private CanvasGroup wateringSelectionCanvasGroup;

        public void InitializeUI()
        {
            wateringSelectionCanvasGroup = GetComponent<CanvasGroup>();
            BuildUI();
        }

        public void RegisterBusListeners()
        {
            var bus = ServiceLocator.Get<GameEventBus>();
            bus.BackpackChanged += RefreshUI;
        }

        public void OpenWateringMenu()
        {
            gameObject.SetActive(true);
            BuildUI();
            wateringSelectionCanvasGroup.alpha = 1f;
            wateringSelectionCanvasGroup.interactable = true;
            wateringSelectionCanvasGroup.blocksRaycasts = true;
        }

        public void CloseWateringMenu()
        {
            wateringSelectionCanvasGroup.alpha = 0f;
            wateringSelectionCanvasGroup.interactable = false;
            wateringSelectionCanvasGroup.blocksRaycasts = false;
        }

        private void BuildUI()
        {
            foreach (Transform child in gridParent)
                Destroy(child.gameObject);
            slots.Clear();

            // Always add Normal Water option first
            var slotGO = Instantiate(waterSelectionSlotPrefab, gridParent);
            var slotUI = slotGO.GetComponent<UpgradeSelectionSlot>();
            slotUI.Initialize(null, OnWateringOptionSelected);
            slotUI.SetSelected(currentSelectedWatering == null);
            slots.Add(slotUI);

            // If we have a Fertilizer Sprayer equipped, enable fertilizer selection
            var wateringUpgrade = ServiceLocator.Get<BackpackEquipSystem>().wateringUpgrade;
            if (wateringUpgrade != null)
            {
                var allBackpackSlots = ServiceLocator.Get<BackpackInventorySystem>().GetAllSlots();

                foreach (var slot in allBackpackSlots)
                {
                    if (slot.item.itemType == ItemType.Fertilizer)
                    {
                        var fertSlotGO = Instantiate(waterSelectionSlotPrefab, gridParent);
                        var fertSlotUI = fertSlotGO.GetComponent<UpgradeSelectionSlot>();
                        fertSlotUI.Initialize(slot.item, OnWateringOptionSelected);
                        fertSlotUI.SetSelected(slot.item == currentSelectedWatering);
                        slots.Add(fertSlotUI);
                    }
                }
            }
        }

        private void RefreshUI()
        {
            BuildUI();
        }

        private void OnWateringOptionSelected(ItemData selectedItem)
        {
            currentSelectedWatering = selectedItem;
            //farmToolHandler.SetSelectedWateringOption(selectedItem);  // We'll wire this into actual logic later

            foreach (var slot in slots)
            {
                slot.SetSelected(slot.Item == currentSelectedWatering);
            }
        }

        public ItemData GetCurrentSelectedItem()
        {
            return currentSelectedWatering;
        }
    }
}
