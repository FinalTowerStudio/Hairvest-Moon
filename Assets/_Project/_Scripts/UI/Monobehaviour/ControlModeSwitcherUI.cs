using UnityEngine;
using UnityEngine.UI;
using HairvestMoon.Core;

/// <summary>
/// UI system for toggling between Keyboard/Mouse and Gamepad input.
/// Follows all systemic rules: event-driven, bus registration, never directly mutates other systems.
/// </summary>
public class ControlModeSwitchUI : MonoBehaviour, IBusListener
{
    [Header("Buttons")]
    [SerializeField] private Button keyboardButton;
    [SerializeField] private Button gamepadButton;

    [Header("Highlights/Visuals")]
    [SerializeField] private GameObject keyboardHighlight; // optional
    [SerializeField] private GameObject gamepadHighlight;  // optional

    private InputController inputController;
    private GameEventBus eventBus;
    private bool isInitialized = false;

    public void RegisterBusListeners()
    {
        eventBus = ServiceLocator.Get<GameEventBus>();
        eventBus.GlobalSystemsInitialized += OnGlobalSystemsInitialized;
        eventBus.ControlModeChanged += OnControlModeChanged;
        // Add any additional event hooks you need
    }

    private void OnGlobalSystemsInitialized()
    {
        inputController = ServiceLocator.Get<InputController>();
        Initialize();
        isInitialized = true;
    }

    public void Initialize()
    {
        if (keyboardButton != null)
            keyboardButton.onClick.AddListener(() => SetMode(ControlMode.Mouse));
        if (gamepadButton != null)
            gamepadButton.onClick.AddListener(() => SetMode(ControlMode.Gamepad));

        // Start with whatever mode is currently set
        UpdateVisuals(inputController.CurrentMode);
    }

    private void SetMode(ControlMode mode)
    {
        if (!isInitialized) return;
        inputController.SetControlMode(mode); // Will raise ControlModeChanged event
        // UI will update via event
    }

    private void OnControlModeChanged(ControlModeChangedArgs args)
    {
        UpdateVisuals(args.Mode);
    }

    private void UpdateVisuals(ControlMode mode)
    {
        // Grey out the active mode button and highlight it
        if (keyboardButton != null)
            keyboardButton.interactable = mode != ControlMode.Mouse;
        if (gamepadButton != null)
            gamepadButton.interactable = mode != ControlMode.Gamepad;

        if (keyboardHighlight != null)
            keyboardHighlight.SetActive(mode == ControlMode.Mouse);
        if (gamepadHighlight != null)
            gamepadHighlight.SetActive(mode == ControlMode.Gamepad);
    }
}
