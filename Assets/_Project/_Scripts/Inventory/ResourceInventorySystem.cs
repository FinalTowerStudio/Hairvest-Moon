using HairvestMoon.Inventory;
using HairvestMoon;
using System.Collections.Generic;
using UnityEngine;
using System;
using HairvestMoon.Farming;
using HairvestMoon.Core;

namespace HairvestMoon.Inventory
{
    /// <summary>
    /// Manages the player's inventory, including adding items, tracking discovered items, and querying item quantities.
    /// </summary>
    public class ResourceInventorySystem : MonoBehaviour, IBusListener
    {
        [System.Serializable]
        public class InventorySlot
        {
            public ItemData item;
            public int quantity;
        }

        [Header("Inventory Settings")]
        public List<InventorySlot> inventory = new();
        public HashSet<ItemData> discoveredItems = new HashSet<ItemData>();

        public void RegisterBusListeners()
        {
            var bus = ServiceLocator.Get<GameEventBus>();
            bus.GlobalSystemsInitialized += OnGlobalSystemsInitialized;
        }

        private void OnGlobalSystemsInitialized()
        {
            Initialize();
        }

        public void Initialize() { }

        public void MarkDiscovered(ItemData item)
        {
            if (!discoveredItems.Contains(item))
            {
                discoveredItems.Add(item);
                Debug.Log($"Discovered new item: {item.itemID}");
            }
        }

        public bool AddItem(ItemData newItem, int amount = 1)
        {
            // Unlimited stacking for Seeds and Crops
            foreach (var slot in inventory)
            {
                if (slot.item == newItem)
                {
                    slot.quantity += amount;
                    NotifyInventoryChanged();
                    return true;
                }
            }

            var newSlot = new InventorySlot { item = newItem, quantity = amount };
            inventory.Add(newSlot);
            MarkDiscovered(newItem);
            NotifyInventoryChanged();
            return true;
        }

        public int GetQuantity(ItemData queryItem)
        {
            foreach (var slot in inventory)
            {
                if (slot.item == queryItem)
                    return slot.quantity;
            }
            return 0;
        }

        public bool RemoveItem(ItemData item, int amount)
        {
            foreach (var slot in inventory)
            {
                if (slot.item == item)
                {
                    if (slot.quantity < amount)
                        return false;

                    slot.quantity -= amount;
                    if (slot.quantity <= 0)
                        inventory.Remove(slot);

                    NotifyInventoryChanged();
                    return true;
                }
            }
            return false;
        }

        public List<ItemData> GetOwnedItemsByType(ItemType type)
        {
            List<ItemData> result = new();

            foreach (var slot in inventory)
            {
                if (slot.item.itemType == type)
                {
                    result.Add(slot.item);
                }
            }
            return result;
        }

        public List<ItemData> GetDiscoveredItemsByType(ItemType type)
        {
            List<ItemData> result = new();

            foreach (var item in discoveredItems)
            {
                if (item.itemType == type)
                {
                    result.Add(item);
                }
            }
            return result;
        }

        public List<InventorySlot> GetAllSlots()
        {
            return inventory;
        }

        public void ForceRefresh()
        {
            NotifyInventoryChanged();
        }

        private void NotifyInventoryChanged()
        {
            ServiceLocator.Get<GameEventBus>().RaiseInventoryChanged();
        }
    }
}