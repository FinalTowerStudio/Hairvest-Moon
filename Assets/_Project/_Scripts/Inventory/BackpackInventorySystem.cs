using HairvestMoon.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HairvestMoon.Inventory
{
    public class BackpackInventorySystem : MonoBehaviour, IBusListener
    {
        [System.Serializable]
        public class BackpackSlot
        {
            public ItemData item;
            public int quantity;
        }

        [Header("Backpack Settings")]
        public List<BackpackSlot> backpack = new();
        
        [NonSerialized] public int maxBackpackSlots;

        public void RegisterBusListeners()
        {
            var bus = ServiceLocator.Get<GameEventBus>();
            bus.GlobalSystemsInitialized += OnGlobalSystemsInitialized;
        }

        private void OnGlobalSystemsInitialized()
        {
            // No initialization currently required.
        }

        public bool CanAddItem(ItemData newItem, int amount = 1)
        {
            // Check for stackable items already in backpack
            foreach (var slot in backpack)
            {
                if (slot.item == newItem)
                {
                    if (IsStackable(newItem))
                    {
                        return true; // stacking doesn't require slot space
                    }
                    break;
                }
            }

            // Check slot limit for non-stackables
            return backpack.Count < maxBackpackSlots;
        }


        public bool AddItem(ItemData newItem, int amount = 1)
        {
            foreach (var slot in backpack)
            {
                if (slot.item == newItem)
                {
                    if (IsStackable(newItem))
                    {
                        slot.quantity += amount;
                        NotifyBackpackChanged();
                        return true;
                    }
                    break;
                }
            }

            if (backpack.Count >= maxBackpackSlots)
            {
                Debug.Log("Backpack full.");
                return false;
            }

            backpack.Add(new BackpackSlot { item = newItem, quantity = amount });
            NotifyBackpackChanged();
            return true;
        }

        private bool IsStackable(ItemData item)
        {
            return item.itemType == ItemType.QuestItem
                || item.itemType == ItemType.Currency
                || item.itemType == ItemType.Fertilizer;
        }

        public bool RemoveItem(ItemData item, int amount = 1)
        {
            for (int i = 0; i < backpack.Count; i++)
            {
                if (backpack[i].item == item)
                {
                    if (backpack[i].quantity < amount)
                        return false;

                    backpack[i].quantity -= amount;
                    if (backpack[i].quantity <= 0)
                        backpack.RemoveAt(i);

                    NotifyBackpackChanged();
                    return true;
                }
            }
            return false;
        }

        public int GetQuantity(ItemData queryItem)
        {
            foreach (var slot in backpack)
            {
                if (slot.item == queryItem)
                    return slot.quantity;
            }
            return 0;
        }

        public void ForceRefresh()
        {
            NotifyBackpackChanged();
        }

        private void NotifyBackpackChanged()
        {
            ServiceLocator.Get<GameEventBus>().RaiseBackpackChanged();
        }

        public List<BackpackSlot> GetAllSlots() => backpack;
    }
}
