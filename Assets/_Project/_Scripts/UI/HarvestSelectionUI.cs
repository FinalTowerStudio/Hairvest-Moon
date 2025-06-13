using HairvestMoon.Core;
using HairvestMoon.Inventory;
using HairvestMoon.Tool;
using System.Collections.Generic;
using UnityEngine;

namespace HairvestMoon.UI
{
    /// <summary>
    /// UI for selecting equipped harvest tool or upgrade, always includes "hands" slot.
    /// </summary>
    public class HarvestSelectionUI : MonoBehaviour, IBusListener
    {
        [Header("UI References")]
        [SerializeField] private GameObject optionPrefab;
        [SerializeField] private Transform optionParent;

        private List<SelectionSlotUI> _slots = new();
        private ItemData _currentSelectedItem;
        private CanvasGroup _canvasGroup;
        private BackpackInventorySystem _backpackInventory;
        private BackpackEquipSystem _equipSystem;

        public void RegisterBusListeners()
        {
            var bus = ServiceLocator.Get<GameEventBus>();
            bus.GlobalSystemsInitialized += OnGlobalSystemsInitialized;
            bus.BackpackChanged += RefreshUI;
        }

        private void OnGlobalSystemsInitialized()
        {
            _backpackInventory = ServiceLocator.Get<BackpackInventorySystem>();
            _equipSystem = ServiceLocator.Get<BackpackEquipSystem>();
            _canvasGroup = GetComponent<CanvasGroup>();
            BuildUI();
        }

        private void BuildUI()
        {
            foreach (Transform child in optionParent)
                Destroy(child.gameObject);

            _slots.Clear();

            var equippedHarvest = _equipSystem.harvestTool;

            // Always show hands slot
            var handsGO = Instantiate(optionPrefab, optionParent);
            var handsSlot = handsGO.GetComponent<SelectionSlotUI>();
            handsSlot.SetHandsTooltip(
                "Hands (Nothing Equipped)",
                "No harvest tool equipped. You'll harvest by hand (less efficient)!"
            );
            handsSlot.Initialize(null, OnOptionSelected);
            handsSlot.SetSelected(equippedHarvest == null);
            _slots.Add(handsSlot);

            // Show equipped harvest tool if present
            if (equippedHarvest != null)
            {
                var slotUI = Instantiate(optionPrefab, optionParent).GetComponent<SelectionSlotUI>();
                slotUI.Initialize(equippedHarvest, OnOptionSelected);
                slotUI.SetSelected(true);
                _slots.Add(slotUI);
            }
        }

        private void OnOptionSelected(ItemData selectedItem)
        {
            _currentSelectedItem = selectedItem;
            foreach (var slot in _slots)
                slot.SetSelected(slot.Item == _currentSelectedItem);

            _equipSystem.SetEquippedItem(ToolType.Harvest, selectedItem);
        }


        public ItemData GetCurrentSelectedItem() => _currentSelectedItem;

        private void RefreshUI() => BuildUI();

        public void OpenMenu()
        {
            gameObject.SetActive(true);
            BuildUI();
            SetCanvasVisible(true);
        }

        public void CloseMenu() => SetCanvasVisible(false);

        private void SetCanvasVisible(bool visible)
        {
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.alpha = visible ? 1f : 0f;
            _canvasGroup.interactable = visible;
            _canvasGroup.blocksRaycasts = visible;
        }
    }
}
