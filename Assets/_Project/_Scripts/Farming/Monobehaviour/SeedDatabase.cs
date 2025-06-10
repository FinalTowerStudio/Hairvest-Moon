using HairvestMoon.Core;
using System.Collections.Generic;
using UnityEngine;

namespace HairvestMoon.Farming
{
    /// <summary>
    /// Holds all seed types for look-up and UI purposes. 
    /// Fast lookup from ItemData (inventory) to SeedData.
    /// </summary>
    public class SeedDatabase : MonoBehaviour, IBusListener
    {
        [SerializeField] private List<SeedData> allSeeds;

        private Dictionary<ItemData, SeedData> lookup = new();

        public List<SeedData> AllSeeds => allSeeds;

        public void RegisterBusListeners()
        {
            var bus = ServiceLocator.Get<GameEventBus>();
            bus.GlobalSystemsInitialized += OnGlobalSystemsInitialized;
        }

        private void OnGlobalSystemsInitialized()
        {
            BuildSeedLookup();
        }

        /// <summary>
        /// Builds the lookup dictionary from item to seed.
        /// </summary>
        public void BuildSeedLookup()
        {
            lookup.Clear();
            foreach (var seed in allSeeds)
            {
                if (seed.seedItem != null)
                {
                    if (lookup.ContainsKey(seed.seedItem))
                        Debug.LogWarning($"Duplicate seedItem in SeedDatabase: {seed.seedItem.name}");
                    lookup[seed.seedItem] = seed;
                }
            }
        }

        /// <summary>
        /// Returns the SeedData for a given item (if it is a seed).
        /// </summary>
        public SeedData GetSeedDataByItem(ItemData item)
        {
            lookup.TryGetValue(item, out var seedData);
            return seedData;
        }
    }
}
