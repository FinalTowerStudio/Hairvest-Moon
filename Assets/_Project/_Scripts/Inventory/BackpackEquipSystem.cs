using HairvestMoon.Core;
using HairvestMoon.Tool;
using UnityEngine;

namespace HairvestMoon.Inventory
{
    /// <summary>
    /// Holds which tools/upgrades are currently equipped in each slot.
    /// One tool and one upgrade per slot (Hoe, Watering, Seed, Harvest, etc.).
    /// </summary>
    public class BackpackEquipSystem : IBusListener
    {
        // Tool slots
        public ItemData hoeTool { get; private set; }
        public ItemData wateringTool { get; private set; }
        public ItemData seedTool { get; private set; }
        public ItemData harvestTool { get; private set; }

        // Upgrade slots (one upgrade per type)
        public ItemData hoeUpgrade;
        public ItemData wateringUpgrade;
        public ItemData seedUpgrade;
        public ItemData harvestUpgrade;

        private GameEventBus _eventBus;
        private bool _isInitialized = false;

        public void RegisterBusListeners()
        {
            _eventBus = ServiceLocator.Get<GameEventBus>();
            _eventBus.GlobalSystemsInitialized += OnGlobalSystemsInitialized;
        }

        private void OnGlobalSystemsInitialized()
        {
            ClearAll();
            _isInitialized = true;
        }

        public ToolType GetEquipSlotType(ItemData item)
        {
            // Only care about valid tools/upgrades
            return item.toolType;
        }

        public ItemData GetEquippedItem(ToolType slotType)
        {
            switch (slotType)
            {
                case ToolType.Hoe: return hoeTool ?? hoeUpgrade;
                case ToolType.WateringCan: return wateringTool ?? wateringUpgrade;
                case ToolType.Seed: return seedTool ?? seedUpgrade;
                case ToolType.Harvest: return harvestTool ?? harvestUpgrade;
                default: return null;
            }
        }

        public void SetEquippedItem(ToolType slotType, ItemData item)
        {
            // Use itemType to decide if it's a tool or upgrade
            switch (slotType)
            {
                case ToolType.Hoe:
                    if (item == null) { hoeTool = null; hoeUpgrade = null; }
                    else if (item.itemType == ItemType.Tool) hoeTool = item;
                    else if (item.itemType == ItemType.Upgrade) hoeUpgrade = item;
                    break;
                case ToolType.WateringCan:
                    if (item == null) { wateringTool = null; wateringUpgrade = null; }
                    else if (item.itemType == ItemType.Tool) wateringTool = item;
                    else if (item.itemType == ItemType.Upgrade) wateringUpgrade = item;
                    break;
                case ToolType.Seed:
                    if (item == null) { seedTool = null; seedUpgrade = null; }
                    else if (item.itemType == ItemType.Tool) seedTool = item;
                    else if (item.itemType == ItemType.Upgrade) seedUpgrade = item;
                    break;
                case ToolType.Harvest:
                    if (item == null) { harvestTool = null; harvestUpgrade = null; }
                    else if (item.itemType == ItemType.Tool) harvestTool = item;
                    else if (item.itemType == ItemType.Upgrade) harvestUpgrade = item;
                    break;
                default: break;
            }

            // Raise event to refresh all relevant UIs
            _eventBus?.RaiseBackpackChanged();
        }

        public void ClearAll()
        {
            hoeTool = wateringTool = seedTool = harvestTool = null;
            hoeUpgrade = wateringUpgrade = seedUpgrade = harvestUpgrade = null;
        }

        public ItemData GetEquippedTool(ToolType slotType)
        {
            switch (slotType)
            {
                case ToolType.Hoe: return hoeTool;
                case ToolType.WateringCan: return wateringTool;
                case ToolType.Seed: return seedTool;
                case ToolType.Harvest: return harvestTool;
                default: return null;
            }
        }

        public ItemData GetEquippedUpgrade(ToolType slotType)
        {
            switch (slotType)
            {
                case ToolType.Hoe: return hoeUpgrade;
                case ToolType.WateringCan: return wateringUpgrade;
                case ToolType.Seed: return seedUpgrade;
                case ToolType.Harvest: return harvestUpgrade;
                default: return null;
            }
        }

        public void SetEquippedTool(ToolType slotType, ItemData item)
        {
            switch (slotType)
            {
                case ToolType.Hoe: hoeTool = item; break;
                case ToolType.WateringCan: wateringTool = item; break;
                case ToolType.Seed: seedTool = item; break;
                case ToolType.Harvest: harvestTool = item; break;
            }
            _eventBus?.RaiseBackpackChanged();
        }

        public void SetEquippedUpgrade(ToolType slotType, ItemData item)
        {
            switch (slotType)
            {
                case ToolType.Hoe: hoeUpgrade = item; break;
                case ToolType.WateringCan: wateringUpgrade = item; break;
                case ToolType.Seed: seedUpgrade = item; break;
                case ToolType.Harvest: harvestUpgrade = item; break;
            }
            _eventBus?.RaiseBackpackChanged();
        }

    }
}
