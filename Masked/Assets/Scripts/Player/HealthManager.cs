using Extensions.EventBus;
using PrimeTween;
using UnityEngine;

public class HealthManager : MonoBehaviour
{
    public bool isInvincible = false;
    public int maxHealth = 10;
    public int currentHealth;
    public float invincibilityDuration = 0.25f; // quarter beat invincibility
    private float lastDamageBeatStamp = -Mathf.Infinity;
    private bool gameOverTriggered = false; // Prevent multiple game over calls

    public EventBinding<ScoreChangedEvent> scoreChangedBinding;

    private void Awake()
    {
        currentHealth = maxHealth;
        gameOverTriggered = false; // Reset for new game instance
        
        scoreChangedBinding = new EventBinding<ScoreChangedEvent>(OnScoreChanged);
        EventBus<ScoreChangedEvent>.Register(scoreChangedBinding);
        
        Time.timeScale = 1f;
    }
    
    private void OnDestroy()
    {
        EventBus<ScoreChangedEvent>.Deregister(scoreChangedBinding);
    }

    private void OnScoreChanged(ScoreChangedEvent e)
    {
        if (e.scoreProfile.scoreType != ScoreType.Miss) return;
        
        if (BeatMapManager.Instance.CurrentBeatStamp - lastDamageBeatStamp < invincibilityDuration)
        {
            return; // Still invincible
        }
        
        lastDamageBeatStamp = BeatMapManager.Instance.CurrentBeatStamp;
        
        if (isInvincible) return;
        ModifyHealth(-1);
    }

    public void ModifyHealth(int amount)
    {
        currentHealth += amount;
        
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        else if (currentHealth <= 0 && !gameOverTriggered)
        {
            gameOverTriggered = true;
            GameOver();
        }
        
        EventBus<HealthChangedEvent>.Raise(new HealthChangedEvent(currentHealth, maxHealth));
    }
    
    private void GameOver()
    {
        Debug.Log("HealthManager.GameOver() called");
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
        else
        {
            Debug.LogError("GameManager not found! Cannot trigger game over.");
        }
    }
}

public struct HealthChangedEvent : IEvent
{
    public int currentHealth;
    public int maxHealth;

    public HealthChangedEvent(int currentHealth, int maxHealth)
    {
        this.currentHealth = currentHealth;
        this.maxHealth = maxHealth;
    }
}