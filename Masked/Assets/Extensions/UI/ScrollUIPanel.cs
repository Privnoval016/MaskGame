using System;
using UnityEngine;

namespace Extensions.UI
{
    public abstract class ScrollUIPanel : MonoBehaviour
    {
        public RectTransform rectTransform;
        private void Awake()
        {
            rectTransform ??= GetComponent<RectTransform>();
        }

        public abstract void OnSelected(); // called when the panel is hovered over
        
        public abstract void OnDeselected(); // called when the panel is no longer hovered over
        
        public abstract void Refresh(ItemUIInfo info); // called to update the panel with new info
    }
}