using HairvestMoon.Core;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace HairvestMoon.Farming
{
    /// <summary>
    /// Displays water level sliders above watered tiles on the farm.
    /// </summary>
    public class WaterVisualSystem : MonoBehaviour, IBusListener, ITickable
    {
        [SerializeField] private GameObject waterSliderPrefab;
        [SerializeField] private Grid farmGrid;
        [SerializeField] private float tileOffsetY = 1.0f;

        private Dictionary<Vector3Int, WaterSliderInstance> activeSliders = new();
        private Dictionary<Vector3Int, float> targetSliderFill = new();
        private FarmTileDataManager _farmTileDataManager;
        private bool isInitialized = false;

        public void RegisterBusListeners()
        {
            var bus = ServiceLocator.Get<GameEventBus>();
            bus.GlobalSystemsInitialized += OnGlobalSystemsInitialized;
            ServiceLocator.Get<GameTimeManager>().RegisterTickable(this);
        }

        private void OnGlobalSystemsInitialized()
        {
            _farmTileDataManager = ServiceLocator.Get<FarmTileDataManager>();
            RefreshSliders();
            isInitialized = true;
        }

        public void Tick(GameTimeChangedArgs args)
        {
            if (!isInitialized) return;
            foreach (var entry in activeSliders)
            {
                var pos = entry.Key;
                var data = _farmTileDataManager.GetTileData(pos);
                float target = data.waterMinutesRemaining / (float)FarmTileData.MinutesPerWatering;
                targetSliderFill[pos] = Mathf.Clamp01(target);
            }
        }

        private void Update()
        {
            if (!isInitialized) return;
            foreach (var entry in activeSliders)
            {
                var pos = entry.Key;
                var instance = entry.Value;

                float current = instance.GetFill();
                float target = targetSliderFill.TryGetValue(pos, out float t) ? t : 0f;
                float newFill = Mathf.MoveTowards(current, target, Time.deltaTime); // interpolate
                instance.SetFill(newFill);
            }
        }

        public void HandleWateredTile(Vector3Int pos, FarmTileData data)
        {
            if (data.isWatered)
            {
                if (!activeSliders.ContainsKey(pos))
                    CreateSlider(pos);
            }
            else
            {
                RemoveSlider(pos);
            }
        }

        private void RefreshSliders()
        {
            foreach (var entry in _farmTileDataManager.AllTileData)
            {
                var pos = entry.Key;
                var data = entry.Value;

                if (!data.isTilled || !data.isWatered)
                {
                    RemoveSlider(pos);
                    continue;
                }

                if (!activeSliders.ContainsKey(pos))
                    CreateSlider(pos);
            }
        }

        private void CreateSlider(Vector3Int pos)
        {
            Debug.Log($"Creating slider for {pos}");
            var worldPos = farmGrid.CellToWorld(pos) + new Vector3(0.5f, tileOffsetY, 0);
            var instanceObj = Instantiate(waterSliderPrefab, worldPos, Quaternion.identity, transform);

            // Get the correct initial fill value based on water remaining
            var data = _farmTileDataManager.GetTileData(pos);
            float initialFill = 1f; // fallback
            if (data != null && FarmTileData.MinutesPerWatering > 0)
                initialFill = Mathf.Clamp01(data.waterMinutesRemaining / (float)FarmTileData.MinutesPerWatering);

            var instance = new WaterSliderInstance(instanceObj);
            instance.SetFill(initialFill); 
            activeSliders[pos] = instance;
            targetSliderFill[pos] = initialFill;
        }

        private void RemoveSlider(Vector3Int pos)
        {
            if (!activeSliders.ContainsKey(pos)) return;
            Destroy(activeSliders[pos].gameObject);
            activeSliders.Remove(pos);
        }

        // Optional: extend with SetColor for fancier feedback
        private class WaterSliderInstance
        {
            public readonly GameObject gameObject;
            private readonly Slider slider;

            public WaterSliderInstance(GameObject obj)
            {
                gameObject = obj;
                slider = obj.GetComponentInChildren<Slider>();
            }

            public void SetFill(float fillAmount)
            {
                slider.value = fillAmount;
            }

            public float GetFill()
            {
                return slider.value;
            }

            // Optional: if we want to change color based on fill
            // public void SetColor(Color color) { /* ... */ }
        }
    }
}
