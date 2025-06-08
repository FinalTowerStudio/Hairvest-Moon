using System.Collections.Generic;
using UnityEngine;
using HairvestMoon.Inventory;
using HairvestMoon.Core;
using HairvestMoon.Tool;

namespace HairvestMoon.UI
{
    /// <summary>
    /// UI for selecting and equipping a hoe tool or upgrade.
    /// Shows all available options, highlights current equip.
    /// </summary>
    public class HoeSelectionUI : MonoBehaviour, IBusListener
    {
        [SerializeField] private Transform optionParent;
        [SerializeField] private SelectionSlotUI optionPrefab;

        private List<SelectionSlotUI> _slots = new();
        private BackpackInventorySystem _backpackInventory;
        private BackpackEquipSystem _equipSystem;
        private BackpackEquipInstallManager _equipInstallManager;
        private GameEventBus _eventBus;
        private bool _isOpen = false;

        public void RegisterBusListeners()
        {
            _eventBus = ServiceLocator.Get<GameEventBus>();
            _eventBus.BackpackChanged += RefreshUI;
            _eventBus.GlobalSystemsInitialized += OnGlobalSystemsInitialized;
        }

        private void OnGlobalSystemsInitialized()
        {
            _backpackInventory = ServiceLocator.Get<BackpackInventorySystem>();
            _equipSystem = ServiceLocator.Get<BackpackEquipSystem>();
            _equipInstallManager = ServiceLocator.Get<BackpackEquipInstallManager>();
            BuildOptions();
            RefreshUI();
        }

        /// <summary>
        /// Builds option slots for all owned hoe tools/upgrades.
        /// </summary>
        private void BuildOptions()
        {
            foreach (Transform child in optionParent) Destroy(child.gameObject);
            _slots.Clear();

            foreach (var slot in _backpackInventory.Slots)
            {
                if (slot.Item != null && slot.Item.toolType == ToolType.Hoe)
                {
                    var slotUI = Instantiate(optionPrefab, optionParent);
                    slotUI.Initialize(slot.Item, OnOptionSelected);
                    _slots.Add(slotUI);
                }
            }
        }

        /// <summary>
        /// Called when player selects a hoe option.
        /// </summary>
        private void OnOptionSelected(ItemData item)
        {
            if (_equipInstallManager.TryInstallItem(item))
                RefreshUI();
            // TODO: Play sound, show feedback, close menu if you wish
        }

        /// <summary>
        /// Highlights the currently equipped hoe.
        /// </summary>
        public void RefreshUI()
        {
            foreach (var slotUI in _slots)
            {
                bool isEquipped = (_equipSystem.hoeTool == slotUI.Item || _equipSystem.hoeUpgrade == slotUI.Item);
                slotUI.SetSelected(isEquipped);
            }
        }

        /// <summary>
        /// Opens the selection menu (show/hide logic).
        /// </summary>
        public void OpenHoeMenu()
        {
            _isOpen = true;
            gameObject.SetActive(true);
            RefreshUI();
        }

        public void CloseHoeMenu()
        {
            _isOpen = false;
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Returns currently selected option (for external queries).
        /// </summary>
        public ItemData GetCurrentSelectedItem()
        {
            foreach (var slotUI in _slots)
                if (slotUI.IsSelected)
                    return slotUI.Item;
            return null;
        }
    }
}
