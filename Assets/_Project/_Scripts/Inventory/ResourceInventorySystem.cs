using HairvestMoon.Core;
using HairvestMoon.Inventory;
using System.Collections.Generic;

namespace HairvestMoon.Inventory
{
    /// <summary>
    /// Global resource inventory for crops, seeds, materials, etc.
    /// Tracks discovered items for encyclopedia/unlock purposes.
    /// Fires events on add, remove, and discover.
    /// </summary>
    public class ResourceInventorySystem : IBusListener
    {
        private readonly Dictionary<ItemData, int> _inventory = new();
        private readonly HashSet<ItemData> _discoveredItems = new();

        public IReadOnlyDictionary<ItemData, int> Inventory => _inventory;
        public IReadOnlyCollection<ItemData> DiscoveredItems => _discoveredItems;

        private bool _isInitialized = false;
        private GameEventBus _eventBus;

        public void RegisterBusListeners()
        {
            _eventBus = ServiceLocator.Get<GameEventBus>();
            _eventBus.GlobalSystemsInitialized += OnGlobalSystemsInitialized;
        }

        private void OnGlobalSystemsInitialized()
        {
            Initialize();
            _isInitialized = true;
        }

        /// <summary>
        /// Clears inventory and discoveries (new game/start).
        /// </summary>
        public void Initialize()
        {
            _inventory.Clear();
            _discoveredItems.Clear();
        }

        /// <summary>
        /// Add an item and fire InventoryChanged. Returns true if successful.
        /// </summary>
        public bool AddItem(ItemData item, int quantity)
        {
            if (item == null || quantity <= 0) return false;

            _inventory.TryGetValue(item, out int currentQty);
            _inventory[item] = currentQty + quantity;

            MarkDiscovered(item);
            _eventBus.RaiseInventoryChanged();
            // TODO: Optionally fire "ItemAdded" with item/quantity for UI.
            return true;
        }

        /// <summary>
        /// Remove an item and fire InventoryChanged. Returns true if successful.
        /// </summary>
        public bool RemoveItem(ItemData item, int quantity)
        {
            if (item == null || quantity <= 0) return false;
            if (!_inventory.TryGetValue(item, out int currentQty) || currentQty < quantity)
                return false;

            _inventory[item] -= quantity;
            if (_inventory[item] <= 0)
                _inventory.Remove(item);

            _eventBus.RaiseInventoryChanged();
            // TODO: Optionally fire "ItemRemoved" with item/quantity for UI.
            return true;
        }

        /// <summary>
        /// Get quantity of an item (0 if missing).
        /// </summary>
        public int GetQuantity(ItemData item)
        {
            _inventory.TryGetValue(item, out int quantity);
            return quantity;
        }

        /// <summary>
        /// Mark an item as discovered (for encyclopedia/progression).
        /// Fires event if first discovered.
        /// </summary>
        public void MarkDiscovered(ItemData item)
        {
            if (item == null || _discoveredItems.Contains(item)) return;

            _discoveredItems.Add(item);
            _eventBus.RaiseInventoryChanged();
            // TODO: Fire a special "ItemDiscovered" event if desired.
        }

        /// <summary>
        /// Has this item ever been discovered?
        /// </summary>
        public bool IsDiscovered(ItemData item) => _discoveredItems.Contains(item);
    }
}
