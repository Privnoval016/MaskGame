
using UnityEngine;

[CreateAssetMenu(fileName = "ScoreProfile", menuName = "Score/ScoreProfile", order = 1)]
public class ScoreProfile : ScriptableObject
{
    [Header("Score Type")]
    public ScoreType scoreType;

    [Header("UI Parameters")] 
    public string scoreText;

    [Header("Score Parameters")]
    public int correctScoreIncrease; // points awarded for hitting the correct boolean note
    public int incorrectScoreIncrease; // even if you hit the wrong boolean note, you still get some points
    [Tooltip("How much longer the hit window is compared to the previous score type")]
    public float hitWindowDelta = 0.05f;
}

public enum ScoreType
{
    Perfect,
    Great,
    Good,
    Miss
}