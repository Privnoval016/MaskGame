using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Extensions.UI
{
    public class TabGroup : MonoBehaviour
    {
        [Header("Tabs")] public List<TabButton> tabButtons;
        public TabButton selectedTab;
        private int SelectedTabIndex => tabButtons.IndexOf(selectedTab);

        [Header("Input Actions")] 
        public bool tabActive = true;

        #region MonoBehaviour Callbacks

        private void Awake()
        {
            InputManager.Instance.onTabLeft += OnTabLeft;
            InputManager.Instance.onTabRight += OnTabRight;
        }

        private void Start()
        {
            if (tabButtons == null || tabButtons.Count == 0) return;

            // Select the first tab by default
            
            selectedTab = tabButtons[0];
            selectedTab.Select();
        }

        #endregion

        #region Input Callbacks

        private void OnTabLeft(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
            
            if (!tabActive) return;

            if (tabButtons.Count == 0) return;

            if (selectedTab != null)
            {
                selectedTab.Deselect();
            }

            if (SelectedTabIndex <= 0)
            {
                selectedTab = tabButtons[^1];
            }
            else
            {
                selectedTab = tabButtons[SelectedTabIndex - 1];
            }

            selectedTab.Select();
        }

        private void OnTabRight(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
            
            if (!tabActive) return;

            if (tabButtons.Count == 0) return;

            if (selectedTab != null)
            {
                selectedTab.Deselect();
            }

            if (SelectedTabIndex >= tabButtons.Count - 1)
            {
                selectedTab = tabButtons[0];
            }
            else
            {
                selectedTab = tabButtons[SelectedTabIndex + 1];
            }

            selectedTab.Select();
        }

        #endregion

        #region Tab Button Methods


        public void Subscribe(TabButton button)
        {
            if (tabButtons == null)
                tabButtons = new List<TabButton>();

            if (!tabButtons.Contains(button))
                tabButtons.Add(button);
        }

        #endregion
    }
}
