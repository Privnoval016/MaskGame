using System.Collections.Generic;
using System.Linq;
using Extensions.EventBus;
using UnityEngine;

/// <summary>
/// Automatically plays the game by hitting all notes perfectly with correct truth values.
/// Mirrors PlayerInputDetector but triggers input programmatically instead of from user input.
/// </summary>
public class AutoPlayer : MonoBehaviour
{
    private BeatMapManager beatMapManager;
    public ScoreManager scoreManager;
    public ScoreProfile perfectScoreProfile;
    
    [Header("AutoPlayer Settings")]
    [SerializeField] private bool enableAutoPlayer = false;
    
    // Track which notes we've already auto-hit to prevent double-hitting
    private HashSet<LogicNote> processedNotes = new HashSet<LogicNote>();
    
    private void Awake()
    {
        beatMapManager = BeatMapManager.Instance;
    }
    
    private void Start()
    {
        // Get autoplay setting from GameManager
        if (GameManager.Instance?.livePlayData != null)
        {
            enableAutoPlayer = GameManager.Instance.livePlayData.autoPlayEnabled;
        }
    }
    
    private void Update()
    {
        if (!enableAutoPlayer || beatMapManager == null || scoreManager == null)
            return;
        
        AutoPlayNotes();
    }
    
    private void AutoPlayNotes()
    {
        float currentBeatStamp = beatMapManager.CurrentBeatStamp;
        
        // Get all active notes
        var activeNotes = GetActiveLogicNotes();
        
        // Group notes by lane and beat
        var notesByLane = activeNotes
            .Where(n => !processedNotes.Contains(n))
            .GroupBy(n => n.laneIndex)
            .ToList();
        
        foreach (var laneGroup in notesByLane)
        {
            int laneIndex = laneGroup.Key;
            
            // Find notes that are at the perfect hit window
            var notesToHit = laneGroup
                .Where(n =>
                {
                    float hitDelta = Mathf.Abs(n.beatStamp - currentBeatStamp) * beatMapManager.BeatDuration;
                    // subtracting by hitWindowDelta to ensure we only consider perfect hits
                    ScoreType scoreType = scoreManager.GetScoreTypeByHitDelta(hitDelta);
                    return scoreType == ScoreType.Perfect; // Only hit at perfect timing
                })
                .ToList();
            
            if (notesToHit.Count > 0)
            {
                // Check if simple mode is enabled
                bool simpleModeEnabled = GameManager.Instance?.livePlayData?.simpleModeEnabled ?? false;
                
                bool expectedResult;
                
                if (simpleModeEnabled)
                {
                    // In simple mode, notes pass through directly - use the real truth value
                    expectedResult = notesToHit[0].realTruthValue == 1;
                }
                else
                {
                    // Normal mode - evaluate the expected truth value using logic gate
                    LogicSplitter splitter = beatMapManager.GetLogicSplitter(beatMapManager.activeLogicOperation);
                    int[] truthValues = notesToHit.Select(n => n.truthValue).ToArray();
                    expectedResult = splitter.EvaluateTruthValue(truthValues);
                }
                
                // Convert to input direction (1 = up, -1 = down)
                int expectedDirection = expectedResult ? 1 : -1;
                Vector2 direction = new Vector2(0, expectedDirection);
                
                // Raise button pressed event
                EventBus<ButtonPressedEvent>.Raise(new ButtonPressedEvent(laneIndex, direction));
                
                // Mark these notes as processed
                foreach (var note in notesToHit)
                {
                    processedNotes.Add(note);
                }
                
                Debug.Log($"AutoPlayer hit lane {laneIndex} with direction {expectedDirection} (expected: {expectedResult})");
            }
        }
        
        // Clean up processed notes that are no longer active
        processedNotes.RemoveWhere(n => n == null || !activeNotes.Contains(n));
    }
    

    private HashSet<LogicNote> GetActiveLogicNotes()
    {
        return BeatMapManager.Instance.activeLogicNotes ?? new HashSet<LogicNote>();
    }
    
    public void SetAutoPlayerEnabled(bool on)
    {
        enableAutoPlayer = on;
        
        if (enableAutoPlayer)
        {
            processedNotes.Clear();
            Debug.Log("AutoPlayer enabled");
        }
        else
        {
            Debug.Log("AutoPlayer disabled");
        }
    }
}

