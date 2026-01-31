
using UnityEngine;

namespace Extensions.UI
{
    public class RadialMenu<T>
    {
        #region Properties
        public int numberOfOptions;
        public Vector2 zeroDirection;
        
        public RadialMenuOption<T>[] options;
        
        #endregion
        
        #region Selections
        
        public bool isMenuOpen = false;

        public RadialMenuOption<T> currentOption;

        public RadialMenuOption<T> selectedOption;
        
        #endregion
        
        #region Constructors
        
        public RadialMenu(int numberOfOptions, Vector2 zeroDirection = default)
        {
            this.numberOfOptions = numberOfOptions;
            options = new RadialMenuOption<T>[numberOfOptions];
            for (int i = 0; i < numberOfOptions; i++)
            {
                options[i] = new RadialMenuOption<T> { index = i };
            }
            
            if (zeroDirection == default)
            {
                zeroDirection = Vector2.up;
            }
            this.zeroDirection = zeroDirection.normalized;
        }
        
        public RadialMenu(RadialMenuOption<T>[] options, Vector2 zeroDirection = default)
        {
            this.options = options;
            numberOfOptions = options.Length;
            this.zeroDirection = zeroDirection == default ? Vector2.up : zeroDirection.normalized;
        }
        
        #endregion
        
        #region Input Detection
        
        public void OnElementMenuOpen(RadialMenuOption<T> current)
        {
            isMenuOpen = true;
            currentOption = current;
        }
        
        public RadialMenuOption<T> OnElementMenuClose()
        {
            isMenuOpen = false;
            RadialMenuOption<T> selected = selectedOption;
            selectedOption = null;
            currentOption = selected;
            return selected;
        }

        public RadialMenuOption<T> UpdateMenu(Vector2 input)
        {
            if (!isMenuOpen) return null;
            
            selectedOption = GetOptionFromInput(input, selectedOption);
            return selectedOption;
        }
        
        #endregion
        
        #region Accessors
        
        public RadialMenuOption<T> GetOption(int index)
        {
            if (index < 0 || index >= numberOfOptions)
            {
                Debug.LogError("Index out of bounds for RadialMenu options.");
                return null;
            }
            return options[index];
        }
        
        public RadialMenuOption<T> GetOptionFromInput(Vector2 input, RadialMenuOption<T> defaultOption)
        {
            if (numberOfOptions <= 0)
            {
                Debug.LogError("No options available in RadialMenu.");
                return null;
            }
            
            // Normalize input direction
            Vector2 direction = input.normalized;
            if (direction == Vector2.zero) return defaultOption;
            
            // Calculate angle from zeroDirection
            float angle = Vector2.SignedAngle(zeroDirection, direction);
            float anglePerOption = 360f / numberOfOptions;
            
            // Determine which option corresponds to the angle
            int optionIndex = Mathf.RoundToInt((angle + 180f) / anglePerOption) % numberOfOptions;
            return GetOption(optionIndex);
        }
        
        #endregion
        
        
    }

    public class RadialMenuOption<T>
    {
        public int index; // counted from top right most option
        public T data; // data associated with this option
        
        public RadialMenuOption(int index = 0, T data = default)
        {
            this.index = index;
            this.data = data;
        }
    }
}
