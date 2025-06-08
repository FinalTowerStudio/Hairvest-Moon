using HairvestMoon.Core;

namespace HairvestMoon.Inventory
{
    public class BackpackUpgradeManager : IBusListener
    {
        private bool isInitialized = false;

        private const int baseSlots = 10;
        private int extraSlots = 20;
        private const int slotsPerUpgrade = 2;

        public int SlotsPerUpgrade => slotsPerUpgrade;
        public int BaseSlots => baseSlots;
        public int GetCurrentSlots() => baseSlots + (extraSlots * slotsPerUpgrade);

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
            extraSlots = 20;
        }

        public void ApplyBackpackUpgrade()
        {
            extraSlots++;
            ServiceLocator.Get<GameEventBus>().RaiseBackpackChanged();
        }
    }
}
