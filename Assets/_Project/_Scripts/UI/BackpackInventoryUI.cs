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
        [SerializeField] private GameObject backpackSlotPrefab;
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

        private readonly Dictionary<ItemData, BackpackSlotUI> slots = new();
        private ItemData _currentSelectedItem;

        private BackpackUpgradeManager _upgradeManager;
        private BackpackInventorySystem _backpackInventory;

        public void InitializeUI()
        {
            _upgradeManager = ServiceLocator.Get<BackpackUpgradeManager>();
            _backpackInventory = ServiceLocator.Get<BackpackInventorySystem>();
            BuildUI();
        }

        public void RegisterBusListeners()
        {
            var bus = ServiceLocator.Get<GameEventBus>();
            bus.BackpackChanged += RefreshUI;
            bus.ItemInstalled += OnItemInstalled;
        }

        /// <summary>
        /// Builds the UI grid. Only call this when slot/capacity structure changes!
        /// </summary>
        private void BuildUI()
        {
            foreach (Transform child in backpackGridParent)
                Destroy(child.gameObject);

            slots.Clear();

            int unlockedSlots = _upgradeManager.GetCurrentSlots();
            var allBackpackSlots = _backpackInventory.Slots;

            int filledIndex = 0;
            for (int i = 0; i < unlockedSlots; i++)
            {
                if (filledIndex < allBackpackSlots.Count && allBackpackSlots[filledIndex].Item != null)
                {
                    var slotGO = Instantiate(backpackSlotPrefab, backpackGridParent);
                    var slot = slotGO.GetComponent<BackpackSlotUI>();
                    var data = allBackpackSlots[filledIndex];
                    slot.Initialize(data.Item, data.Stack, OnSlotSelected);
                    slots[data.Item] = slot;
                    filledIndex++;
                }
                else
                {
                    Instantiate(emptyGridPrefab, backpackGridParent);
                }
            }
            // TODO: Add lockedGridPrefab for locked slots if using upgrades beyond current unlocks
        }

        /// <summary>
        /// Refreshes slot visuals and selection (after inventory changes).
        /// </summary>
        public void RefreshUI()
        {
            BuildUI(); // For jam, this is fine. For polish, consider only updating contents, not structure.
            UpdateSelection(_currentSelectedItem);
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
            foreach (var pair in slots)
                pair.Value.SetSelected(pair.Key == selectedItem);

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
