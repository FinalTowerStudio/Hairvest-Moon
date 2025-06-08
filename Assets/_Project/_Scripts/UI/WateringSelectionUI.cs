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

        /// <summary>
        /// Builds the selection grid for watering upgrades/fertilizers owned.
        /// </summary>
        private void BuildUI()
        {
            foreach (Transform child in wateringGridParent)
                Destroy(child.gameObject);

            slots.Clear();
            List<ItemData> wateringOptions = new();

            foreach (var slot in _backpackInventory.Slots)
            {
                if (slot.Item != null && IsWateringOption(slot.Item))
                    wateringOptions.Add(slot.Item);
            }

            if (wateringOptions.Count == 0)
            {
                _currentSelectedItem = null;
                return;
            }

            foreach (var item in wateringOptions)
            {
                var slotGO = Instantiate(wateringSlotPrefab, wateringGridParent);
                var slotUI = slotGO.GetComponent<SelectionSlotUI>();
                slotUI.Initialize(item, OnWateringSelected);
                slotUI.SetSelected(item == _currentSelectedItem);
                slots.Add(slotUI);
            }

            if (_currentSelectedItem == null)
                OnWateringSelected(wateringOptions[0]);
        }

        /// <summary>
        /// Define what counts as a watering option (upgrade, fertilizer, etc).
        /// Adjust as needed for your game’s logic.
        /// </summary>
        private bool IsWateringOption(ItemData item)
        {
            // Could check itemType == Upgrade && toolType == WateringCan
            // Or check for fertilizer tags—expand as your design requires
            return item.itemType == ItemType.Upgrade && item.toolType == ToolType.WateringCan;
        }

        private void OnWateringSelected(ItemData selectedItem)
        {
            _currentSelectedItem = selectedItem;
            foreach (var slot in slots)
                slot.SetSelected(slot.Item == _currentSelectedItem);

            // Optionally: Call a system to actually "use" the selection
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
