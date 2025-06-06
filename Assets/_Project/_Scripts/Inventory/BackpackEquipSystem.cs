using UnityEngine;
using HairvestMoon.Tool;
using HairvestMoon.Core;

namespace HairvestMoon.Inventory
{
    public class BackpackEquipSystem : MonoBehaviour, IBusListener
    {
        [Header("Tool Equip Slots")]
        public ItemData hoeTool;
        public ItemData wateringTool;
        public ItemData seedTool;
        public ItemData harvestTool;

        [Header("Upgrade Equip Slots")]
        public ItemData hoeUpgrade;
        public ItemData wateringUpgrade;
        public ItemData seedUpgrade;
        public ItemData harvestUpgrade;

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

        public void EquipTool(ItemData toolItem)
        {
            switch (toolItem.toolType)
            {
                case ToolType.Hoe:
                    hoeTool = toolItem;
                    break;
                case ToolType.WateringCan:
                    wateringTool = toolItem;
                    break;
                case ToolType.Seed:
                    seedTool = toolItem;
                    break;
                case ToolType.Harvest:
                    harvestTool = toolItem;
                    break;
            }
        }

        public void EquipUpgrade(ItemData upgradeItem)
        {
            switch (upgradeItem.toolType)
            {
                case ToolType.Hoe:
                    hoeUpgrade = upgradeItem;
                    break;
                case ToolType.WateringCan:
                    wateringUpgrade = upgradeItem;
                    break;
                case ToolType.Seed:
                    seedUpgrade = upgradeItem;
                    break;
                case ToolType.Harvest:
                    harvestUpgrade = upgradeItem;
                    break;
            }
        }
    }
}
