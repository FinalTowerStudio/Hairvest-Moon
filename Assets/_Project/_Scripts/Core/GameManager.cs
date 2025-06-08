using HairvestMoon.Player;
using UnityEngine;

namespace HairvestMoon.Core
{
    public class GameManager : IBusListener
    {
        private PlayerStateController _playerState;
        private GameStateManager _gameStateManager;

        private bool isInitialized = false;

        public void RegisterBusListeners()
        {
            var bus = ServiceLocator.Get<GameEventBus>();
            bus.GlobalSystemsInitialized += OnGlobalSystemsInitialized;
            bus.OnDawn += HandleDawn;
            bus.OnDusk += HandleDusk;
            bus.OnNewDay += HandleNewDay;
        }

        private void OnGlobalSystemsInitialized()
        {
            Initialize();
            isInitialized = true;
        }

        public void Initialize()
        {
            _playerState = ServiceLocator.Get<PlayerStateController>();
            _gameStateManager = ServiceLocator.Get<GameStateManager>();
        }

        public void HandleDusk()
        {
            _playerState.RequestPlayerForm(PlayerStateController.PlayerForm.Werewolf);
        }

        public void HandleDawn()
        {
            _gameStateManager.RequestStateChange(GameState.FreeRoam);
            _playerState.RequestPlayerForm(PlayerStateController.PlayerForm.Human);
        }

        public void HandleNewDay()
        {
            // Reset quests/tasks/shops/etc at midnight.  Different than Dawn at 6am.
        }
    }
}