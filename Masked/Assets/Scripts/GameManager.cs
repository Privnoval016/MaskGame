using Extensions.Patterns;
using UnityEngine;
using PrimeTween;

public class GameManager : Singleton<GameManager>
{
    [Header("Available Songs")]
    public BeatMapData[] availableSongs;
    
    [Header("Current Play Session")]
    public LivePlayData livePlayData;
    
    [Header("Gameplay Modifiers")]
    [Tooltip("If enabled, AutoPlayer will hit all notes perfectly")]
    public bool autoPlayEnabled;
    
    [Tooltip("If enabled, removes logic gate mechanics - notes pass through directly")]
    public bool simpleModeEnabled;
    
    [Header("Game Over Settings")]
    public float gameOverSlowdownDuration = 1.5f; // Time to slow down to 0
    public float gameOverHoldDuration = 2f; // How long to show game over before returning to menu
    
    [Header("Level Complete Settings")]
    public float levelCompleteHoldDuration = 2f; // How long to show "Level Complete" before stats
    
    private bool isGameOverActive = false; // Prevent multiple game over triggers
    private bool isLevelCompleteActive = false; // Prevent multiple level complete triggers
    
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }
    
    /// <summary>
    /// Set the play data before starting a game session
    /// </summary>
    public void SetPlayData(BeatMapData beatMap, Difficulty difficulty, float playSpeed, LogicOperation[] enabledOperations)
    {
        livePlayData = new LivePlayData(beatMap, difficulty, playSpeed, enabledOperations, autoPlayEnabled, simpleModeEnabled);
        
        // Reset flags for new game instance
        isGameOverActive = false;
        isLevelCompleteActive = false;
        
        Debug.Log($"Play data set for new game. Difficulty: {difficulty}, AutoPlay: {autoPlayEnabled}, SimpleMode: {simpleModeEnabled}, Song: {beatMap.songTitle}");
    }
    
    /// <summary>
    /// Called when player runs out of health - slows down time, fades in game over screen, returns to menu
    /// </summary>
    public void GameOver()
    {
        // Prevent multiple calls
        if (isGameOverActive)
        {
            Debug.LogWarning("GameOver already active, ignoring duplicate call.");
            return;
        }
        
        isGameOverActive = true;
        Debug.Log("Game Over!");
        
        LevelEndManager levelEndManager = LevelEndManager.Instance;
        
        // Disable player input immediately
        PlayerInputDetector inputDetector = PlayerInputDetector.Instance;
        if (inputDetector != null)
        {
            inputDetector.DisableInput();
            Debug.Log("Input disabled");
        }
        
        // Stop all sound effects immediately
        if (SoundEffectManager.Instance != null)
        {
            SoundEffectManager.Instance.StopAll();
            Debug.Log("Sound effects stopped");
        }
        
        // Slow down music with pitch effect
        MusicManager musicManager = MusicManager.Instance;
        if (musicManager != null)
        {
            musicManager.SlowDownMusic(gameOverSlowdownDuration, targetPitch: 0f);
            Debug.Log("Music slowing down");
        }
        
        // Slow down time
        Time.timeScale = 1f;
        Tween.GlobalTimeScale(0f, gameOverSlowdownDuration, Ease.OutQuad).OnComplete(() =>
        {
            // Show game over screen
            if (levelEndManager != null)
            {
                levelEndManager.ShowGameOver();
            }
            
            // Wait, then return to menu
            Tween.Delay(gameOverHoldDuration, useUnscaledTime: true).OnComplete(() =>
            {
                Debug.Log("Game over hold duration complete, returning to menu...");
                
                // IMPORTANT: Reset time scale FIRST before any other operations
                Time.timeScale = 1f;
                
                // DON'T call StopAll here - it will stop the game over fade and cause flickering
                // The scene transition will clean up tweens naturally
                
                if (SceneLoader.Instance != null)
                {
                    Debug.Log("Loading menu scene from game over...");
                    SceneLoader.Instance.LoadMenuScene();
                }
                else
                {
                    Debug.LogError("SceneLoader.Instance is null during game over!");
                }
            });
        });
    }
    
    /// <summary>
    /// Called when player completes the level - shows win screen, displays stats, allows return to menu
    /// </summary>
    public void LevelComplete(int totalScore, int maxCombo, int maxCorrectCombo)
    {
        // Prevent multiple calls
        if (isLevelCompleteActive)
        {
            Debug.LogWarning("LevelComplete already active, ignoring duplicate call.");
            return;
        }
        
        isLevelCompleteActive = true;
        Debug.Log($"GameManager.LevelComplete called! Score: {totalScore}, Max Combo: {maxCombo}, Max Correct Combo: {maxCorrectCombo}");
        
        // Play finished song sound effect
        if (SoundEffectManager.Instance != null)
        {
            SoundEffectManager.Instance.Play(SoundEffectManager.Instance.soundEffectAtlas.finishedSong);
        }
        
        LevelEndManager levelEndManager = LevelEndManager.Instance;
        
        if (levelEndManager != null)
        {
            Debug.Log("LevelEndManager found! Calling ShowLevelComplete...");
            levelEndManager.ShowLevelComplete(totalScore, maxCombo, maxCorrectCombo);
        }
        else
        {
            Debug.LogError("LevelEndManager not found in scene! Cannot show level complete screen.");
        }
    }
}
