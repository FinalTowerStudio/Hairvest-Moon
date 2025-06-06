using HairvestMoon.Core;
using UnityEngine;

namespace HairvestMoon.Inventory
{
    public class BackpackUpgradeManager : MonoBehaviour, IBusListener
    {
        [Header("Upgrade Settings")]
        [SerializeField] private int baseSlots = 10;
        [SerializeField] private int maxUpgrades = 10;
        [SerializeField] private int slotsPerUpgrade = 2;

        public int SlotsPerUpgrade => slotsPerUpgrade;
        public int BaseSlots => baseSlots;

        private int upgradeLevel = 0;

        public void RegisterBusListeners()
        {
            var bus = ServiceLocator.Get<GameEventBus>();
            bus.GlobalSystemsInitialized += OnGlobalSystemsInitialized;
        }

        private void OnGlobalSystemsInitialized()
        {
            Initialize();
        }

        public void Initialize()
        {
            ApplyUpgrade();
        }

        public void UpgradeBackpack()
        {
            if (upgradeLevel < maxUpgrades)
            {
                upgradeLevel++;
                ApplyUpgrade();
                Debug.Log($"Backpack upgraded to level {upgradeLevel}");
            }
            else
            {
                Debug.Log("Backpack fully upgraded.");
            }
        }

        private void ApplyUpgrade()
        {
            int totalSlots = baseSlots + (upgradeLevel * slotsPerUpgrade);
            ServiceLocator.Get<BackpackInventorySystem>().maxBackpackSlots = totalSlots;
            ServiceLocator.Get<BackpackInventorySystem>().ForceRefresh();
        }


        public int GetUpgradeLevel() => upgradeLevel;
        public int GetMaxUpgrades() => maxUpgrades;
        public int GetCurrentSlots() => ServiceLocator.Get<BackpackInventorySystem>().maxBackpackSlots;
    }
}
