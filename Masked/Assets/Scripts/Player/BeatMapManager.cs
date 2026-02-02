using System;
using System.Collections.Generic;
using System.Linq;
using Extensions.EventBus;
using Extensions.Patterns;
using UnityEngine;

public class BeatMapManager : Singleton<BeatMapManager>
{
    private float dspStartTime;
    public float SongTimer => (float)(AudioSettings.dspTime - dspStartTime);
    
    [Header("Startup Settings")]
    public BeatMapData beatMapData;
    public MusicManager musicManager;
    public ScoreManager scoreManager;
    public float startupDelay = 2f; // Delay before the first beat spawns, to give player time to prepare and account for initial note travel time
    public float songBufferInSeconds = 3f; // Extra time after the last beat before considering the song ended.
    public float BeatDuration => 60f / beatMapData.bpm; // Duration of a single beat in seconds.
    
    private bool musicBegan = false;
    private bool songEnded = false; // Flag to ensure song end only triggers once
    
    public int numberOfLanes = 4; // Total number of lanes available (valid note indices: 0 to numberOfLanes - 1)
    public int numberOfSpawnLocations = 2; // Number of different spawn locations for notes (right now top and bottom)
    
    public float CurrentBeatStamp => SongTimer / BeatDuration;

    [SerializeField] private float beatStamp; // only for debugging in inspector

    [Header("Runtime Values")] public LogicOperation activeLogicOperation = LogicOperation.Or;
    
    private EventBinding<ButtonPressedEvent> buttonPressedBinding;
    
    public Dictionary<LogicOperation, LogicSplitter> logicSplitters = new Dictionary<LogicOperation, LogicSplitter>();
    
    // store all active logic notes so we can check for hits by the player and deal with score accordingly
    public readonly HashSet<LogicNote> activeLogicNotes = new HashSet<LogicNote>();
    
    [HideInInspector] public MaterialColorShifter materialColorShifter;
    
    protected override void Awake()
    {
        base.Awake();
        
        // Use beatmap from LivePlayData if available
        if (GameManager.Instance?.livePlayData?.selectedBeatMap != null)
        {
            beatMapData = GameManager.Instance.livePlayData.selectedBeatMap;
        }
        
        materialColorShifter = GetComponent<MaterialColorShifter>();
        dspStartTime = (float)AudioSettings.dspTime + startupDelay; // set the dsp start time to be startupDelay seconds in the future
        musicManager.PlayMusic(beatMapData.clip, dspStartTime);
        
        buttonPressedBinding = new EventBinding<ButtonPressedEvent>(OnButtonPressed);
        EventBus<ButtonPressedEvent>.Register(buttonPressedBinding);
        
        InitializeLogicSplitters();
        PickRandomOperation(); // Pick initial operation from enabled operations
    }

    private void Start()
    {
        materialColorShifter.ShiftColors(activeLogicOperation);
    }
    
    /// <summary>
    /// Picks a random logic operation from the enabled operations in LivePlayData
    /// </summary>
    public void PickRandomOperation()
    {
        if (GameManager.Instance?.livePlayData?.enabledOperations != null &&
            GameManager.Instance.livePlayData.enabledOperations.Length > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, GameManager.Instance.livePlayData.enabledOperations.Length);
            activeLogicOperation = GameManager.Instance.livePlayData.enabledOperations[randomIndex];
        }
        else
        {
            // Fallback to OR if no operations are set
            activeLogicOperation = LogicOperation.Or;
        }
    }

    private void OnDisable()
    {
        EventBus<ButtonPressedEvent>.Deregister(buttonPressedBinding);
    }

    private void Update()
    {
        beatStamp = CurrentBeatStamp;
        
        if (!musicBegan && SongTimer >= 0f)
        {
            musicBegan = true;
            Debug.Log("Music began!");
        }
        
        // Check if song has ended - only trigger once
        if (musicBegan && !songEnded && CurrentBeatStamp >= beatMapData.beats + (songBufferInSeconds / BeatDuration))
        {
            songEnded = true;
            Debug.Log($"Song end condition met! CurrentBeatStamp: {CurrentBeatStamp} >= beatMapData.beats: {beatMapData.beats}");
            OnSongEnd();
        }
    }
    
    private void OnSongEnd()
    {
        Debug.Log($"OnSongEnd called! Stats will be shown now.");
        
        // Get stats from ScoreManager
        int totalScore = Mathf.RoundToInt(scoreManager.totalScore);
        int maxAllCombo = Mathf.RoundToInt(scoreManager.maxAllCombo);
        int maxCorrectCombo = Mathf.RoundToInt(scoreManager.maxCorrectCombo);
        
        Debug.Log($"Song ended! Stats - Score: {totalScore}, MaxCombo: {maxAllCombo}, MaxCorrectCombo: {maxCorrectCombo}");
        
        // Call GameManager to show level complete
        if (GameManager.Instance != null)
        {
            Debug.Log("Calling GameManager.LevelComplete...");
            GameManager.Instance.LevelComplete(totalScore, maxAllCombo, maxCorrectCombo);
        }
        else
        {
            Debug.LogError("GameManager not found! Cannot trigger level complete.");
        }
    }

    private void OnButtonPressed(ButtonPressedEvent e)
    {
        List<LogicNote> notesOnBeat = new List<LogicNote>();
        foreach (var note in activeLogicNotes)
        {
            if (note.laneIndex != e.buttonIndex) continue; // Only consider notes in the lane where the button was pressed
            
            // Check if the note is within the hit window, multiplied by BeatDuration to convert from beats to seconds
            ScoreType noteHitWindow = scoreManager.GetScoreTypeByHitDelta(Mathf.Abs(note.beatStamp - CurrentBeatStamp) * BeatDuration);
            if (noteHitWindow != ScoreType.Miss)
            {
                notesOnBeat.Add(note);
            }
        }
        
        if (notesOnBeat.Count == 0)
        {
            // No notes to evaluate
            return;
        }
        
        // Check if simple mode is enabled
        bool simpleModeEnabled = GameManager.Instance?.livePlayData?.simpleModeEnabled ?? false;
        
        int expectedTruthValue;
        
        if (simpleModeEnabled)
        {
            // In simple mode, notes pass through directly - just check the real truth value
            // Since all notes in simple mode have the same value, we can just check the first one
            expectedTruthValue = notesOnBeat[0].realTruthValue == 1 ? 1 : -1;
        }
        else
        {
            // Normal mode - use logic gate evaluation
            LogicSplitter splitter = GetLogicSplitter(activeLogicOperation);
            expectedTruthValue = splitter.EvaluateTruthValue(notesOnBeat.Select(n => n.truthValue).ToArray()) ? 1 : -1;
        }
        
        int buttonTruthValue = Mathf.RoundToInt(e.direction.y); // Up is 1, Down is -1
        
        if (expectedTruthValue == buttonTruthValue)
        {
            Debug.Log($"{buttonTruthValue} - {expectedTruthValue}");
            // Correct input
            foreach (var note in notesOnBeat)
            {
                note.ReturnToPool(true, scoreManager.GetScoreTypeByHitDelta(Mathf.Abs(note.beatStamp - CurrentBeatStamp) * BeatDuration));
            }
        }
        else
        {
            Debug.Log($"{buttonTruthValue} - {expectedTruthValue}");
            // Incorrect input
            foreach (var note in notesOnBeat)
            {                
                note.ReturnToPool(false, scoreManager.GetScoreTypeByHitDelta(Mathf.Abs(note.beatStamp - CurrentBeatStamp) * BeatDuration));
            }
        }
        
    }
    

    #region Logic Note Registration
    
    public void RegisterLogicNote(LogicNote note)
    {
        activeLogicNotes.Add(note);
    }
    
    public void UnregisterLogicNote(LogicNote note)
    {
        activeLogicNotes.Remove(note);
    }
    
    #endregion
    
    #region Logic Operations

    public void CycleOperation(BeatMapCheckpoint checkpoint)
    {
        LogicOperation previousOperation = activeLogicOperation;
        
        // Check if simple mode is enabled
        bool simpleModeEnabled = GameManager.Instance?.livePlayData?.simpleModeEnabled ?? false;
        
        // Get enabled operations from LivePlayData (player selection)
        LogicOperation[] allowedOps = GameManager.Instance?.livePlayData?.enabledOperations;
        
        // In simple mode, enable all logic gates for visual variety
        if (simpleModeEnabled)
        {
            allowedOps = (LogicOperation[])Enum.GetValues(typeof(LogicOperation));
        }
        
        // Fallback to all operations if LivePlayData not set
        if (allowedOps == null || allowedOps.Length == 0)
        {
            allowedOps = (LogicOperation[])Enum.GetValues(typeof(LogicOperation));
        }
        
        // Cycle to a different random operation
        int currentIndex = Array.IndexOf(allowedOps, activeLogicOperation);
        int newIndex = currentIndex;
        while (newIndex == currentIndex && allowedOps.Length > 1)
        {
            newIndex = UnityEngine.Random.Range(0, allowedOps.Length);
        }
        activeLogicOperation = allowedOps[newIndex];
        
        materialColorShifter.ShiftColors(activeLogicOperation);
        
        SoundEffectManager.Instance.Play(SoundEffectManager.Instance.soundEffectAtlas.operationChange);
        
        // Fire an event to notify other systems of the checkpoint reached and logic operation change
        EventBus<CheckpointReachedEvent>.Raise(new CheckpointReachedEvent(checkpoint, previousOperation, activeLogicOperation));
    }
    
    private void InitializeLogicSplitters()
    {
        logicSplitters[LogicOperation.And] = new AndSplitter(numberOfSpawnLocations);
        logicSplitters[LogicOperation.Or] = new OrSplitter(numberOfSpawnLocations);
        logicSplitters[LogicOperation.Nand] = new NandSplitter(numberOfSpawnLocations);
        logicSplitters[LogicOperation.Nor] = new NorSplitter(numberOfSpawnLocations);
        logicSplitters[LogicOperation.Xor] = new XorSplitter(numberOfSpawnLocations);
        logicSplitters[LogicOperation.Xnor] = new XnorSplitter(numberOfSpawnLocations);
    }
    
    public LogicSplitter GetLogicSplitter(LogicOperation operation)
    {
        if (logicSplitters.TryGetValue(operation, out var splitter))
        {
            return splitter;
        }
        throw new KeyNotFoundException($"LogicSplitter for operation {operation} not found.");
    }
    
    #endregion
}

public struct CheckpointReachedEvent : IEvent
{
    public BeatMapCheckpoint checkpoint;
    public LogicOperation previousOperation;
    public LogicOperation newOperation;
    
    public CheckpointReachedEvent(BeatMapCheckpoint checkpoint, LogicOperation previousOperation, LogicOperation newOperation)
    {
        this.checkpoint = checkpoint;
        this.previousOperation = previousOperation;
        this.newOperation = newOperation;
    }
}

public struct LogicNoteHitEvent : IEvent
{
    public readonly LogicNote hitNote;
    public readonly bool? isCorrect;
    public readonly ScoreType scoreType;
    public int actualValue;

    public LogicNoteHitEvent(LogicNote hitNote, bool? isCorrect, ScoreType scoreType, int actualValue)
    {
        this.hitNote = hitNote;
        this.isCorrect = isCorrect;
        this.scoreType = scoreType;
        this.actualValue = actualValue;
    }
}