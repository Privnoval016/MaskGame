using System;
using Extensions.EventBus;
using Extensions.UI;
using TMPro;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    [Header("Score Settings")]
    public SliderBar scoreSlider;
    public float scoreTweenSpeed = 0.05f;
    
    [Header("Combo Settings")]
    public TMP_Text allComboText;
    public TMP_Text correctComboText;

    private EventBinding<ScoreChangedEvent> scoreChangedBinding;

    private void Awake()
    {
        scoreChangedBinding = new EventBinding<ScoreChangedEvent>(OnScoreChanged);
        EventBus<ScoreChangedEvent>.Register(scoreChangedBinding);
    }

    private void OnDisable()
    {
        EventBus<ScoreChangedEvent>.Deregister(scoreChangedBinding);
    }

    private void OnScoreChanged(ScoreChangedEvent e)
    {
        scoreSlider.TweenSliderValue(e.newScore / e.maxScore, scoreTweenSpeed);

        int dir = (int.Parse(allComboText.text) < e.allCombo) ? 1 : -1;
        allComboText.text = $"{e.allCombo}";

        allComboText.gameObject.PulseOutIn(dir * 1.1f, 0.05f, 0f, 0.05f);
        
        dir = int.Parse(correctComboText.text) < e.correctCombo ? 1 : -1;
        correctComboText.text = $"{e.correctCombo}";
        
        correctComboText.gameObject.PulseOutIn(dir * 1.1f, 0.05f, 0f, 0.05f);

    }
}

public struct ScoreChangedEvent : IEvent
{
    public float newScore;
    public float maxScore;
    public int allCombo;
    public int correctCombo;

    public ScoreChangedEvent(float newScore, float allCombo, float correctCombo, float maxScore)
    {
        this.newScore = newScore;
        this.allCombo = Mathf.RoundToInt(allCombo);
        this.correctCombo = Mathf.RoundToInt(correctCombo);
        this.maxScore = maxScore;
    }
}
