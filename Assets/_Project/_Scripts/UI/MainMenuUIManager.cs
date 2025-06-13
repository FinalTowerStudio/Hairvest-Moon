using System.Collections.Generic;
using UnityEngine;
using HairvestMoon.Core;
using HairvestMoon.Inventory;

namespace HairvestMoon.UI
{
    ///
    public class MainMenuUIManager : MonoBehaviour, IBusListener
    {
        [Header("UI Panels")]
        [SerializeField] private GameObject inventoryPanel;
        [SerializeField] private GameObject cropLogPanel;
        [SerializeField] private GameObject mapPanel;
        [SerializeField] private GameObject questPanel;
        [SerializeField] private GameObject settingsPanel;

        [Header("Tab Buttons")]
        [SerializeField] private List<GameObject> tabButtons;

        private int currentTabIndex = 0;
        private GameObject[] allPanels;
        private CanvasGroup mainMenuCanvasGroup;
        private bool isOpen = true;

        public void InitializeUI()
        {
            mainMenuCanvasGroup = GetComponent<CanvasGroup>();
            allPanels = new GameObject[] { inventoryPanel, cropLogPanel, mapPanel, questPanel, settingsPanel };
            CloseMenu();
        }

        public void RegisterBusListeners()
        {
            var bus = ServiceLocator.Get<GameEventBus>();
            bus.MenuToggle += HandleMenuToggle;
            bus.ToolNext += NextTab;
            bus.ToolPrevious += PreviousTab;
        }

        public void OpenMenu()
        {
            if (isOpen) return;
            gameObject.SetActive(true);
            mainMenuCanvasGroup.alpha = 1f;
            mainMenuCanvasGroup.interactable = true;
            mainMenuCanvasGroup.blocksRaycasts = true;
            isOpen = true;

            // Set game state to menu and lock input
            var gameStateManager = ServiceLocator.Get<GameStateManager>();
            gameStateManager.RequestStateChange(GameState.Menu);
            ServiceLocator.Get<GameEventBus>().RaiseInputLockChanged(true);
        }

        public void CloseMenu()
        {
            if (!isOpen) return;
            mainMenuCanvasGroup.alpha = 0f;
            mainMenuCanvasGroup.interactable = false;
            mainMenuCanvasGroup.blocksRaycasts = false;
            isOpen = false;

            // Set game state back to FreeRoam (or previous state) and unlock input
            var gameStateManager = ServiceLocator.Get<GameStateManager>();
            gameStateManager.RequestStateChange(GameState.FreeRoam); // or cached previous state if we have one
            ServiceLocator.Get<GameEventBus>().RaiseInputLockChanged(false);
        }

        private void HandleMenuToggle()
        {
            if (isOpen)
                CloseMenu();
            else
                OpenMenu();
        }


        public void OpenTab(int index)
        {
            currentTabIndex = index;

            for (int i = 0; i < allPanels.Length; i++)
            {
                var canvasGroup = allPanels[i].GetComponent<CanvasGroup>();
                bool active = (i == currentTabIndex);

                canvasGroup.alpha = active ? 1f : 0f;
                canvasGroup.interactable = active;
                canvasGroup.blocksRaycasts = active;
            }

            if (currentTabIndex == 0)
                inventoryPanel.GetComponent<BackpackInventoryUI>().RefreshUI();

            if (currentTabIndex == 1)
                cropLogPanel.GetComponent<ResourceInventoryUI>().RefreshUI();
        }


        public void NextTab()
        {
            if (!isOpen) return;
            int nextIndex = (currentTabIndex + 1) % allPanels.Length;
            OpenTab(nextIndex);
        }

        public void PreviousTab()
        {
            if (!isOpen) return;
            int prevIndex = (currentTabIndex - 1 + allPanels.Length) % allPanels.Length;
            OpenTab(prevIndex);
        }

        // For HUD button
        public void ToggleMenuButton()
        {
            HandleMenuToggle();
        }
    }
}