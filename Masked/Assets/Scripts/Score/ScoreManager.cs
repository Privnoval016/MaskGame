using System;
using Extensions.EventBus;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public ScoreProfile[] scoreProfiles;
    
    [Header("Combos")]
    public float comboMultiplier = 1f;
    public Vector2 incrementPerCombo = new Vector2(0.1f, 50); // (increment, combo interval to apply)
    public float maxComboMultiplier = 2f;
    public float maxScore = 1000000f;
    
    [Header("Score Tracking")]
    public float totalScore = 0;
    public float correctCombo = 0;
    public float allCombo = 0;
    
    private EventBinding<LogicNoteHitEvent> hitEventBinding;

    private void Awake()
    {
        hitEventBinding = new EventBinding<LogicNoteHitEvent>(OnNoteHit);
        EventBus<LogicNoteHitEvent>.Register(hitEventBinding);
    }
    
    private void OnDestroy()
    {
        EventBus<LogicNoteHitEvent>.Deregister(hitEventBinding);
    }

    public ScoreType GetScoreTypeByHitDelta(float hitDelta)
    {
        // sort by the enum where the smallest hit window is index 0
        Array.Sort(scoreProfiles, (a, b) => a.hitWindowDelta.CompareTo(b.hitWindowDelta));

        float accumulatedWindow = 0f;
        foreach (var profile in scoreProfiles)
        {
            accumulatedWindow += profile.hitWindowDelta;
            if (hitDelta <= accumulatedWindow)
            {
                return profile.scoreType;
            }
        }
        return ScoreType.Miss; // Default to Miss if no other type matches
    }
    
    private void OnNoteHit(LogicNoteHitEvent e)
    {
        if (e.hitNote.spawnLocationIndex != 0) return; // only count hits from the first row to avoid double counting
        
        ScoreType scoreType = e.scoreType;
        ScoreProfile profile = Array.Find(scoreProfiles, sp => sp.scoreType == scoreType);

        if (e.isCorrect == true)
        {
            SoundEffectManager.Instance.Play(e.actualValue == 1 ? SoundEffectManager.Instance.soundEffectAtlas.correctHitOne : 
                SoundEffectManager.Instance.soundEffectAtlas.correctHitZero);
        }
        else if (e.isCorrect == false)
        {
            SoundEffectManager.Instance.Play(SoundEffectManager.Instance.soundEffectAtlas.incorrectHit);
        }
        else
        {
            SoundEffectManager.Instance.Play(SoundEffectManager.Instance.soundEffectAtlas.missHit);
        }
        
        
        if (profile == null)
        {
            Debug.LogWarning("Score Profile not found");
            return;
        }
        
        if (e.isCorrect == true)
        {
            totalScore += profile.correctScoreIncrease;
            allCombo++;
            correctCombo++;
        }
        else if (e.isCorrect == false)
        {
            totalScore += profile.incorrectScoreIncrease;
            allCombo++;
            correctCombo = 0; // reset correct combo on incorrect hit
        }
        else
        {
            correctCombo = 0; // reset correct combo on miss
            allCombo = 0; // reset all combo on miss
        }
        
        // Update combo multiplier
        comboMultiplier = 1f + Mathf.Min(maxComboMultiplier - 1f, Mathf.Floor(allCombo / incrementPerCombo.y) * incrementPerCombo.x);
        
        EventBus<ScoreChangedEvent>.Raise(new ScoreChangedEvent(totalScore, allCombo, correctCombo, maxScore));
    }
}