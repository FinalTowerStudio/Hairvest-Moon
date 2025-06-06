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
        }

        public void OpenMenu()
        {
            gameObject.SetActive(true);
            mainMenuCanvasGroup.alpha = 1f;
            mainMenuCanvasGroup.interactable = true;
            mainMenuCanvasGroup.blocksRaycasts = true;
        }

        public void CloseMenu()
        {
            mainMenuCanvasGroup.alpha = 0f;
            mainMenuCanvasGroup.interactable = false;
            mainMenuCanvasGroup.blocksRaycasts = false;
        }

        private void HandleMenuToggle()
        {
            if (ServiceLocator.Get<GameStateManager>().CurrentState == GameState.Menu)
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
                cropLogPanel.GetComponent<InventoryOverviewUI>().RefreshUI();
        }


        public void NextTab()
        {
            int nextIndex = (currentTabIndex + 1) % allPanels.Length;
            OpenTab(nextIndex);
        }

        public void PreviousTab()
        {
            int prevIndex = (currentTabIndex - 1 + allPanels.Length) % allPanels.Length;
            OpenTab(prevIndex);
        }
    }
}