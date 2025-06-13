using HairvestMoon.Core;
using HairvestMoon.Inventory;
using HairvestMoon.Tool;
using System.Collections.Generic;
using UnityEngine;

namespace HairvestMoon.UI
{
    /// <summary>
    /// UI for selecting a watering can upgrade or fertilizer.
    /// Only shows options present in the player's backpack inventory.
    /// Highlights selected.
    /// </summary>
    public class WateringSelectionUI : MonoBehaviour, IBusListener
    {
        [Header("UI References")]
        [SerializeField] private GameObject wateringSlotPrefab;
        [SerializeField] private Transform wateringGridParent;

        private List<SelectionSlotUI> slots = new();
        private ItemData _currentSelectedItem;
        private CanvasGroup _canvasGroup;
        private BackpackInventorySystem _backpackInventory;
        private BackpackEquipSystem _equipSystem;

        public void RegisterBusListeners()
        {
            var bus = ServiceLocator.Get<GameEventBus>();
            bus.BackpackChanged += RefreshUI;
            bus.GlobalSystemsInitialized += OnGlobalSystemsInitialized;
        }

        private void OnGlobalSystemsInitialized()
        {
            _backpackInventory = ServiceLocator.Get<BackpackInventorySystem>();
            _equipSystem = ServiceLocator.Get<BackpackEquipSystem>();
            _canvasGroup = GetComponent<CanvasGroup>();
            BuildUI();
        }

        private void BuildUI()
        {
            foreach (Transform child in wateringGridParent)
                Destroy(child.gameObject);

            slots.Clear();

            var equippedWatering = _equipSystem.wateringTool;

            // Always show hands slot
            var handsGO = Instantiate(wateringSlotPrefab, wateringGridParent);
            var handsSlotUI = handsGO.GetComponent<SelectionSlotUI>();
            handsSlotUI.SetHandsTooltip(
                "Hands (Nothing Equipped)",
                "You have not equipped a watering can. Equip one to water crops!"
            );
            handsSlotUI.Initialize(null, OnWateringSelected);
            handsSlotUI.SetSelected(equippedWatering == null);
            slots.Add(handsSlotUI);

            // Show equipped watering tool if present
            if (equippedWatering != null)
            {
                var slotGO = Instantiate(wateringSlotPrefab, wateringGridParent);
                var slotUI = slotGO.GetComponent<SelectionSlotUI>();
                slotUI.Initialize(equippedWatering, OnWateringSelected);
                slotUI.SetSelected(true);
                slots.Add(slotUI);
            }
        }

        private void OnWateringSelected(ItemData selectedItem)
        {
            _currentSelectedItem = selectedItem;
            foreach (var slot in slots)
                slot.SetSelected(slot.Item == _currentSelectedItem);

            _equipSystem.SetEquippedItem(ToolType.WateringCan, selectedItem);
        }

        public ItemData GetCurrentSelectedItem() => _currentSelectedItem;

        private void RefreshUI()
        {
            BuildUI();
        }

        public void OpenWateringMenu()
        {
            gameObject.SetActive(true);
            BuildUI();
            SetCanvasVisible(true);
        }

        public void CloseWateringMenu()
        {
            SetCanvasVisible(false);
        }

        private void SetCanvasVisible(bool visible)
        {
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.alpha = visible ? 1f : 0f;
            _canvasGroup.interactable = visible;
            _canvasGroup.blocksRaycasts = visible;
        }
    }
}
