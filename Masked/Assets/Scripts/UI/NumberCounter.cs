using UnityEngine;
using TMPro;
using PrimeTween;
using System;

/// <summary>
/// Helper class for animating number counters that tick up from 0 to a target value
/// </summary>
public class NumberCounter : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI textField;
    
    [Header("Settings")]
    public float countDuration = 1f;
    public Ease countEase = Ease.OutQuad;
    public string numberFormat = "N0"; // Standard numeric format (e.g., "N0" for integers, "F2" for 2 decimals)
    public bool playTickSound = true; // Whether to play tick sound when counting
    public float tickSoundInterval = 0.05f; // Minimum time between tick sounds to prevent audio spam
    
    private Tween currentTween;
    private int lastDisplayedValue = -1; // Track last displayed value to only play sound on change
    private float lastTickSoundTime = 0f; // Track time of last tick sound
    
    private void OnDestroy()
    {
        // Stop tween when component is destroyed to prevent accessing destroyed objects
        if (currentTween.isAlive)
        {
            currentTween.Stop();
        }
    }
    
    /// <summary>
    /// Animate the number from 0 to targetValue
    /// </summary>
    public void CountTo(int targetValue, Action onComplete = null)
    {
        CountTo(targetValue, countDuration, onComplete);
    }
    
    /// <summary>
    /// Animate the number from 0 to targetValue with custom duration
    /// </summary>
    public void CountTo(int targetValue, float duration, Action onComplete = null)
    {
        if (textField == null)
        {
            Debug.LogWarning("NumberCounter: textField is null!");
            onComplete?.Invoke();
            return;
        }
        
        // Stop any existing tween
        if (currentTween.isAlive)
        {
            currentTween.Stop();
        }
        
        // Start from 0
        int currentValue = 0;
        lastDisplayedValue = 0;
        lastTickSoundTime = Time.unscaledTime;
        textField.text = FormatNumber(currentValue);
        
        // Animate to target
        currentTween = Tween.Custom(0f, 1f, duration, onValueChange: progress =>
        {
            currentValue = Mathf.RoundToInt(Mathf.Lerp(0, targetValue, progress));
            
            // Only update text and play sound if the value changed
            if (currentValue != lastDisplayedValue)
            {
                textField.text = FormatNumber(currentValue);
                
                // Play tick sound if enabled and enough time has passed
                if (playTickSound && SoundEffectManager.Instance != null && 
                    Time.unscaledTime - lastTickSoundTime >= tickSoundInterval)
                {
                    SoundEffectManager.Instance.Play(SoundEffectManager.Instance.soundEffectAtlas.tickSound);
                    lastTickSoundTime = Time.unscaledTime;
                }
                
                lastDisplayedValue = currentValue;
            }
        }, ease: countEase, useUnscaledTime: true).OnComplete(() =>
        {
            // Ensure we end exactly at target value
            textField.text = FormatNumber(targetValue);
            onComplete?.Invoke();
        });
    }
    
    /// <summary>
    /// Set the number immediately without animation
    /// </summary>
    public void SetValue(int value)
    {
        if (currentTween.isAlive)
        {
            currentTween.Stop();
        }
        
        if (textField != null)
        {
            textField.text = FormatNumber(value);
        }
    }
    
    /// <summary>
    /// Set the text directly (for non-numeric values like song titles)
    /// </summary>
    public void SetText(string text)
    {
        if (currentTween.isAlive)
        {
            currentTween.Stop();
        }
        
        if (textField != null)
        {
            textField.text = text;
        }
    }
    
    private string FormatNumber(int value)
    {
        try
        {
            return value.ToString(numberFormat);
        }
        catch
        {
            return value.ToString();
        }
    }
}

