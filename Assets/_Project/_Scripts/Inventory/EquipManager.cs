using HairvestMoon.Core;
using System.Diagnostics.Contracts;
using UnityEngine;

namespace HairvestMoon.Inventory
{
    public class EquipManager : MonoBehaviour, IBusListener
    {
        private bool isInitialized = false;

        public void RegisterBusListeners()
        {
            var bus = ServiceLocator.Get<GameEventBus>();
            bus.GlobalSystemsInitialized += OnGlobalSystemsInitialized;
        }

        private void OnGlobalSystemsInitialized()
        {
            Initialize();
        }

        private void Initialize()
        {
            SyncEquippedItemsIntoBackpackInventory();
            isInitialized = true;
        }

        private void SyncEquippedItemsIntoBackpackInventory()
        {
            var equipSystem = ServiceLocator.Get<BackpackEquipSystem>();
            var backpackInventory = ServiceLocator.Get<BackpackInventorySystem>();

            void TryAdd(ItemData item)
            {
                if (item != null)
                {
                    if (!backpackInventory.GetAllSlots().Exists(slot => slot.item == item))
                    {
                        backpackInventory.AddItem(item, 1);
                    }
                }
            }

            TryAdd(equipSystem.hoeTool);
            TryAdd(equipSystem.wateringTool);
            TryAdd(equipSystem.seedTool);
            TryAdd(equipSystem.harvestTool);
            TryAdd(equipSystem.hoeUpgrade);
            TryAdd(equipSystem.wateringUpgrade);
            TryAdd(equipSystem.seedUpgrade);
            TryAdd(equipSystem.harvestUpgrade);
        }

        public bool TryEquip(ItemData item)
        {
            if (item == null)
                return false;

            var equipSystem = ServiceLocator.Get<BackpackEquipSystem>();

            if (item.itemType == ItemType.Tool)
            {
                equipSystem.EquipTool(item);
                ServiceLocator.Get<GameEventBus>().RaiseItemInstalled(item);
                return true;
            }

            if (item.itemType == ItemType.Upgrade)
            {
                equipSystem.EquipUpgrade(item);
                ServiceLocator.Get<GameEventBus>().RaiseItemInstalled(item);
                return true;
            }

            Debug.LogWarning($"Cannot install item type: {item.itemType}");
            return false;
        }
    }
}
