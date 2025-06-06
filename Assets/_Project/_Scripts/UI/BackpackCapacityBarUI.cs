using HairvestMoon.Core;
using HairvestMoon.Inventory;
using UnityEngine;
using UnityEngine.UI;

namespace HairvestMoon.UI
{
    public class BackpackCapacityBarUI : MonoBehaviour, IBusListener
    {
        [SerializeField] private Image fillImage;

        public void InitializeUI()
        {
            Refresh();
        }

        public void RegisterBusListeners()
        {
            var bus = ServiceLocator.Get<GameEventBus>();
            bus.BackpackChanged += Refresh;
        }

        private void Refresh()
        {
            int current = ServiceLocator.Get<BackpackInventorySystem>().GetAllSlots().Count;
            int total = ServiceLocator.Get<BackpackUpgradeManager>().GetCurrentSlots();

            float fillAmount = (float)current / total;
            fillImage.fillAmount = fillAmount;
        }
    }
}
