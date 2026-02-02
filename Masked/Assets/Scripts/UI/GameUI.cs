using System;
using Extensions.EventBus;
using Extensions.UI;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [Header("Score Settings")]
    public SliderBar scoreSlider;
    public float scoreTweenSpeed = 0.05f;
    
    [Header("Combo Settings")]
    public RectTransform[] comboTransforms;
    public TMP_Text allComboText;
    public TMP_Text correctComboText;
    
    [Header("Accuracy Indicator")]
    public TMP_Text indicatorText;
    public RectTransform indicatorTransform;
    public Vector2 indicatorMoveAmount = new Vector2(0f, 30f);
    public float indicatorTweenTime = 0.2f;
    private Vector2 indicatorInitialPosition;
    
    [Header("Health Settings")]
    public RectTransform healthBarTransform;
    public RectTransform healthBarSegmentTransform;

    [Header("Logic Settings")] 
    public TMP_Text[] operatorImages;
    private RectTransform[] operatorTransforms;
    private LogicOperation currentOperator = LogicOperation.Xnor;

    private Image[] healthBarSegments;
    private RectTransform[] healthBarSegmentTransforms;
    private int currentHealthSegmentsActive;
    public float healthSegmentDeactivateTime = 0.3f;
    private Vector3 healthSegmentInitialScale = Vector3.one;
    
    private EventBinding<ScoreChangedEvent> scoreChangedBinding;
    private EventBinding<HealthChangedEvent> healthChangedBinding;
    private EventBinding<CheckpointReachedEvent> checkpointReachedBinding;

    private void Awake()
    {
        scoreChangedBinding = new EventBinding<ScoreChangedEvent>(OnScoreChanged);
        EventBus<ScoreChangedEvent>.Register(scoreChangedBinding);
        
        healthChangedBinding = new EventBinding<HealthChangedEvent>(OnHealthChanged);
        EventBus<HealthChangedEvent>.Register(healthChangedBinding);
        
        checkpointReachedBinding = new EventBinding<CheckpointReachedEvent>(OnCheckpointReached);
        EventBus<CheckpointReachedEvent>.Register(checkpointReachedBinding);
        
        indicatorText.gameObject.SetActive(false);
        indicatorInitialPosition = indicatorTransform.anchoredPosition;
        
        healthBarSegments = healthBarSegmentTransform.GetComponentsInChildren<Image>();
        healthBarSegmentTransforms = new RectTransform[healthBarSegments.Length];
        for (int i = 0; i < healthBarSegments.Length; i++)
        {
            healthBarSegmentTransforms[i] = healthBarSegments[i].GetComponent<RectTransform>();
        }
        currentHealthSegmentsActive = healthBarSegments.Length;
        healthSegmentInitialScale = healthBarSegmentTransforms[0].localScale;
        
    }
    
    private void Start()
    {
        // Initialize the operation UI with the starting operation
        InitializeOperationUI();
    }

    private void OnDisable()
    {
        EventBus<ScoreChangedEvent>.Deregister(scoreChangedBinding);
        EventBus<HealthChangedEvent>.Deregister(healthChangedBinding);
        EventBus<CheckpointReachedEvent>.Deregister(checkpointReachedBinding);
    }

    private void Update()
    {
        // pulse combo transforms on the beat

        float beatProgress = BeatMapManager.Instance.CurrentBeatStamp;
        
        if (beatProgress < 0f) return;
        
        foreach (var comboTransform in comboTransforms)
        {
            float nearestBeat = Mathf.Round(beatProgress);
            float distanceToBeat = Mathf.Abs(beatProgress - nearestBeat);
            float pulseThreshold = 0.1f; // adjust this value to change sensitivity
            if (distanceToBeat < pulseThreshold)
            {
                Func<float, float, float> pulseFunction = (t, maxScale) =>
                {
                    float normalizedTime = t / pulseThreshold;
                    return Mathf.Lerp(maxScale, 1f, normalizedTime);
                };
                
                float pulseAmount = pulseFunction(distanceToBeat, 1.1f);
                
                comboTransform.localScale = Vector3.Lerp(comboTransform.localScale, Vector3.one * pulseAmount, Time.deltaTime * 10f);
            }
            else
            {
                comboTransform.localScale = Vector3.Lerp(comboTransform.localScale, Vector3.one, Time.deltaTime * 10f);
            }
        }
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

        indicatorText.text = e.scoreProfile.scoreText;
        
        Tween.StopAll(indicatorTransform);

        dir = e.scoreProfile.scoreType switch
        {
            ScoreType.Perfect => 1,
            ScoreType.Great => 1,
            ScoreType.Good => 0,
            ScoreType.Miss => -1,
            _ => 0
        };
        
        indicatorText.gameObject.SetActive(true);
        indicatorTransform.anchoredPosition = indicatorInitialPosition;
        indicatorText.alpha = 1;
        indicatorText.color = e.color;
        Tween.UIAnchoredPosition(indicatorTransform,
            indicatorInitialPosition + new Vector2(0f, dir * indicatorMoveAmount.y),
            indicatorTweenTime,
            Ease.OutQuad);
        Tween.Alpha(indicatorText, 0f, indicatorTweenTime, Ease.OutQuad);
    }
    
    private void OnHealthChanged(HealthChangedEvent e)
    {
        if (e.currentHealth < 0 || e.currentHealth > e.maxHealth)
            return;
        
        if (e.currentHealth < currentHealthSegmentsActive)
        {
            for (int i = currentHealthSegmentsActive - 1; i >= e.currentHealth; i--)
            {
                Deactivate(healthBarSegments[i], healthBarSegmentTransforms[i]);
                currentHealthSegmentsActive--;
            }
            
            SoundEffectManager.Instance.Play(SoundEffectManager.Instance.soundEffectAtlas.missHit);
        }
        else if (e.currentHealth > currentHealthSegmentsActive)
        {
            for (int i = currentHealthSegmentsActive; i < e.currentHealth; i++)
            {
                healthBarSegments[i].enabled = true;
                currentHealthSegmentsActive++;
            }
        }
    }
    
    private void Deactivate(Image segment, RectTransform segmentTransform)
    {
        // grow and fade out

        segment.enabled = true;
        Sequence.Create(1, Sequence.SequenceCycleMode.Restart, Ease.Linear, true)
            .Group(Tween.Scale(segmentTransform, healthSegmentInitialScale * 1.5f, healthSegmentDeactivateTime, Ease.InQuad))
            .Group(Tween.Alpha(segment, 0f, healthSegmentDeactivateTime, Ease.InQuad)// next, shake the healthbar
            .Group(Tween.ShakeLocalPosition(healthBarTransform, new Vector3(5f, 0f, 0f), healthSegmentDeactivateTime, 10)))
            .OnComplete(() =>
            {
                segment.enabled = false;
                segmentTransform.localScale = healthSegmentInitialScale;
                segment.color = Color.white;
            });
    }
    
    private void OnCheckpointReached(CheckpointReachedEvent e)
    {
        if (e.newOperation == currentOperator) return;
        
        currentOperator = e.newOperation;
        UpdateOperationUI(true); // Update with animation
    }
    
    /// <summary>
    /// Initialize the operation UI at game start
    /// </summary>
    private void InitializeOperationUI()
    {
        currentOperator = BeatMapManager.Instance.activeLogicOperation;
        UpdateOperationUI(false); // Update without animation
    }
    
    /// <summary>
    /// Update the operation UI display
    /// </summary>
    private void UpdateOperationUI(bool animate)
    {
        foreach (var img in operatorImages)
        {
            img.text = !GameManager.Instance.livePlayData.simpleModeEnabled
                ? BeatMapManager.Instance.materialColorShifter.GetCurrentLogicData().displayName
                : "?!";
            
            if (animate)
            {
                Tween.Color(img, BeatMapManager.Instance.materialColorShifter.GetCurrentLogicData().baseColor, 0.15f, Ease.InCubic);
                img.gameObject.PulseOutIn(1.1f, 0.05f, 0f, 0.05f);
            }
            else
            {
                img.color = BeatMapManager.Instance.materialColorShifter.GetCurrentLogicData().baseColor;
            }
        }
    }
}

public struct ScoreChangedEvent : IEvent
{
    public float newScore;
    public float maxScore;
    public int allCombo;
    public int correctCombo;
    public ScoreProfile scoreProfile;
    public Color color;

    public ScoreChangedEvent(float newScore, float maxScore, float allCombo, float correctCombo, ScoreProfile scoreProfile, Color color)
    {
        this.newScore = newScore;
        this.maxScore = maxScore;
        this.allCombo = Mathf.RoundToInt(allCombo);
        this.correctCombo = Mathf.RoundToInt(correctCombo);
        this.scoreProfile = scoreProfile;
        this.color = color;
    }
}
