using HairvestMoon.Core;
using System.Collections.Generic;
using UnityEngine;

namespace HairvestMoon.Inventory
{
    /// <summary>
    /// Slot-based, player inventory for items. 
    /// Each slot may have its own stack size, lock, or flags.
    /// Fires events on changes.
    /// </summary>
    public class BackpackInventorySystem : IBusListener
    {
        public const int MaxSlots = 30;
        private int unlockedSlots = 10; // Or [SerializeField] if you want to tune in inspector
        public int UnlockedSlots => unlockedSlots;

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
        public void Initialize()
        {
            _slots.Clear();
            for (int i = 0; i < MaxSlots; i++)
                _slots.Add(new BackpackSlotData());
            unlockedSlots = 10; // reset at new game
            _eventBus.RaiseBackpackChanged();
        }

        /// <summary>
        /// Unlocks the next N slots, up to MaxSlots.
        /// </summary>
        public void UnlockSlots(int count)
        {
            int prev = unlockedSlots;
            unlockedSlots = Mathf.Min(unlockedSlots + count, MaxSlots);
            if (unlockedSlots != prev)
                _eventBus.RaiseBackpackChanged();
        }

        /// <summary>
        /// Checks if this item (with quantity) can be added to any unlocked slot.
        /// </summary>
        public bool CanAddItem(ItemData item, int quantity)
        {
            if (item == null || quantity <= 0) return false;

            // Try to stack in unlocked slots
            for (int i = 0; i < unlockedSlots; i++)
            {
                var slot = _slots[i];
                if (slot.Item == item /* && slot.Stack < item.MaxStackSize */)
                    return true; // can stack
            }
            // Try to find an empty unlocked slot
            for (int i = 0; i < unlockedSlots; i++)
            {
                if (_slots[i].Item == null)
                    return true;
            }
            return false; // No space
        }

        /// <summary>
        /// Attempts to add an item to the first available unlocked slot, stacking if possible.
        /// Returns true if added, false if no space.
        /// </summary>
        public bool AddItem(ItemData item, int quantity = 1)
        {
            // Try to stack in unlocked slots first
            for (int i = 0; i < unlockedSlots; i++)
            {
                var slot = _slots[i];
                if (slot.Item == item && slot.Stack < item.maxStack)
                {
                    int addable = Mathf.Min(quantity, item.maxStack - slot.Stack);
                    slot.Stack += addable;
                    quantity -= addable;
                    if (quantity <= 0)
                    {
                        _eventBus.RaiseBackpackChanged();
                        return true;
                    }
                }
            }
            // Try empty unlocked slot
            for (int i = 0; i < unlockedSlots; i++)
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
            // No space
            return false;
        }

        /// <summary>
        /// Attempts to remove the specified quantity of an item from unlocked slots.
        /// Returns true if fully removed, false if not enough found.
        /// </summary>
        public bool RemoveItem(ItemData item, int quantity = 1)
        {
            int removed = 0;
            // Remove from stacked slots first (unlocked only)
            for (int i = 0; i < unlockedSlots; i++)
            {
                var slot = _slots[i];
                if (slot.Item == item && slot.Stack > 0)
                {
                    int toRemove = Mathf.Min(slot.Stack, quantity - removed);
                    slot.Stack -= toRemove;
                    removed += toRemove;
                    if (slot.Stack == 0)
                        slot.Item = null;
                    if (removed >= quantity)
                    {
                        _eventBus.RaiseBackpackChanged();
                        return true;
                    }
                }
            }
            // Not enough found
            if (removed > 0)
                _eventBus.RaiseBackpackChanged();
            return false;
        }

        /// <summary>
        /// Moves an item between unlocked slots if possible.
        /// </summary>
        public bool MoveItem(int fromIndex, int toIndex)
        {
            if (fromIndex == toIndex) return false;
            if (fromIndex < 0 || fromIndex >= unlockedSlots) return false;
            if (toIndex < 0 || toIndex >= unlockedSlots) return false;

            var fromSlot = _slots[fromIndex];
            var toSlot = _slots[toIndex];

            if (fromSlot.Item == null) return false;
            if (toSlot.Item == null)
            {
                // Simple move
                toSlot.Item = fromSlot.Item;
                toSlot.Stack = fromSlot.Stack;
                fromSlot.Item = null;
                fromSlot.Stack = 0;
                _eventBus.RaiseBackpackChanged();
                return true;
            }
            // Stack if possible
            if (fromSlot.Item == toSlot.Item && toSlot.Stack < toSlot.Item.maxStack)
            {
                int moveable = Mathf.Min(fromSlot.Stack, toSlot.Item.maxStack - toSlot.Stack);
                toSlot.Stack += moveable;
                fromSlot.Stack -= moveable;
                if (fromSlot.Stack == 0)
                    fromSlot.Item = null;
                _eventBus.RaiseBackpackChanged();
                return true;
            }
            // Can't stack/move
            return false;
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
