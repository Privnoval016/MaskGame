using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Extensions.Timers;
using UnityEngine;
using UnityEngine.InputSystem;
using PrimeTween;
using UnityEngine.Serialization;

namespace Extensions.UI
{
    public enum MovementAxis
    {
        Horizontal,
        Vertical
    }
    public class ScrollMenu : MonoBehaviour
    {
        [Header("Scroll Settings")]
        public MovementAxis axis;
        
        public int NumPanels => scrollItemUIPanels.Count;
        [HideInInspector] public bool ableToScroll = true; // Is scrolling enabled based on number of items vs panels
        
        [Header("Item Settings")]
        public Transform scrollItemContainer;
        public List<ScrollUIPanel> scrollItemUIPanels;
        [FormerlySerializedAs("selectedItemIndex")] public int selectedUIPanelIndex = 0;
        public ScrollUIPanel SelectedItem => scrollItemUIPanels[selectedUIPanelIndex];
        
        private List<Vector3> itemPositions = new List<Vector3>();
        
        private CountdownTimer scrollCooldownTimer;
        private float scrollCooldown = 0.11f;
        private float scrollDuration = 0.1f;

        [HideInInspector] public int activePanels;
        
        [HideInInspector] public bool canScroll = true;
        
        #region Inventory Parameters
        
        private IList inventoryItems;
        private int inventoryStartIndex = 0;
        private Func<object, ItemUIInfo> getItemInfoFunc;
        
        
        #endregion


        private void Awake()
        {
            scrollItemUIPanels = scrollItemContainer.GetComponentsInChildren<ScrollUIPanel>(true).ToList();

            itemPositions = scrollItemUIPanels.Select(panel => panel.rectTransform.localPosition).ToList();
            
            InitializeScrollTimer();

            ableToScroll = true;
        }
        
        private void InitializeScrollTimer()
        {
            if (scrollCooldownTimer != null) return;
            
            scrollCooldownTimer = new CountdownTimer(scrollCooldown, true);
            
            scrollCooldownTimer.OnTimerStart += () => canScroll = false;
            scrollCooldownTimer.OnTimerStop += () => canScroll = true;
        }
        
        public void Activate<T>(List<T> items, int initialIndex, Func<T, ItemUIInfo> getInfoFunc)
        {
            selectedUIPanelIndex = 0;
            InputManager.Instance.onScroll += OnScroll;
            
            InitializeScrollTimer();
            
            scrollCooldownTimer.Stop();
            InitializeMenuWithInventory(items, initialIndex, getInfoFunc);
        }
        
        public void Deactivate()
        {
            InputManager.Instance.onScroll -= OnScroll;
            
            InitializeScrollTimer();
            
            scrollCooldownTimer.Stop();
            
            inventoryItems = null;
            getItemInfoFunc = null;
            inventoryStartIndex = 0;
        }
        
        private void OnScroll(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Canceled) return;
            
            if (!canScroll) return;
            
            scrollCooldownTimer.Restart();

            Vector2 scrollValue = context.ReadValue<Vector2>();
            float value = (axis == MovementAxis.Horizontal) ? scrollValue.x : scrollValue.y;
            
            if (value > 0f)
            {
                ScrollRight();
            }
            else if (value < 0f)
            {
                ScrollLeft();
            }
        }
        
        private void ScrollLeft()
        {
            Debug.Log("Scrolling Left");
            
            SelectedItem.OnDeselected();
            
            var shiftedPanel = scrollItemUIPanels[0];
            scrollItemUIPanels.RemoveAt(0);
            scrollItemUIPanels.Add(shiftedPanel);
            
            if (!ableToScroll)
            {
                selectedUIPanelIndex -= 1;
                
                int direction = -1;
                if (selectedUIPanelIndex < 0)
                {
                    selectedUIPanelIndex = 0;
                    direction = 0;
                }
                ScrollIndex(direction);
                
                SelectedItem.OnSelected();
                return;
            }

            selectedUIPanelIndex = 0;
            
            ScrollIndex(-1);
            SelectedItem.OnSelected();
            
            for (int i = 0; i < scrollItemUIPanels.Count - 1; i++)
            {
                var panel = scrollItemUIPanels[i];
                var targetPosition = itemPositions[i];
                
                Tween.LocalPosition(panel.rectTransform, targetPosition, scrollDuration, Ease.InOutCubic, 1, 
                    CycleMode.Restart, 0, 0, true);
            }
            
            var newPosition = itemPositions[0] + (itemPositions[0] - itemPositions[1]);
            
            Tween.LocalPosition(shiftedPanel.rectTransform, newPosition, scrollDuration, Ease.InOutCubic, 1, 
                CycleMode.Restart, 0, 0, true).OnComplete(() =>
            {
                shiftedPanel.rectTransform.localPosition = itemPositions.Last();
            });
            
            RefreshScrollPanels(-1, shiftedPanel);
        }
        
        private void ScrollRight()
        {
            Debug.Log("Scrolling Right");
            
            SelectedItem.OnDeselected();
            
            var shiftedPanel = scrollItemUIPanels.Last();
            scrollItemUIPanels.RemoveAt(scrollItemUIPanels.Count - 1);
            scrollItemUIPanels.Insert(0, shiftedPanel);
            
            if (!ableToScroll)
            {
                selectedUIPanelIndex += 1;
                
                int direction = 1;
                if (selectedUIPanelIndex >= activePanels)
                {
                    selectedUIPanelIndex = activePanels - 1;
                    direction = 0;
                }
                ScrollIndex(direction);
                
                SelectedItem.OnSelected();
                return;
            }
            
            ScrollIndex(1);
            SelectedItem.OnSelected();
            
            RefreshScrollPanels(1, shiftedPanel);
            
            for (int i = 1; i < scrollItemUIPanels.Count; i++)
            {
                var panel = scrollItemUIPanels[i];
                var targetPosition = itemPositions[i];

                Tween.LocalPosition(panel.rectTransform, targetPosition, scrollDuration, Ease.InOutCubic, 1, 
                    CycleMode.Restart, 0, 0, true);
            }
            
            var newPosition = itemPositions[0] + (itemPositions[0] - itemPositions[1]);
            
            shiftedPanel.rectTransform.localPosition = newPosition;
            
            Tween.LocalPosition(shiftedPanel.rectTransform, itemPositions[0], scrollDuration, Ease.InOutCubic, 1,
                CycleMode.Restart, 0, 0, true);
        }

        /**
         * <summary>
         * Initializes the scroll menu with a list of items from an inventory.
         * </summary>
         *
         * <typeparam name="T">The type of items in the inventory.</typeparam>
         * <param name="items">The list of items to display in the scroll menu.</param>
         * <param name="initialIndex">The index of the item to be initially selected.</param>
         * <param name="getInfoFunc">A function that takes an item of type T and returns its ItemUIInfo.</param>
         */
        private void InitializeMenuWithInventory<T>(List<T> items, int initialIndex, Func<T, ItemUIInfo> getInfoFunc)
        {
            if (initialIndex < 0) initialIndex = 0;
            
            ableToScroll = items.Count >= NumPanels;
            
            activePanels = 0;
            for (int i = 0; i < NumPanels; i++)
            {
                int index = initialIndex + i;
                var panel = scrollItemUIPanels[i];
                if (index >= items.Count)
                {
                    panel.gameObject.SetActive(false);
                }
                else
                {
                    panel.gameObject.SetActive(true);
                    Debug.Log("Refreshing panel " + panel.name + " with accessory at index " + index);
                    var accessory = items[index];
                    panel.Refresh(getInfoFunc(accessory));
                    activePanels++;
                }
            }
            
            inventoryItems = items;
            inventoryStartIndex = initialIndex;
            getItemInfoFunc = obj => getInfoFunc((T)obj);
        }

        /**
         * <summary>
         * Scrolls the stored indices of the menu in the given direction.
         * </summary>
         */
        private void ScrollIndex(int direction)
        {
            if (direction == 0) return;
            
            inventoryStartIndex -= direction;
            inventoryStartIndex += inventoryItems.Count;
            inventoryStartIndex %= inventoryItems.Count;
            Debug.Log("Current item stack index in UI: " + inventoryStartIndex);
        }
        
        /**
         * <summary>
         * Refreshes the scroll panels based on the information from the inventory.
         * </summary>
         *
         * <param name="direction">The direction of the scroll (1 for right, -1 for up).</param>
         * <param name="refreshedPanel">The panel that was refreshed (if any).</param>
         */
        private void RefreshScrollPanels(int direction, ScrollUIPanel refreshedPanel)
        {
            if (direction == 0) return;
            
            int refreshedItemIndex = direction > 0 ? inventoryStartIndex : NumPanels - 1 + inventoryStartIndex;
            refreshedItemIndex %= inventoryItems.Count;
        
            var accessory = inventoryItems[refreshedItemIndex];
            refreshedPanel.Refresh(getItemInfoFunc(accessory));
        }

        /**
         * <summary>
         * Gets the index of the currently selected item in the inventory list.
         * </summary>
         *
         * <returns>The index of the selected item in the inventory list.</returns>
         */
        public int GetIndexInInventory()
        {
            return 0;
        }
    }
}