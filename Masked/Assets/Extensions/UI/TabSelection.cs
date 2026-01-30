using System;
using UnityEngine;

namespace Extensions.UI
{
    public class TabSelection : MonoBehaviour
    {
        private void Awake()
        {
            gameObject.SetActive(false);
        }

        public virtual void OnTabSelect() // called when the tab is selected
        {
            gameObject.SetActive(true);
        }

        public virtual void OnTabDeselect() // called when the tab is deselected
        {
            gameObject.SetActive(false);
        }
    }
}