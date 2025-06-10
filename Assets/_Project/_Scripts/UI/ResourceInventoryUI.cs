using HairvestMoon.Core;
using HairvestMoon.Farming;
using HairvestMoon.Inventory;
using HairvestMoon;
using System.Collections.Generic;
using UnityEngine;
using HairvestMoon.UI;

public class ResourceInventoryUI : MonoBehaviour, IBusListener
{
    [Header("UI References")]
    [SerializeField] private Transform cropGridParent;
    [SerializeField] private Transform seedGridParent;
    [SerializeField] private GameObject inventorySlotPrefab;

    private readonly Dictionary<ItemData, InventorySlotUI> cropSlots = new();
    private readonly Dictionary<ItemData, InventorySlotUI> seedSlots = new();

    // Cached references
    private SeedDatabase _seedDatabase;
    private ResourceInventorySystem _resourceInventory;

    private InventorySlotUI _selectedSlot = null;

    public void InitializeUI()
    {
        _seedDatabase = ServiceLocator.Get<SeedDatabase>();
        _resourceInventory = ServiceLocator.Get<ResourceInventorySystem>();
        // Do not call BuildUI or RefreshUI here
    }

    public void RegisterBusListeners()
    {
        var bus = ServiceLocator.Get<GameEventBus>();
        bus.InventoryChanged += RefreshUI;
        bus.GlobalSystemsInitialized += OnGlobalSystemsInitialized;
    }

    private void OnGlobalSystemsInitialized()
    {
        BuildUI();
        RefreshUI();
    }

    /// <summary>
    /// Builds UI slots for all seeds and crops.
    /// </summary>
    private void BuildUI()
    {
        foreach (Transform child in seedGridParent) Destroy(child.gameObject);
        foreach (Transform child in cropGridParent) Destroy(child.gameObject);
        seedSlots.Clear();
        cropSlots.Clear();

        foreach (var seedData in _seedDatabase.AllSeeds)
        {
            var item = seedData.seedItem;
            var slotGO = Instantiate(inventorySlotPrefab, seedGridParent);
            var slot = slotGO.GetComponent<InventorySlotUI>();
            slot.Initialize(item, OnSlotSelected);
            seedSlots[item] = slot;
        }

        foreach (var seedData in _seedDatabase.AllSeeds)
        {
            var cropData = seedData?.cropData;
            if (cropData?.harvestedItem == null) continue;

            var item = cropData.GetHarvestItem();
            if (cropSlots.ContainsKey(item)) continue;

            var slotGO = Instantiate(inventorySlotPrefab, cropGridParent);
            var slot = slotGO.GetComponent<InventorySlotUI>();
            slot.Initialize(item);
            cropSlots[item] = slot;
        }
    }

    private void OnSlotSelected(ItemData item)
    {
        // Deselect old slot
        if (_selectedSlot != null)
            _selectedSlot.SetSelected(false);

        // Find the newly selected slot
        if (seedSlots.TryGetValue(item, out var slot) || cropSlots.TryGetValue(item, out slot))
        {
            _selectedSlot = slot;
            slot.SetSelected(true);
            // (Optionally) Show price or enable "Sell" button here.
        }
    }

    /// <summary>
    /// Refreshes all slot displays with current quantities.
    /// </summary>
    public void RefreshUI()
    {
        foreach (var pair in cropSlots)
            pair.Value.UpdateDisplay();
        foreach (var pair in seedSlots)
            pair.Value.UpdateDisplay();
    }
}
