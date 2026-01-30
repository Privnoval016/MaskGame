using PrimeTween;
using UnityEngine;

namespace Extensions.UI
{
    public class TabButton : MonoBehaviour
    {
        public TabGroup tabGroup;
        public bool isSelected;

        public TabSelection contentPanel;


        #region MonoBehaviour Callbacks

        private void Awake()
        {
            if (tabGroup == null)
            {
                tabGroup = GetComponentInParent<TabGroup>();
            }

            tabGroup?.Subscribe(this);
            Deselect();
        }

        #endregion

        public void Select()
        {
            isSelected = true;
            OnTabSelect();
        }

        public void Deselect()
        {
            isSelected = false;
            OnTabDeselect();
        }
        
        protected virtual void OnTabSelect()
        {
            contentPanel?.OnTabSelect();
        }
        
        protected virtual void OnTabDeselect()
        {
            contentPanel?.OnTabDeselect();
        }
        
        
    }
}
