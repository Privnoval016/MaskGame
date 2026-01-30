using System.Collections.Generic;
using System.Linq;
using Extensions.Timers;
using UnityEngine;

namespace Extensions.UI
{
    public class EffectTileUI : MonoBehaviour
    {
        [SerializeField] private List<EffectIconUI> effectIcons = new List<EffectIconUI>();

        [SerializeField] private float iconScrollInterval = 2f; // Time in seconds before swapping to the next set of icons
    
        private int IconsLength => effectIcons.Count; // Total number of effect icons that can be displayed at once
        private int currentStartIndex = 0; // Index of the first icon currently being displayed
    
        private LockOnTarget target;
    
        private CountdownTimer iconScrollTimer; // Timer to manage icon scrolling

        private void Awake()
        {
            iconScrollTimer = new CountdownTimer(iconScrollInterval, true);
            iconScrollTimer.OnTimerStop += OnIconTimerComplete;
        }

        private void Update()
        {
            RefreshEffectIcons();
        }
    
        public void SetTarget(LockOnTarget newTarget)
        {
            target = newTarget;
            currentStartIndex = 0; // Reset to the beginning when changing targets
            RefreshEffectIcons();
            iconScrollTimer.Reset();
        }
    
        private void OnIconTimerComplete()
        {
            if (target == null) return;

            var statusEffects = target.damageable.Stats.StatusEffects().ToList();
            int totalEffects = statusEffects.Count;

            if (totalEffects <= IconsLength)
            {
                // No need to scroll if all effects fit within the available icons
                return;
            }

            // Update the starting index for the next set of icons
            currentStartIndex += IconsLength;
            if (currentStartIndex >= totalEffects)
            {
                currentStartIndex = 0; // Loop back to the beginning
            }
        
            RefreshEffectIcons();
        
            // Restart the timer for the next scroll
            iconScrollTimer.Reset();
        }

        private void RefreshEffectIcons()
        {
            if (target == null) return;

            var activeEffects = target.damageable.Stats
                .StatusEffects()
                .Where(kvp => kvp.Value > 0)
                .ToList();

            int start = currentStartIndex;
            int count = activeEffects.Count;

            for (int i = 0; i < IconsLength; i++)
            {
                int effectIdx = start + i;

                if (effectIdx < count)
                {
                    var effect = activeEffects[effectIdx];
                    effectIcons[i].SetEffect(effect.Key, effect.Value);
                }
                else
                {
                    effectIcons[i].ClearEffect();
                }
            }
        }

    }
}