using HairvestMoon.Core;
using UnityEngine;

public class SystemTemplate : MonoBehaviour, IBusListener
{
    private bool isInitialized = false;

    public void RegisterBusListeners()
    {
        var bus = ServiceLocator.Get<GameEventBus>();
        bus.GlobalSystemsInitialized += OnGlobalSystemsInitialized;

        // Subscribe to other bus events here
        bus.TimeChanged += OnTimeChanged;
        bus.GameStateChanged += OnGameStateChanged;
        // etc
    }

    private void OnGlobalSystemsInitialized()
    {
        Initialize();
        isInitialized = true;
    }

    public void Initialize()
    {
        // Put your system state setup logic here
    }

    private void Update()
    {
        if (!isInitialized) return;

        // Your runtime logic here
    }

    private void OnTimeChanged(GameTimeChangedArgs args)
    {
        if (!isInitialized) return;

        // Time-based logic
    }

    private void OnGameStateChanged(GameStateChangedArgs args)
    {
        if (!isInitialized) return;

        // State change handling logic
    }
}
