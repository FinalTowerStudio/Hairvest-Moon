using UnityEngine;
using TMPro;
using HairvestMoon.Core;
using HairvestMoon.Player;
using HairvestMoon.Farming;

namespace HairvestMoon.Utility
{

    // UI script for debugging: shows time, day, current form, and game state
    // Automatically updates from GameTimeManager, GameStateManager, PlayerStateController

    public class DebugUIOverlay : MonoBehaviour, IBusListener
    {
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private TextMeshProUGUI dayText;
        [SerializeField] private TextMeshProUGUI formText;
        [SerializeField] private TextMeshProUGUI stateText;
        [SerializeField] private TextMeshProUGUI toolText;
        [SerializeField] private TextMeshProUGUI lastActionText;

        private GameTimeManager _gameTimeManager;
        private PlayerStateController _playerStateController;
        private FarmToolHandler _farmToolHandler;
        private bool isInitialized = false;

        public void RegisterBusListeners()
        {
            var bus = ServiceLocator.Get<GameEventBus>();
            bus.GlobalSystemsInitialized += OnGlobalSystemsInitialized;
        }

        private void OnGlobalSystemsInitialized()
        {
            isInitialized = true;
        }

        public void InitializeUI()
        {
            _gameTimeManager = ServiceLocator.Get<GameTimeManager>();
            _playerStateController = ServiceLocator.Get<PlayerStateController>();
            _farmToolHandler = ServiceLocator.Get<FarmToolHandler>();
        }

        private void Update()
        {
            if (!isInitialized) return;

            dayText.text = $"Day: {_gameTimeManager.Day}";

            formText.text = $"Form: {_playerStateController.CurrentForm}";
        }

        public void UpdateTimeText(int hour, int minute)
        {
            timeText.text = $"Time: {hour:00}:{minute:00}";
        }

        public void UpdateStateText(GameState state)
        {
            stateText.text = $"GameState: {state}";
        }

        public void ShowLastAction(string text) => lastActionText.text = $"Last Action: {text}";
    }


}