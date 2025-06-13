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
        [SerializeField] private UnityEngine.UI.Button installButton;
        [SerializeField] private UnityEngine.UI.Button uninstallButton;
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

            if (installButton != null)
                installButton.onClick.AddListener(OnInstall);
            if (uninstallButton != null)
                uninstallButton.onClick.AddListener(OnUninstall);
        }

        // Called from BackpackInventoryUI when a slot is selected
        public void ShowForInstall(ItemData item)
        {
            _pendingItem = item;
            installButton.interactable = item != null && (item.itemType == ItemType.Tool || item.itemType == ItemType.Upgrade);
            uninstallButton.interactable = false;
            itemDescriptionUI.SetItem(item);
        }

        // Called from EquipSlotUI or UI when equip slot is selected
        public void ShowForUninstall(ItemData item)
        {
            _pendingItem = item;
            installButton.interactable = false;
            uninstallButton.interactable = item != null; // Only if something is equipped
            itemDescriptionUI.SetItem(item);
        }

        private void OnInstall()
        {
            if (_pendingItem == null) return;
            bool installed = _equipInstallManager.TryInstallItem(_pendingItem);
            // Optionally: Show result/feedback
        }

        private void OnUninstall()
        {
            if (_pendingItem == null) return;
            bool uninstalled = _equipInstallManager.TryUninstallItem(_pendingItem);
            // Optionally: Show result/feedback
        }
    }
}
