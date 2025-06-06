using HairvestMoon.Core;
using HairvestMoon.Farming;
using System.Collections.Generic;
using UnityEngine;

public class WaterVisualSystem : MonoBehaviour
{
    [SerializeField] private GameObject waterSliderPrefab;
    [SerializeField] private Grid farmGrid;
    [SerializeField] private float tileOffsetY = 1.0f;

    private Dictionary<Vector3Int, WaterSliderInstance> activeSliders = new();
    private bool isInitialized = false;

    public void RegisterBusListeners()
    {
        var bus = ServiceLocator.Get<GameEventBus>();
        bus.GlobalSystemsInitialized += OnGlobalSystemsInitialized;
    }

    private void OnGlobalSystemsInitialized()
    {
        RefreshSliders(0, 0);
        isInitialized = true;
    }

    private void Update()
    {
        if (!isInitialized) return;

        foreach (var entry in activeSliders)
        {
            var pos = entry.Key;
            var data = ServiceLocator.Get<FarmTileDataManager>().GetTileData(pos);

            float progress = data.waterMinutesRemaining / FarmTileData.MinutesPerWatering;
            entry.Value.SetFill(Mathf.Clamp01(progress));
        }
    }

    //This isn't being called, we need to see if this is uneccessary now, or if we are missing a call to it
    public void RefreshSliders(int hour, int minute)
    {
        foreach (var entry in ServiceLocator.Get<FarmTileDataManager>().AllTileData)
        {
            var pos = entry.Key;
            var data = entry.Value;

            if (!data.isTilled)
            {
                RemoveSlider(pos);
                continue;
            }

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

    private void CreateSlider(Vector3Int pos)
    {
        var worldPos = farmGrid.CellToWorld(pos) + new Vector3(0.5f, tileOffsetY, 0);
        var instance = Instantiate(waterSliderPrefab, worldPos, Quaternion.identity, transform);
        activeSliders[pos] = new WaterSliderInstance(instance);
    }

    private void RemoveSlider(Vector3Int pos)
    {
        if (!activeSliders.ContainsKey(pos)) return;
        Destroy(activeSliders[pos].gameObject);
        activeSliders.Remove(pos);
    }

    private class WaterSliderInstance
    {
        public readonly GameObject gameObject;
        private readonly UnityEngine.UI.Slider slider;

        public WaterSliderInstance(GameObject obj)
        {
            gameObject = obj;
            slider = obj.GetComponentInChildren<UnityEngine.UI.Slider>();
        }

        public void SetFill(float fillAmount)
        {
            slider.value = Mathf.Clamp01(fillAmount);
        }
    }
}
