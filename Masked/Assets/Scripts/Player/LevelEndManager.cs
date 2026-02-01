using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PrimeTween;
using System.Collections;
using Extensions.Patterns;

/// <summary>
/// Handles all end-of-level UI logic including game over and level complete screens
/// </summary>
public class LevelEndManager : Singleton<LevelEndManager>
{
    [Header("Game Over")]
    public CanvasGroup gameOverCanvas;
    
    [Header("Level Complete")]
    public CanvasGroup levelCompleteCanvas;
    public GameObject statsWindow; // The stats window that opens like a computer window
    public NumberCounter songTitleCounter; // Displays song title (uses SetText)
    public NumberCounter totalScoreCounter; // Displays total score
    public NumberCounter maxComboCounter; // Displays max all combo
    public NumberCounter maxCorrectComboCounter; // Displays max correct combo
    public Button returnToMenuButton;
    
    [Header("Animation Settings")]
    public float gameOverFadeDuration = 1f; // Duration to fade in game over screen
    public float levelCompleteFadeDuration = 1f; // Duration to fade in level complete text
    public float windowOpenDuration = 0.5f;
    public Ease windowOpenEase = Ease.OutBack;
    public float statCountDuration = 1f; // Duration for each stat to count up
    public float delayBetweenStats = 0.3f; // Delay between each stat animation
    
    private void Start()
    {
        // Hide all UI initially
        if (gameOverCanvas != null)
        {
            gameOverCanvas.alpha = 0f;
            gameOverCanvas.gameObject.SetActive(false);
        }
        
        if (levelCompleteCanvas != null)
        {
            levelCompleteCanvas.alpha = 0f;
            levelCompleteCanvas.gameObject.SetActive(false);
        }
        
        if (statsWindow != null)
        {
            statsWindow.transform.localScale = Vector3.zero;
            statsWindow.SetActive(false);
        }
        
        // Setup return button
        if (returnToMenuButton != null)
        {
            returnToMenuButton.onClick.AddListener(OnReturnToMenu);
        }
    }
    
    /// <summary>
    /// Show game over screen with fade in
    /// </summary>
    public void ShowGameOver()
    {
        if (gameOverCanvas == null) return;
        
        SoundEffectManager.Instance.Play(SoundEffectManager.Instance.soundEffectAtlas.gameOver);
        
        gameOverCanvas.gameObject.SetActive(true);
        gameOverCanvas.interactable = true; // Allow button clicks
        gameOverCanvas.blocksRaycasts = true;
        gameOverCanvas.alpha = 1;
    }
    
    /// <summary>
    /// Show level complete screen with stats
    /// </summary>
    public void ShowLevelComplete(int totalScore, int maxCombo, int maxCorrectCombo)
    {
        Debug.Log($"LevelEndManager.ShowLevelComplete called! Score: {totalScore}, MaxCombo: {maxCombo}, MaxCorrectCombo: {maxCorrectCombo}");
        
        if (levelCompleteCanvas == null)
        {
            Debug.LogError("levelCompleteCanvas is NULL!");
            return;
        }
        
        Debug.Log("Activating level complete canvas...");
        
        // Activate and set up canvas
        levelCompleteCanvas.gameObject.SetActive(true);
        levelCompleteCanvas.interactable = true; // Allow button clicks
        levelCompleteCanvas.blocksRaycasts = true;
        levelCompleteCanvas.alpha = 0f;
        
        Debug.Log($"Canvas active: {levelCompleteCanvas.gameObject.activeSelf}, Alpha: {levelCompleteCanvas.alpha}");
        
        // Fade in the "Level Complete" text
        Debug.Log($"Starting fade tween with duration: {levelCompleteFadeDuration}");
        Tween.Alpha(levelCompleteCanvas, 1f, levelCompleteFadeDuration, Ease.InOutQuad)
            .OnComplete(() =>
            {
                Debug.Log($"Fade complete! Alpha is now: {levelCompleteCanvas.alpha}. Waiting {GameManager.Instance.levelCompleteHoldDuration}s before showing stats...");
                
                // Hold for configured duration, then show stats
                Tween.Delay(GameManager.Instance.levelCompleteHoldDuration).OnComplete(() =>
                {
                    Debug.Log("Delay complete! Showing stats window...");
                    ShowStatsWindow(totalScore, maxCombo, maxCorrectCombo);
                });
            });
    }
    
    private void ShowStatsWindow(int totalScore, int maxCombo, int maxCorrectCombo)
    {
        Debug.Log($"ShowStatsWindow called! Stats: {totalScore}, {maxCombo}, {maxCorrectCombo}");
        
        if (statsWindow == null)
        {
            Debug.LogError("statsWindow is NULL!");
            return;
        }
        
        Debug.Log("Setting up stats window...");
        
        statsWindow.SetActive(true);
        statsWindow.transform.localScale = Vector3.zero;
        
        // Get song title from GameManager
        string songTitle = "Unknown Song";
        if (GameManager.Instance?.livePlayData?.selectedBeatMap != null)
        {
            songTitle = GameManager.Instance.livePlayData.selectedBeatMap.songTitle;
        }
        
        Debug.Log($"Song title: {songTitle}");
        
        // Set song title immediately (no animation)
        if (songTitleCounter != null)
        {
            songTitleCounter.SetText(songTitle);
        }
        else
        {
            Debug.LogWarning("songTitleCounter is null!");
        }
        
        // Animate window opening like a computer window
        Debug.Log($"Animating window scale from 0 to 1 over {windowOpenDuration}s");
        Tween.Scale(statsWindow.transform, Vector3.one, windowOpenDuration, windowOpenEase)
            .OnComplete(() =>
            {
                Debug.Log("Stats window opened! Starting stat animations...");
                // Sequentially animate stats: Score -> Max Combo -> Max Correct Combo
                AnimateStatsSequentially(totalScore, maxCombo, maxCorrectCombo);
            });
    }
    
    private void AnimateStatsSequentially(int totalScore, int maxCombo, int maxCorrectCombo)
    {
        // Animate total score first
        if (totalScoreCounter != null)
        {
            totalScoreCounter.CountTo(totalScore, statCountDuration, () =>
            {
                // After score finishes, wait a bit then animate max combo
                Tween.Delay(delayBetweenStats).OnComplete(() =>
                {
                    if (maxComboCounter != null)
                    {
                        maxComboCounter.CountTo(maxCombo, statCountDuration, () =>
                        {
                            // After max combo finishes, wait a bit then animate max correct combo
                            Tween.Delay(delayBetweenStats).OnComplete(() =>
                            {
                                if (maxCorrectComboCounter != null)
                                {
                                    maxCorrectComboCounter.CountTo(maxCorrectCombo, statCountDuration);
                                }
                            });
                        });
                    }
                });
            });
        }
        else
        {
            // Fallback if totalScoreCounter is missing
            Debug.LogWarning("LevelEndManager: totalScoreCounter is null!");
        }
    }
    
    private void OnReturnToMenu()
    {
        Debug.Log("Return to Menu button clicked!");
        
        // Stop all tweens including number counters
        Tween.StopAll();
        
        // Reset time scale in case it was altered
        Time.timeScale = 1f;
        
        if (SceneLoader.Instance != null)
        {
            Debug.Log("Loading menu scene...");
            SceneLoader.Instance.LoadMenuScene();
        }
        else
        {
            Debug.LogError("SceneLoader.Instance is null!");
        }
    }
}

