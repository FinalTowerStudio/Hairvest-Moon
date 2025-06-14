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
        /// If none are available, shows a "hands"/empty slot with context tooltip.
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

            // Determine what to select
            ItemData selectionToRestore = null;
            // If current selection is still present, keep it
            if (_currentSelectedItem != null && seedsInInventory.Contains(_currentSelectedItem))
                selectionToRestore = _currentSelectedItem;
            // Else, auto-select first available (if any)
            else if (seedsInInventory.Count > 0)
                selectionToRestore = seedsInInventory[0];
            // Else, fallback to null/hands

            // No seeds left: show hands/empty slot only
            if (seedsInInventory.Count == 0)
            {
                var handsGO = Instantiate(seedSlotPrefab, seedGridParent);
                var handsSlotUI = handsGO.GetComponent<SelectionSlotUI>();
                handsSlotUI.SetHandsTooltip(
                    "No Seeds Available",
                    "You don’t have any seeds to plant. Buy or find seeds to get started!"
                );
                handsSlotUI.Initialize(null, OnSeedSelected);
                handsSlotUI.SetSelected(true);
                slots.Add(handsSlotUI);

                _currentSelectedItem = null;
                // Optionally fire selection changed bus event here!
                return;
            }

            // Build UI for each owned seed
            foreach (var item in seedsInInventory)
            {
                var slotGO = Instantiate(seedSlotPrefab, seedGridParent);
                var slotUI = slotGO.GetComponent<SelectionSlotUI>();
                slotUI.Initialize(item, OnSeedSelected);
                slotUI.SetSelected(item == selectionToRestore);
                slots.Add(slotUI);
            }

            // Finalize selection
            _currentSelectedItem = selectionToRestore;
            SetSelectedSeed(_currentSelectedItem);

            // Optionally fire selection changed event here as well!
        }


        private void OnSeedSelected(ItemData selectedItem)
        {
            _currentSelectedItem = selectedItem;
            SetSelectedSeed(_currentSelectedItem);

            foreach (var slot in slots)
                slot.SetSelected(slot.Item == _currentSelectedItem);

            // Raise selection changed event -- Not Currently Used
            // ServiceLocator.Get<GameEventBus>().RaiseSeedSelectionChanged(_currentSelectedItem);
        }


        private void SetSelectedSeed(ItemData selectedItem)
        {
            var seedData = _seedDatabase.GetSeedDataByItem(selectedItem);
            //farmToolHandler.SetSelectedSeed(seedData);
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

        public SeedData GetSelectedSeed()
        {
            if (_currentSelectedItem == null || _seedDatabase == null)
                return null;
            return _seedDatabase.GetSeedDataByItem(_currentSelectedItem);
        }

    }
}
