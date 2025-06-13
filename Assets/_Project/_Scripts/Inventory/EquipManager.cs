using HairvestMoon.Core;
using HairvestMoon.Tool;

namespace HairvestMoon.Inventory
{
    /// <summary>
    /// Handles the logic for installing (equipping) or uninstalling (unequipping) tools/upgrades.
    /// Mutates BackpackInventorySystem and BackpackEquipSystem. Fires events for UI updates.
    /// </summary>
    public class EquipManager : IBusListener
    {
        private BackpackEquipSystem _equipSystem;
        private BackpackInventorySystem _backpackInventory;
        private GameEventBus _eventBus;
        private bool _isInitialized = false;

        public void RegisterBusListeners()
        {
            _eventBus = ServiceLocator.Get<GameEventBus>();
            _eventBus.GlobalSystemsInitialized += OnGlobalSystemsInitialized;
        }

        private void OnGlobalSystemsInitialized()
        {
            _equipSystem = ServiceLocator.Get<BackpackEquipSystem>();
            _backpackInventory = ServiceLocator.Get<BackpackInventorySystem>();
            _isInitialized = true;
        }
        public void InstallItem(ItemData item)
        {
            if (!_isInitialized || item == null) return;

            var slotType = _equipSystem.GetEquipSlotType(item);

            if (item.itemType == ItemType.Tool)
            {
                // Remove tool from inventory
                if (!_backpackInventory.RemoveItem(item, 1)) return;

                // Swap out any tool already equipped (only the tool, not upgrade!)
                var currentlyEquippedTool = _equipSystem.GetEquippedTool(slotType);
                if (currentlyEquippedTool != null)
                    _backpackInventory.AddItem(currentlyEquippedTool, 1);

                // Install tool (leave upgrade as-is)
                _equipSystem.SetEquippedTool(slotType, item);
            }
            else if (item.itemType == ItemType.Upgrade)
            {
                // Remove upgrade from inventory
                if (!_backpackInventory.RemoveItem(item, 1)) return;

                // Swap out any upgrade already equipped (only the upgrade, not tool!)
                var currentlyEquippedUpgrade = _equipSystem.GetEquippedUpgrade(slotType);
                if (currentlyEquippedUpgrade != null)
                    _backpackInventory.AddItem(currentlyEquippedUpgrade, 1);

                // Install upgrade (leave tool as-is)
                _equipSystem.SetEquippedUpgrade(slotType, item);
            }

            _eventBus.RaiseBackpackChanged();
        }

        public void UninstallItem(ItemData item)
        {
            if (!_isInitialized || item == null) return;

            var slotType = _equipSystem.GetEquipSlotType(item);

            if (item.itemType == ItemType.Tool)
            {
                // Remove tool, leave upgrade in place
                if (_equipSystem.GetEquippedTool(slotType) == item)
                {
                    _equipSystem.SetEquippedTool(slotType, null);
                    _backpackInventory.AddItem(item, 1);
                }
            }
            else if (item.itemType == ItemType.Upgrade)
            {
                // Remove upgrade, leave tool in place
                if (_equipSystem.GetEquippedUpgrade(slotType) == item)
                {
                    _equipSystem.SetEquippedUpgrade(slotType, null);
                    _backpackInventory.AddItem(item, 1);
                }
            }

            _eventBus.RaiseBackpackChanged();
        }
    }
}