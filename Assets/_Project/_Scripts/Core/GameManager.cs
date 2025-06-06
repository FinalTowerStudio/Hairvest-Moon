using HairvestMoon.Player;
using HairvestMoon.Tool;
using HairvestMoon.UI;
using UnityEngine;

namespace HairvestMoon.Core
{
    // Singleton
    // Subscribes to OnDusk and OnDawn
    // Triggers werewolf transformation and state changes

    public class GameManager : MonoBehaviour, IBusListener
    {
        [SerializeField] private GameTimeManager _timeManager;
        [SerializeField] private PlayerStateController _playerState;

        private bool isInitialized = false;

        private void OnGlobalSystemsInitialized()
        {
            isInitialized = true;
        }

        public void RegisterBusListeners()
        {
            var bus = ServiceLocator.Get<GameEventBus>();
            bus.GlobalSystemsInitialized += OnGlobalSystemsInitialized;
            bus.OnDawn += HandleDawn;
            bus.OnDusk += HandleDusk;
        }

        public void HandleDusk()
        {
            _playerState.EnterWerewolfForm();
        }

        public void HandleDawn()
        {
            ServiceLocator.Get<GameStateManager>().SetState(GameState.FreeRoam);
            _playerState.ExitWerewolfForm();
        }
    }
}