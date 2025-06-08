using HairvestMoon.Core;
using HairvestMoon.Inventory;
using HairvestMoon.Tool;

namespace HairvestMoon.Inventory
{
    /// <summary>
    /// Holds the currently equipped tools and upgrades for the player.
    /// Publishes events on equip/unequip for UI and analytics.
    /// </summary>
    public class BackpackEquipSystem : IBusListener
    {
        // Equipped Tools
        public ItemData hoeTool { get; private set; }
        public ItemData wateringTool { get; private set; }
        public ItemData seedTool { get; private set; }
        public ItemData harvestTool { get; private set; }

        // Equipped Upgrades
        public ItemData hoeUpgrade { get; private set; }
        public ItemData wateringUpgrade { get; private set; }
        public ItemData seedUpgrade { get; private set; }
        public ItemData harvestUpgrade { get; private set; }

        private GameEventBus _eventBus;
        private bool _isInitialized = false;

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
        /// Initializes all equipped slots to null.
        /// </summary>
        public void Initialize()
        {
            hoeTool = wateringTool = seedTool = harvestTool = null;
            hoeUpgrade = wateringUpgrade = seedUpgrade = harvestUpgrade = null;
        }

        /// <summary>
        /// Equip an item in the appropriate slot, fire events.
        /// </summary>
        public void EquipItem(ItemData item)
        {
            if (item == null) return;

            switch (item.itemType)
            {
                case ItemType.Tool:
                    EquipTool(item);
                    break;
                case ItemType.Upgrade:
                    EquipUpgrade(item);
                    break;
            }
        }

        private void EquipTool(ItemData item)
        {
            switch (item.toolType)
            {
                case ToolType.Hoe: hoeTool = item; break;
                case ToolType.WateringCan: wateringTool = item; break;
                case ToolType.Seed: seedTool = item; break;
                case ToolType.Harvest: harvestTool = item; break;
            }
            _eventBus?.RaiseBackpackChanged();
            // Optionally: fire ToolEquipped event here for more granular tracking
        }

        private void EquipUpgrade(ItemData item)
        {
            switch (item.toolType)
            {
                case ToolType.Hoe: hoeUpgrade = item; break;
                case ToolType.WateringCan: wateringUpgrade = item; break;
                case ToolType.Seed: seedUpgrade = item; break;
                case ToolType.Harvest: harvestUpgrade = item; break;
            }
            _eventBus?.RaiseBackpackChanged();
            // Optionally: fire UpgradeEquipped event here for UI or analytics
        }

        /// <summary>
        /// Unequip (clear) an item from the relevant slot.
        /// </summary>
        public void UnequipItem(ItemData item)
        {
            if (item == null) return;

            if (item.itemType == ItemType.Tool)
            {
                if (hoeTool == item) hoeTool = null;
                if (wateringTool == item) wateringTool = null;
                if (seedTool == item) seedTool = null;
                if (harvestTool == item) harvestTool = null;
            }
            else if (item.itemType == ItemType.Upgrade)
            {
                if (hoeUpgrade == item) hoeUpgrade = null;
                if (wateringUpgrade == item) wateringUpgrade = null;
                if (seedUpgrade == item) seedUpgrade = null;
                if (harvestUpgrade == item) harvestUpgrade = null;
            }
            _eventBus?.RaiseBackpackChanged();
        }

        // Optionally: add Save/Load methods for persistence
    }
}
