using Extensions.EventBus;
using PrimeTween;
using UnityEngine;

public class HealthManager : MonoBehaviour
{
    public int maxHealth = 10;
    public int currentHealth;
    public float gameOverDuration = 1.5f;
    public float invincibilityDuration = 0.25f; // quarter beat invincibility
    private float lastDamageBeatStamp = -Mathf.Infinity;

    public EventBinding<ScoreChangedEvent> scoreChangedBinding;

    private void Awake()
    {
        currentHealth = maxHealth;
        
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
        
        ModifyHealth(-1);
    }

    public void ModifyHealth(int amount)
    {
        currentHealth += amount;
        
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        else if (currentHealth < 0)
        {
            GameOver();
        }
        
        EventBus<HealthChangedEvent>.Raise(new HealthChangedEvent(currentHealth, maxHealth));
    }
    
    private void GameOver()
    {
        Debug.Log("Game Over!");

        Time.timeScale = 1f;
        Tween.GlobalTimeScale(0f, gameOverDuration, Ease.OutQuad).OnComplete(() =>
        {
            // Implement game over logic here (e.g., load game over screen)
        });
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