using HairvestMoon.Core;
using HairvestMoon.Inventory;
using System.Collections.Generic;

namespace HairvestMoon.Inventory
{
    /// <summary>
    /// Slot-based, player inventory for items. 
    /// Each slot may have its own stack size, lock, or flags.
    /// Fires events on changes.
    /// </summary>
    public class BackpackInventorySystem : IBusListener
    {
        private List<BackpackSlotData> _slots = new();
        public IReadOnlyList<BackpackSlotData> Slots => _slots;


        private GameEventBus _eventBus;
        private bool _isInitialized = false;

        // --- Initialization and Bus Wiring ---

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
        /// Sets up (or clears) all backpack slots for a new game.
        /// </summary>
        public void Initialize(int slotCount = 12)
        {
            _slots.Clear();
            for (int i = 0; i < slotCount; i++)
                _slots.Add(new BackpackSlotData());
            _eventBus.RaiseBackpackChanged();
        }

        /// <summary>
        /// Checks if this item (with quantity) can be added to any slot.
        /// </summary>
        public bool CanAddItem(ItemData item, int quantity)
        {
            if (item == null || quantity <= 0) return false;

            // Try to stack first (future: obey stack size)
            foreach (var slot in _slots)
            {
                if (slot.Item == item /* && slot.Stack < item.MaxStackSize */)
                    return true; // can stack
            }

            // Try to find an empty slot
            foreach (var slot in _slots)
            {
                if (slot.Item == null)
                    return true;
            }
            return false; // No space
        }

        /// <summary>
        /// Add an item to the first available slot (or stack if allowed). Returns true if added.
        /// </summary>
        public bool AddItem(ItemData item, int quantity)
        {
            if (item == null || quantity <= 0) return false;

            // Try to stack with existing, if stackable (future: add IsStackable flag)
            for (int i = 0; i < _slots.Count; i++)
            {
                var slot = _slots[i];
                if (slot.Item == item /* && slot.Stack < item.MaxStackSize */) // TODO: for stack size
                {
                    slot.Stack += quantity;
                    _eventBus.RaiseBackpackChanged();
                    return true;
                }
            }

            // Place in first empty slot
            for (int i = 0; i < _slots.Count; i++)
            {
                var slot = _slots[i];
                if (slot.Item == null)
                {
                    slot.Item = item;
                    slot.Stack = quantity;
                    _eventBus.RaiseBackpackChanged();
                    return true;
                }
            }
            return false; // Backpack full
        }

        /// <summary>
        /// Remove an item (from any slot). Returns true if successful.
        /// </summary>
        public bool RemoveItem(ItemData item, int quantity)
        {
            if (item == null || quantity <= 0) return false;

            for (int i = 0; i < _slots.Count; i++)
            {
                var slot = _slots[i];
                if (slot.Item == item && slot.Stack >= quantity)
                {
                    slot.Stack -= quantity;
                    if (slot.Stack <= 0)
                        slot.Clear();
                    _eventBus.RaiseBackpackChanged();
                    return true;
                }
            }
            return false; // Not enough items found
        }

        /// <summary>
        /// Moves an item from one slot to another (swap or merge).
        /// </summary>
        public bool MoveItem(int fromIndex, int toIndex)
        {
            if (!IsValidSlot(fromIndex) || !IsValidSlot(toIndex)) return false;
            if (fromIndex == toIndex) return false;

            var from = _slots[fromIndex];
            var to = _slots[toIndex];

            // Simple swap if destination is empty or different item
            if (to.Item == null || to.Item != from.Item)
            {
                (_slots[fromIndex], _slots[toIndex]) = (_slots[toIndex], _slots[fromIndex]);
                _eventBus.RaiseBackpackChanged();
                return true;
            }
            // Merge stacks if same item (future: obey MaxStackSize)
            // TODO: For now, just sum, but clamp if needed
            to.Stack += from.Stack;
            from.Clear();
            _eventBus.RaiseBackpackChanged();
            return true;
        }

        /// <summary>
        /// Get total quantity of a specific item in backpack.
        /// </summary>
        public int GetQuantity(ItemData item)
        {
            int qty = 0;
            foreach (var slot in _slots)
                if (slot.Item == item)
                    qty += slot.Stack;
            return qty;
        }

        /// <summary>
        /// Find the first slot index containing this item (or -1).
        /// </summary>
        public int FindItemSlot(ItemData item)
        {
            for (int i = 0; i < _slots.Count; i++)
                if (_slots[i].Item == item)
                    return i;
            return -1;
        }

        private bool IsValidSlot(int index) => index >= 0 && index < _slots.Count;

        // --- Slot Data Helper Class ---
        [System.Serializable]
        public class BackpackSlotData
        {
            public ItemData Item;
            public int Stack;

            public void Clear()
            {
                Item = null;
                Stack = 0;
            }
        }
    }
}
