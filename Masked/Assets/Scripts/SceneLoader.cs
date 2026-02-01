using UnityEngine;
using UnityEngine.SceneManagement;
using PrimeTween;
using Extensions.Patterns;

/// <summary>
/// Persistent scene loader that handles transitions between menu and game scenes
/// </summary>
public class SceneLoader : Singleton<SceneLoader>
{
    [Header("Scene Names")]
    [Tooltip("Name of the main menu scene")]
    public string menuSceneName = "MenuScene";
    
    [Tooltip("Name of the game/level scene")]
    public string gameSceneName = "GameScene";
    
    [Header("Transition Settings")]
    public float transitionDuration = 0.5f;
    public CanvasGroup fadeCanvas; // Optional fade overlay
    
    private bool isLoading;
    
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
        
        if (fadeCanvas != null)
        {
            fadeCanvas.gameObject.SetActive(true);
            fadeCanvas.alpha = 0f; // Ensure fade canvas starts transparent
        }
    }
    
    /// <summary>
    /// Load the game scene with the current LivePlayData from GameManager
    /// </summary>
    public void LoadGameScene()
    {
        Debug.Log($"LoadGameScene called. isLoading: {isLoading}");
        
        if (isLoading)
        {
            Debug.LogWarning("Already loading a scene, ignoring request.");
            return;
        }
        
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.Instance is NULL!");
            return;
        }
        
        if (GameManager.Instance.livePlayData == null)
        {
            Debug.LogError("GameManager.livePlayData is NULL!");
            return;
        }
        
        Debug.Log($"LivePlayData found: {GameManager.Instance.livePlayData.selectedBeatMap?.songTitle}");
        
        // Reset time scale before loading
        Time.timeScale = 1f;
        
        // Stop all tweens EXCEPT the ones we're about to create
        Tween.StopAll();
        
        Debug.Log($"Loading game scene: {gameSceneName}");
        LoadSceneAsync(gameSceneName);
    }
    
    /// <summary>
    /// Load the menu scene
    /// </summary>
    public void LoadMenuScene()
    {
        Debug.Log($"LoadMenuScene called. isLoading: {isLoading}");
        
        if (isLoading)
        {
            Debug.LogWarning("Already loading a scene, ignoring request.");
            return;
        }
        
        // Reset time scale before loading
        Time.timeScale = 1f;
        
        // Stop all tweens before loading
        Tween.StopAll();
        
        Debug.Log($"Loading menu scene: {menuSceneName}");
        LoadSceneAsync(menuSceneName);
    }
    
    private void LoadSceneAsync(string sceneName)
    {
        isLoading = true;
        
        // Fade out if canvas available
        if (fadeCanvas != null)
        {
            fadeCanvas.gameObject.SetActive(true);
            fadeCanvas.alpha = 0f; // Reset to 0 first in case tweens were stopped
            fadeCanvas.interactable = false;
            fadeCanvas.blocksRaycasts = true;
            
            // IMPORTANT: Use unscaled time so it works even when Time.timeScale = 0
            Tween.Alpha(fadeCanvas, 1f, transitionDuration, useUnscaledTime: true)
                .OnComplete(() => LoadSceneInternal(sceneName));
        }
        else
        {
            LoadSceneInternal(sceneName);
        }
    }
    
    private void LoadSceneInternal(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        
        if (asyncLoad != null)
        {
            asyncLoad.completed += _ => OnSceneLoaded();
        }
        else
        {
            Debug.LogError($"Failed to load scene: {sceneName}");
            isLoading = false;
        }
    }
    
    private void OnSceneLoaded()
    {
        Debug.Log("Scene loaded, fading in from black...");
        
        // Ensure time scale is reset (safety check)
        Time.timeScale = 1f;
        
        // Fade in if canvas available
        if (fadeCanvas != null)
        {
            fadeCanvas.interactable = false;
            fadeCanvas.blocksRaycasts = false;
            fadeCanvas.alpha = 1f; // Ensure we start at exactly 1
            
            // Use unscaled time for consistency
            Tween.Alpha(fadeCanvas, 0f, transitionDuration, useUnscaledTime: true)
                .OnComplete(() =>
                {
                    fadeCanvas.alpha = 0f; // Force to exactly 0
                    fadeCanvas.gameObject.SetActive(false);
                    isLoading = false;
                    Debug.Log("Fade in complete, canvas disabled.");
                });
        }
        else
        {
            isLoading = false;
        }
    }
    
    /// <summary>
    /// Quit the application
    /// </summary>
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

