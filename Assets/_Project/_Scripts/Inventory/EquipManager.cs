using HairvestMoon.Core;
using HairvestMoon.Inventory;

namespace HairvestMoon.Inventory
{
    /// <summary>
    /// Manages equip/install requests for tools and upgrades.
    /// Ensures valid state and notifies inventory/equip systems and UI.
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
            Initialize();
            _isInitialized = true;
        }

        /// <summary>
        /// Empty for now, but may need to sync state on game start.
        /// </summary>
        public void Initialize() { }

        /// <summary>
        /// Handles install/equip requests from the UI or gameplay.
        /// Removes from inventory, installs in equip system, and fires UI events.
        /// </summary>
        public void InstallItem(ItemData item)
        {
            if (item == null) return;

            // Remove from backpack inventory if present
            bool removed = _backpackInventory.RemoveItem(item, 1);
            if (!removed) return;

            _equipSystem.EquipItem(item);
            _eventBus?.RaiseItemInstalled(item);

            // TODO: Fire analytics/event bus for "ItemEquipped"
            // TODO: Play sound or animation for equip
        }

        /// <summary>
        /// Handles unequip requests (optional).
        /// </summary>
        public void UninstallItem(ItemData item)
        {
            if (item == null) return;

            _equipSystem.UnequipItem(item);
            _backpackInventory.AddItem(item, 1);
            _eventBus?.RaiseBackpackChanged();

            // TODO: Fire analytics/event bus for "ItemUnequipped"
        }
    }
}
