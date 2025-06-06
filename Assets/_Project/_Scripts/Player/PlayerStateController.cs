using HairvestMoon.Core;
using System.Collections.Generic;
using UnityEngine;

namespace HairvestMoon.Player
{
    [System.Serializable]
    public class PlayerFormData
    {
        public PlayerStateController.PlayerForm FormType;
        public GameObject VisualRoot;
        public Animator Animator;
        public SpriteRenderer Renderer;
        public float MoveSpeed;
    }

    // Singleton
    // Manages visual GameObjects, animators, and movement stats per form
    // Provides clean access to form-specific data like Animator and speed

    public class PlayerStateController : MonoBehaviour
    {
        public enum PlayerForm { Human, Werewolf }
        public PlayerForm CurrentForm { get; private set; }

        [SerializeField] private List<PlayerFormData> formDataList;

        private PlayerFormData _currentFormData;

        public void InitializePlayerState()
        {
            CurrentForm = PlayerForm.Human;
        }

        public void EnterWerewolfForm() => SwitchToForm(PlayerForm.Werewolf);
        public void ExitWerewolfForm() => SwitchToForm(PlayerForm.Human);

        private void SwitchToForm(PlayerForm newForm)
        {
            foreach (var form in formDataList)
            {
                bool isActive = form.FormType == newForm;
                form.VisualRoot.SetActive(isActive);
                if (isActive)
                {
                    _currentFormData = form;
                    CurrentForm = newForm;

                    ServiceLocator.Get<GameEventBus>().RaisePlayerFormChanged(CurrentForm);
                }
            }
        }


        public Animator CurrentAnimator => _currentFormData.Animator;
        public SpriteRenderer CurrentSpriteRenderer => _currentFormData.Renderer;
        public float MoveSpeed => _currentFormData.MoveSpeed;
        public bool IsWerewolf() => CurrentForm == PlayerForm.Werewolf;
    }

}