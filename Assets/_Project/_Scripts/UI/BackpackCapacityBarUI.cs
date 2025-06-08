using HairvestMoon.Core;
using HairvestMoon.Inventory;
using UnityEngine;
using UnityEngine.UI;

namespace HairvestMoon.UI
{
    /// <summary>
    /// Shows fill bar for backpack capacity, updates on inventory change.
    /// </summary>
    public class BackpackCapacityBarUI : MonoBehaviour, IBusListener
    {
        [SerializeField] private Image fillImage;

        private BackpackInventorySystem _backpackInventory;
        private BackpackUpgradeManager _upgradeManager;

        public void InitializeUI()
        {
            _backpackInventory = ServiceLocator.Get<BackpackInventorySystem>();
            _upgradeManager = ServiceLocator.Get<BackpackUpgradeManager>();
            Refresh();
        }

        public void RegisterBusListeners()
        {
            var bus = ServiceLocator.Get<GameEventBus>();
            bus.BackpackChanged += Refresh;
        }

        private void Refresh()
        {
            int current = _backpackInventory?.Slots.Count ?? 0;
            int total = _upgradeManager?.GetCurrentSlots() ?? 1;
            float fillAmount = (total > 0) ? (float)current / total : 0f;
            fillImage.fillAmount = fillAmount;
        }
    }
}
