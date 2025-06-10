using HairvestMoon.Core;

namespace HairvestMoon.Inventory
{
    public class BackpackUpgradeManager : IBusListener
    {
        private bool isInitialized = false;
        private const int slotsPerUpgrade = 2;

        public int SlotsPerUpgrade => slotsPerUpgrade;

        public void RegisterBusListeners()
        {
            ServiceLocator.Get<GameEventBus>().GlobalSystemsInitialized += OnGlobalSystemsInitialized;
        }

        private void OnGlobalSystemsInitialized()
        {
            Initialize();
            isInitialized = true;
        }

        public void Initialize()
        {
            // Placeholder
        }

        public void ApplyBackpackUpgrade()
        {
            ServiceLocator.Get<BackpackInventorySystem>().UnlockSlots(slotsPerUpgrade);
        }
    }
}
