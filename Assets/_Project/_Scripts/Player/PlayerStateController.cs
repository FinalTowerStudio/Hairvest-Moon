using HairvestMoon.Core;
using UnityEngine;

namespace HairvestMoon.Player
{
    public class PlayerStateController : MonoBehaviour, IBusListener
    {
        [System.Serializable]
        public class PlayerFormData
        {
            public PlayerForm FormType;
            public GameObject VisualRoot;
            public Animator Animator;
            public SpriteRenderer Renderer;
            public float MoveSpeed;
        }

        public enum PlayerForm { Human, Werewolf }
        public PlayerForm CurrentForm { get; private set; }

        [Header("Player Forms")]
        [SerializeField] private PlayerFormData[] formDataList;

        private PlayerFormData _currentFormData;
        private GameEventBus _eventBus;

        public Animator CurrentAnimator => _currentFormData.Animator;
        public SpriteRenderer CurrentSpriteRenderer => _currentFormData.Renderer;
        public float MoveSpeed => _currentFormData.MoveSpeed;
        public bool IsFormInitialized { get; private set; } = false;

        public void RegisterBusListeners()
        {
            _eventBus = ServiceLocator.Get<GameEventBus>();
            _eventBus.GlobalSystemsInitialized += OnGlobalSystemsInitialized;
        }

        private void OnGlobalSystemsInitialized()
        {
            Initialize();
        }

        public void Initialize()
        {
            SwitchToFormInternal(PlayerForm.Human);
            IsFormInitialized = true;
        }

        public bool RequestPlayerForm(PlayerForm newForm)
        {
            if (CurrentForm == newForm) return false;

            if (!CanChangeForm(newForm))
                return false; // Block for quest, item, or cooldown

            SwitchToFormInternal(newForm);
            return true;
        }

        private bool CanChangeForm(PlayerForm requestedForm)
        {
            // Insert any checks here (e.g., only during night, after X seconds, if holding a special item, etc.)
            return true;
        }

        private void SwitchToFormInternal(PlayerForm newForm)
        {
            foreach (var form in formDataList)
                form.VisualRoot.SetActive(false);

            CurrentForm = newForm;
            _currentFormData = System.Array.Find(formDataList, f => f.FormType == CurrentForm);
            _currentFormData?.VisualRoot.SetActive(true);

            _eventBus.RaisePlayerFormChanged(CurrentForm);
        }
    }
}
