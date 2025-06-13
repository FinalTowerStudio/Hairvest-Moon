using System.Collections.Generic;
using UnityEngine;
using HairvestMoon.Inventory;
using HairvestMoon.Core;

namespace HairvestMoon.UI
{
    /// <summary>
    /// Handles the player’s backpack UI grid, updating slot displays and tool/upgrade slots.
    /// Responds to inventory and equip events.
    /// </summary>
    public class BackpackInventoryUI : MonoBehaviour, IBusListener
    {
        [Header("UI References")]
        [SerializeField] private Transform backpackGridParent;
        [SerializeField] private BackpackSlotUI slotPrefab;
        [SerializeField] private GameObject emptyGridPrefab;
        [SerializeField] private GameObject lockedGridPrefab;
        [SerializeField] private ItemDescriptionUI itemDescriptionUI;
        [SerializeField] private BackpackCapacityBarUI capacityBarUI;

        [Header("Tool Slots")]
        [SerializeField] private EquipSlotUI hoeToolSlot;
        [SerializeField] private EquipSlotUI wateringToolSlot;
        [SerializeField] private EquipSlotUI seedToolSlot;
        [SerializeField] private EquipSlotUI harvestToolSlot;

        [Header("Upgrade Slots")]
        [SerializeField] private EquipSlotUI hoeUpgradeSlot;
        [SerializeField] private EquipSlotUI wateringUpgradeSlot;
        [SerializeField] private EquipSlotUI seedUpgradeSlot;
        [SerializeField] private EquipSlotUI harvestUpgradeSlot;

        // Maps slot index to its UI element
        private readonly List<BackpackSlotUI> _slots = new();
        private ItemData _currentSelectedItem;
        private BackpackSlotUI _currentlySelectedBackpackSlot = null;
        private EquipSlotUI _currentlySelectedEquipSlot = null;

        private BackpackInventorySystem _backpackInventory;
        private InstallConfirmUI _installConfirmUI;

        public void InitializeUI()
        {
            _backpackInventory = ServiceLocator.Get<BackpackInventorySystem>();
            _installConfirmUI = ServiceLocator.Get<InstallConfirmUI>();
        }

        public void RegisterBusListeners()
        {
            var bus = ServiceLocator.Get<GameEventBus>();
            bus.GlobalSystemsInitialized += OnGlobalSystemsInitialized;
            bus.BackpackChanged += RefreshUI;
            bus.ItemInstalled += OnItemInstalled;
        }

        private void OnGlobalSystemsInitialized()
        {
            BuildUI();
            RefreshUI();
        }

        /// <summary>
        /// Builds the UI grid. Only call this when slot/capacity structure changes!
        /// </summary>
        private void BuildUI()
        {
            foreach (Transform child in backpackGridParent)
                Destroy(child.gameObject);
            _slots.Clear();

            int desiredSlots = BackpackInventorySystem.MaxSlots;

            for (int i = 0; i < desiredSlots; i++)
            {
                var slotUI = Instantiate(slotPrefab, backpackGridParent);
                slotUI.SetLocked(_backpackInventory != null && i >= _backpackInventory.UnlockedSlots);
                _slots.Add(slotUI);
            }
        }


        public void RefreshUI()
        {
            if (_backpackInventory == null || _backpackInventory.Slots == null)
            {
                Debug.LogWarning("BackpackInventoryUI: No inventory system or slots available!");
                return;
            }

            int slotCount = Mathf.Min(_slots.Count, _backpackInventory.Slots.Count, _backpackInventory.UnlockedSlots);
            for (int i = 0; i < slotCount; i++)
            {
                var slotData = _backpackInventory.Slots[i];
                var item = slotData != null ? slotData.Item : null;
                _slots[i].SetLocked(i >= _backpackInventory.UnlockedSlots);
                if (i < _backpackInventory.UnlockedSlots)
                    _slots[i].Initialize(item, slotData != null ? slotData.Stack : 0, OnSlotSelected);
                else
                    _slots[i].Initialize(null, 0, null);
            }
            for (int i = slotCount; i < _slots.Count; i++)
            {
                _slots[i].SetLocked(true);
                _slots[i].Initialize(null, 0, null);
            }

            var equipSystem = ServiceLocator.Get<BackpackEquipSystem>();

            hoeToolSlot.SetItem(equipSystem.hoeTool, equipSystem.hoeTool != null, OnEquipSlotClicked);
            hoeUpgradeSlot.SetItem(equipSystem.hoeUpgrade, equipSystem.hoeUpgrade != null, OnEquipSlotClicked);

            wateringToolSlot.SetItem(equipSystem.wateringTool, equipSystem.wateringTool != null, OnEquipSlotClicked);
            wateringUpgradeSlot.SetItem(equipSystem.wateringUpgrade, equipSystem.wateringUpgrade != null, OnEquipSlotClicked);

            seedToolSlot.SetItem(equipSystem.seedTool, equipSystem.seedTool != null, OnEquipSlotClicked);
            seedUpgradeSlot.SetItem(equipSystem.seedUpgrade, equipSystem.seedUpgrade != null, OnEquipSlotClicked);

            harvestToolSlot.SetItem(equipSystem.harvestTool, equipSystem.harvestTool != null, OnEquipSlotClicked);
            harvestUpgradeSlot.SetItem(equipSystem.harvestUpgrade, equipSystem.harvestUpgrade != null, OnEquipSlotClicked);

        }

        private void OnEquipSlotClicked(ItemData equippedItem)
        {
            // Clear highlights on all backpack slots
            foreach (var slot in _slots)
                slot.SetSelected(false);

            // Clear highlights on all equip slots
            ClearEquipSlotHighlights();

            // Find and set the clicked equip slot as selected
            _currentlySelectedEquipSlot = GetEquipSlotUIForItem(equippedItem);
            if (_currentlySelectedEquipSlot != null)
                _currentlySelectedEquipSlot.SetHighlight(true);

            // Clear backpack slot selection
            _currentlySelectedBackpackSlot = null;

            itemDescriptionUI.SetItem(equippedItem);
            if (_installConfirmUI != null)
                _installConfirmUI.ShowForUninstall(equippedItem);
        }

        private void OnSlotSelected(ItemData selectedItem)
        {
            // Clear highlights on ALL backpack slots
            foreach (var slot in _slots)
                slot.SetSelected(false);

            // Clear highlight on ALL equip slots
            ClearEquipSlotHighlights();

            // Find and set the clicked slot as selected
            _currentlySelectedBackpackSlot = _slots.Find(slot => slot.Item == selectedItem);
            if (_currentlySelectedBackpackSlot != null)
                _currentlySelectedBackpackSlot.SetSelected(true);

            // Clear equip slot selection
            _currentlySelectedEquipSlot = null;

            _currentSelectedItem = selectedItem;
            UpdateSelection(selectedItem);

            itemDescriptionUI.SetItem(selectedItem);

            if (_installConfirmUI != null)
                _installConfirmUI.ShowForInstall(selectedItem);
        }

        /// <summary>
        /// Visually highlights the selected slot and shows its details.
        /// Always clamps access to model and UI slot counts and unlocked slots for safety.
        /// </summary>
        private void UpdateSelection(ItemData selectedItem)
        {
#if UNITY_EDITOR
            Debug.Log($"[BackpackInventoryUI] UpdateSelection: UI slots: {_slots.Count}, Model slots: {_backpackInventory.Slots.Count}, Unlocked: {_backpackInventory.UnlockedSlots}");
#endif

            int slotCount = Mathf.Min(_slots.Count, _backpackInventory.Slots.Count, _backpackInventory.UnlockedSlots);
            for (int i = 0; i < slotCount; i++)
            {
                var slotData = _backpackInventory.Slots[i];
                var slotItem = slotData != null ? slotData.Item : null;
                _slots[i].SetSelected(slotItem == selectedItem);
            }
            // Deselect any extra UI slots
            for (int i = slotCount; i < _slots.Count; i++)
                _slots[i].SetSelected(false);

            if (selectedItem != null)
                itemDescriptionUI.SetItem(selectedItem);
            else
                itemDescriptionUI.Clear();
        }

        private void ClearEquipSlotHighlights()
        {
            hoeToolSlot.SetHighlight(false);
            wateringToolSlot.SetHighlight(false);
            seedToolSlot.SetHighlight(false);
            harvestToolSlot.SetHighlight(false);

            hoeUpgradeSlot.SetHighlight(false);
            wateringUpgradeSlot.SetHighlight(false);
            seedUpgradeSlot.SetHighlight(false);
            harvestUpgradeSlot.SetHighlight(false);
        }

        private EquipSlotUI GetEquipSlotUIForItem(ItemData item)
        {
            if (item == null) return null;
            if (hoeToolSlot.Item == item) return hoeToolSlot;
            if (wateringToolSlot.Item == item) return wateringToolSlot;
            if (seedToolSlot.Item == item) return seedToolSlot;
            if (harvestToolSlot.Item == item) return harvestToolSlot;
            if (hoeUpgradeSlot.Item == item) return hoeUpgradeSlot;
            if (wateringUpgradeSlot.Item == item) return wateringUpgradeSlot;
            if (seedUpgradeSlot.Item == item) return seedUpgradeSlot;
            if (harvestUpgradeSlot.Item == item) return harvestUpgradeSlot;
            return null;
        }


        /// <summary>
        /// Rebuilds UI after equip/install events.
        /// </summary>
        private void OnItemInstalled(ItemInstalledEventArgs args)
        {
            RefreshUI();
        }
    }
}
