using HairvestMoon.Core;
using HairvestMoon.Inventory;

namespace HairvestMoon.Inventory
{
    /// <summary>
    /// Handles install/equip requests from UI or other systems.
    /// Validates item, then routes request to EquipManager.
    /// </summary>
    public class BackpackEquipInstallManager : IBusListener
    {
        private EquipManager _equipManager;
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
            _equipManager = ServiceLocator.Get<EquipManager>();
            _backpackInventory = ServiceLocator.Get<BackpackInventorySystem>();
            _isInitialized = true;
        }

        /// <summary>
        /// Handles a UI/player request to install/equip an item.
        /// Returns true if successful, false otherwise.
        /// </summary>
        public bool TryInstallItem(ItemData item)
        {
            if (!_isInitialized || item == null) return false;

            // Must have at least 1 in backpack to equip
            int quantity = _backpackInventory.GetQuantity(item);
            if (quantity <= 0)
                return false;

            // Optionally: Validate item type (must be Tool or Upgrade)
            if (item.itemType != ItemType.Tool && item.itemType != ItemType.Upgrade)
                return false;

            // Forward install to EquipManager
            _equipManager.InstallItem(item);

            // Optionally: analytics, UI, sound feedback
            // e.g., _eventBus.RaiseItemInstalled(item);

            return true;
        }

        /// <summary>
        /// (Optional) Handles UI/player request to uninstall/unequip an item.
        /// </summary>
        public bool TryUninstallItem(ItemData item)
        {
            if (!_isInitialized || item == null) return false;

            // Only proceed if item is currently equipped (future: check by slot)
            // (For now, always allow)
            _equipManager.UninstallItem(item);

            // Optionally: feedback/analytics
            // e.g., _eventBus.RaiseBackpackChanged();

            return true;
        }
    }
}
