using UnityEngine;
using TMPro;
using HairvestMoon.Core;
using HairvestMoon.Player;
using HairvestMoon.Farming;
using HairvestMoon.Tool;

namespace HairvestMoon.Utility
{
    /// <summary>
    /// UI script for debugging: shows time, day, current form, game state, current tool, and last action.
    /// Updates live from GameTimeManager, GameStateManager, PlayerStateController, ToolSystem.
    /// </summary>
    public class DebugUIOverlay : MonoBehaviour, IBusListener
    {
        [Header("Debug UI References")]
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private TextMeshProUGUI dayText;
        [SerializeField] private TextMeshProUGUI formText;
        [SerializeField] private TextMeshProUGUI stateText;
        [SerializeField] private TextMeshProUGUI toolText;
        [SerializeField] private TextMeshProUGUI lastActionText;

        private GameTimeManager _gameTimeManager;
        private GameStateManager _gameStateManager;
        private PlayerStateController _playerStateController;
        private ToolSystem _toolSystem;

        private bool isInitialized = false;

        public void RegisterBusListeners()
        {
            var bus = ServiceLocator.Get<GameEventBus>();
            bus.GlobalSystemsInitialized += OnGlobalSystemsInitialized;
            bus.TimeChanged += OnTimeChanged;
            bus.GameStateChanged += OnGameStateChanged;
            bus.PlayerFormChanged += OnPlayerFormChanged;
            bus.ToolChanged += OnToolChanged;
            // You can add more event hooks if needed
        }

        private void OnGlobalSystemsInitialized()
        {
            _gameTimeManager = ServiceLocator.Get<GameTimeManager>();
            _gameStateManager = ServiceLocator.Get<GameStateManager>();
            _playerStateController = ServiceLocator.Get<PlayerStateController>();
            _toolSystem = ServiceLocator.Get<ToolSystem>();

            isInitialized = true;
            UpdateAllTexts();
        }

        private void UpdateAllTexts()
        {
            if (_gameTimeManager != null)
                UpdateTimeText(_gameTimeManager.CurrentHour, _gameTimeManager.CurrentMinute);

            if (_gameTimeManager != null && dayText != null)
                dayText.text = $"Day: {_gameTimeManager.Day}";

            if (_playerStateController != null && formText != null)
                formText.text = $"Form: {_playerStateController.CurrentForm}";

            if (_gameStateManager != null && stateText != null)
                stateText.text = $"GameState: {_gameStateManager.CurrentState}";

            if (_toolSystem != null && toolText != null)
                toolText.text = $"Tool: {_toolSystem.CurrentTool}";
        }

        private void Update()
        {
            if (!isInitialized) return;
            // Show current day (live update if you want)
            if (_gameTimeManager != null && dayText != null)
                dayText.text = $"Day: {_gameTimeManager.Day}";

            // Show current player form (live update if you want)
            if (_playerStateController != null && formText != null)
                formText.text = $"Form: {_playerStateController.CurrentForm}";
        }

        public void UpdateTimeText(int hour, int minute)
        {
            if (timeText != null)
                timeText.text = $"Time: {hour:00}:{minute:00}";
        }

        public void UpdateStateText(GameState state)
        {
            if (stateText != null)
                stateText.text = $"GameState: {state}";
        }

        public void OnTimeChanged(GameTimeChangedArgs args)
        {
            UpdateTimeText(args.Hour, args.Minute);
        }

        public void OnGameStateChanged(GameStateChangedArgs args)
        {
            UpdateStateText(args.State);
        }

        public void OnPlayerFormChanged(PlayerFormChangedArgs args)
        {
            if (formText != null)
                formText.text = $"Form: {args.Form}";
        }

        public void OnToolChanged(ToolType tool)
        {
            if (toolText != null)
                toolText.text = $"Tool: {tool}";
        }

        public void ShowLastAction(string text)
        {
            if (lastActionText != null)
                lastActionText.text = $"Last Action: {text}";
        }
    }
}
