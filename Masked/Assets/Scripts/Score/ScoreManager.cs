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
        ScoreType scoreType = e.scoreType;
        ScoreProfile profile = Array.Find(scoreProfiles, sp => sp.scoreType == scoreType);

        if (profile == null)
        {
            Debug.LogWarning("Score Profile not found");
            return;
        }
        
        // update by half because each hit is for 2 notes
        if (e.isCorrect == true)
        {
            totalScore += profile.correctScoreIncrease * 0.5f;
            allCombo += 0.5f;
            correctCombo += 0.5f;
        }
        else if (e.isCorrect == false)
        {
            totalScore += profile.incorrectScoreIncrease * 0.5f;
            allCombo += 0.5f;
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