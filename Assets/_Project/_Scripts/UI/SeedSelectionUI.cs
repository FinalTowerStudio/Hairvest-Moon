using HairvestMoon.Core;
using HairvestMoon.Farming;
using HairvestMoon.Inventory;
using System.Collections.Generic;
using UnityEngine;

namespace HairvestMoon.UI
{
    /// <summary>
    /// UI for selecting a seed to plant. Only shows seeds in inventory, always highlights selected.
    /// </summary>
    public class SeedSelectionUI : MonoBehaviour, IBusListener
    {
        [Header("UI References")]
        [SerializeField] private GameObject seedSlotPrefab;
        [SerializeField] private Transform seedGridParent;
        [SerializeField] private FarmToolHandler farmToolHandler;

        private List<SelectionSlotUI> slots = new();
        private ItemData _currentSelectedItem;
        private CanvasGroup _canvasGroup;
        private SeedDatabase _seedDatabase;
        private ResourceInventorySystem _resourceInventory;

        public void RegisterBusListeners()
        {
            var bus = ServiceLocator.Get<GameEventBus>();
            bus.InventoryChanged += RefreshUI;
            bus.GlobalSystemsInitialized += OnGlobalSystemsInitialized;
        }

        private void OnGlobalSystemsInitialized()
        {
            _seedDatabase = ServiceLocator.Get<SeedDatabase>();
            _resourceInventory = ServiceLocator.Get<ResourceInventorySystem>();
            _canvasGroup = GetComponent<CanvasGroup>();
            BuildUI();
        }

        /// <summary>
        /// Builds selection grid for seeds owned in inventory.
        /// </summary>
        private void BuildUI()
        {
            foreach (Transform child in seedGridParent)
                Destroy(child.gameObject);

            slots.Clear();
            List<ItemData> seedsInInventory = new();

            foreach (var seedData in _seedDatabase.AllSeeds)
            {
                int quantity = _resourceInventory.GetQuantity(seedData.seedItem);
                if (quantity > 0)
                    seedsInInventory.Add(seedData.seedItem);
            }

            if (seedsInInventory.Count == 0)
            {
                _currentSelectedItem = null;
                farmToolHandler.SetSelectedSeed(null);
                return;
            }

            // Build UI for each owned seed
            foreach (var item in seedsInInventory)
            {
                var slotGO = Instantiate(seedSlotPrefab, seedGridParent);
                var slotUI = slotGO.GetComponent<SelectionSlotUI>();
                slotUI.Initialize(item, OnSeedSelected);
                slotUI.SetSelected(item == _currentSelectedItem);
                slots.Add(slotUI);
            }

            // If no selection is active, auto-select first seed
            if (_currentSelectedItem == null)
                OnSeedSelected(seedsInInventory[0]);
        }

        private void OnSeedSelected(ItemData selectedItem)
        {
            _currentSelectedItem = selectedItem;
            SetSelectedSeed(_currentSelectedItem);

            foreach (var slot in slots)
                slot.SetSelected(slot.Item == _currentSelectedItem);
        }

        private void SetSelectedSeed(ItemData selectedItem)
        {
            var seedData = _seedDatabase.GetSeedDataByItem(selectedItem);
            farmToolHandler.SetSelectedSeed(seedData);
        }

        private void RefreshUI()
        {
            BuildUI();
        }

        public void OpenSeedMenu()
        {
            gameObject.SetActive(true);
            BuildUI();
            SetCanvasVisible(true);
        }

        public void CloseSeedMenu()
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
    }
}
