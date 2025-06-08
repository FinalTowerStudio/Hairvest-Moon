using System.Collections.Generic;
using UnityEngine;
using HairvestMoon.Inventory;
using HairvestMoon.Core;

namespace HairvestMoon.UI
{
    /// <summary>
    /// UI for selecting harvest mode/upgrade. Shows normal and upgraded options, highlights selected.
    /// </summary>
    public class HarvestSelectionUI : MonoBehaviour, IBusListener
    {
        [Header("UI References")]
        [SerializeField] private GameObject harvestSelectionSlotPrefab;
        [SerializeField] private Transform gridParent;

        private List<SelectionSlotUI> slots = new();
        private ItemData _currentSelectedHarvestOption;
        private CanvasGroup _canvasGroup;
        private BackpackEquipSystem _equipSystem;
        private BackpackInventorySystem _backpackInventory;

        public void RegisterBusListeners()
        {
            var bus = ServiceLocator.Get<GameEventBus>();
            bus.BackpackChanged += RefreshUI;
            bus.GlobalSystemsInitialized += OnGlobalSystemsInitialized;
        }

        private void OnGlobalSystemsInitialized()
        {
            _equipSystem = ServiceLocator.Get<BackpackEquipSystem>();
            _backpackInventory = ServiceLocator.Get<BackpackInventorySystem>();
            _canvasGroup = GetComponent<CanvasGroup>();
            BuildUI();
        }

        /// <summary>
        /// Builds the harvest option slots (normal + upgrade if available).
        /// </summary>
        private void BuildUI()
        {
            foreach (Transform child in gridParent)
                Destroy(child.gameObject);
            slots.Clear();

            // Always add Normal Harvest option (null means default)
            var normalSlotGO = Instantiate(harvestSelectionSlotPrefab, gridParent);
            var normalSlotUI = normalSlotGO.GetComponent<SelectionSlotUI>();
            normalSlotUI.Initialize(null, OnHarvestOptionSelected);
            normalSlotUI.SetSelected(_currentSelectedHarvestOption == null);
            slots.Add(normalSlotUI);

            // If Harvest Upgrade equipped, add its option
            var harvestUpgrade = _equipSystem?.harvestUpgrade;
            if (harvestUpgrade != null)
            {
                var upgradeGO = Instantiate(harvestSelectionSlotPrefab, gridParent);
                var upgradeSlotUI = upgradeGO.GetComponent<SelectionSlotUI>();
                upgradeSlotUI.Initialize(harvestUpgrade, OnHarvestOptionSelected);
                upgradeSlotUI.SetSelected(harvestUpgrade == _currentSelectedHarvestOption);
                slots.Add(upgradeSlotUI);
            }
        }

        private void RefreshUI()
        {
            BuildUI();
        }

        private void OnHarvestOptionSelected(ItemData selectedItem)
        {
            _currentSelectedHarvestOption = selectedItem;

            foreach (var slot in slots)
                slot.SetSelected(slot.Item == _currentSelectedHarvestOption);
            // TODO: Play SFX or give feedback if needed
        }

        public void OpenHarvestMenu()
        {
            gameObject.SetActive(true);
            BuildUI();
            SetCanvasVisible(true);
        }

        public void CloseHarvestMenu()
        {
            SetCanvasVisible(false);
        }

        private void SetCanvasVisible(bool visible)
        {
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.alpha = visible ? 1f : 0f;
            _canvasGroup.interactable = visible;
            _canvasGroup.blocksRaycasts = visible;
        }

        public ItemData GetCurrentSelectedItem()
        {
            return _currentSelectedHarvestOption;
        }
    }
}
