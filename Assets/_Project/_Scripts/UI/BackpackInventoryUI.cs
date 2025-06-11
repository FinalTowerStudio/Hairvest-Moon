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

        private BackpackInventorySystem _backpackInventory;

        public void InitializeUI()
        {
            _backpackInventory = ServiceLocator.Get<BackpackInventorySystem>();
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

            for (int i = 0; i < BackpackInventorySystem.MaxSlots; i++)
            {
                var slotUI = Instantiate(slotPrefab, backpackGridParent);
                slotUI.SetLocked(i >= _backpackInventory.UnlockedSlots);
                _slots.Add(slotUI);
            }
        }

        public void RefreshUI()
        {
            for (int i = 0; i < BackpackInventorySystem.MaxSlots; i++)
            {
                bool isUnlocked = i < _backpackInventory.UnlockedSlots;
                var data = _backpackInventory.Slots[i];
                _slots[i].SetLocked(!isUnlocked);

                if (isUnlocked)
                    _slots[i].Initialize(data.Item, data.Stack, OnSlotSelected);
                else
                    _slots[i].Initialize(null, 0, null); // locked slot, clear display
            }
        }

        private void OnSlotSelected(ItemData selectedItem)
        {
            _currentSelectedItem = selectedItem;
            UpdateSelection(selectedItem);
        }

        /// <summary>
        /// Visually highlights the selected slot and shows its details.
        /// </summary>
        private void UpdateSelection(ItemData selectedItem)
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                var slotItem = _backpackInventory.Slots[i].Item;
                _slots[i].SetSelected(slotItem == selectedItem);
            }

            if (selectedItem != null)
                itemDescriptionUI.SetItem(selectedItem);
            else
                itemDescriptionUI.Clear();
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
