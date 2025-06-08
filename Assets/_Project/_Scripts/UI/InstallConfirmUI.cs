using UnityEngine;
using HairvestMoon.Core;
using HairvestMoon.Inventory;

namespace HairvestMoon.UI
{
    /// <summary>
    /// Shows a confirmation dialog for installing/equipping an item.
    /// Calls BackpackEquipInstallManager if confirmed.
    /// </summary>
    public class InstallConfirmUI : MonoBehaviour, IBusListener
    {
        [Header("UI References")]
        [SerializeField] private GameObject dialogRoot;
        [SerializeField] private UnityEngine.UI.Button confirmButton;
        [SerializeField] private UnityEngine.UI.Button cancelButton;
        [SerializeField] private ItemDescriptionUI itemDescriptionUI;

        private BackpackEquipInstallManager _equipInstallManager;
        private ItemData _pendingItem;

        public void RegisterBusListeners()
        {
            ServiceLocator.Get<GameEventBus>().GlobalSystemsInitialized += OnGlobalSystemsInitialized;
        }

        private void OnGlobalSystemsInitialized()
        {
            _equipInstallManager = ServiceLocator.Get<BackpackEquipInstallManager>();

            // Button wiring
            if (confirmButton != null)
                confirmButton.onClick.AddListener(OnConfirm);
            if (cancelButton != null)
                cancelButton.onClick.AddListener(Hide);
        }

        /// <summary>
        /// Show confirmation dialog for given item.
        /// </summary>
        public void Show(ItemData item)
        {
            _pendingItem = item;
            dialogRoot.SetActive(true);
            if (itemDescriptionUI != null)
                itemDescriptionUI.SetItem(item);
        }

        /// <summary>
        /// Hides dialog and clears pending item.
        /// </summary>
        public void Hide()
        {
            dialogRoot.SetActive(false);
            _pendingItem = null;
            if (itemDescriptionUI != null)
                itemDescriptionUI.Clear();
        }

        private void OnConfirm()
        {
            if (_pendingItem == null) return;
            bool installed = _equipInstallManager.TryInstallItem(_pendingItem);
            // Optionally: play feedback SFX, show success/error
            Hide();
        }
    }
}
