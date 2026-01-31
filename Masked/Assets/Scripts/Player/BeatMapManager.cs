using System;
using System.Collections.Generic;
using System.Linq;
using Extensions.EventBus;
using Extensions.Patterns;
using UnityEngine;

public class BeatMapManager : Singleton<BeatMapManager>
{
    public float SongTimer { get; private set; }
    
    [Header("Startup Settings")]
    public BeatMapData beatMapData;
    public float startupDelay = 2f; // Delay before the first beat spawns, to give player time to prepare and account for initial note travel time
    public float BeatDuration => 60f / beatMapData.bpm; // Duration of a single beat in seconds.
    
    public int numberOfLanes = 4; // Total number of lanes available (valid note indices: 0 to numberOfLanes - 1)
    public int numberOfSpawnLocations = 2; // Number of different spawn locations for notes (right now top and bottom)
    
    public int CurrentBeatStamp => Mathf.FloorToInt(SongTimer / BeatDuration);

    [Header("Runtime Values")] 
    public LogicOperation activeLogicOperation = LogicOperation.Or;
    
    private EventBinding<ButtonPressedEvent> buttonPressedBinding;
    
    public Dictionary<LogicOperation, LogicSplitter> logicSplitters = new Dictionary<LogicOperation, LogicSplitter>();
    
    // store all active logic notes so we can check for hits by the player and deal with score accordingly
    private readonly HashSet<LogicNote> activeLogicNotes = new HashSet<LogicNote>();
    
    protected override void Awake()
    {
        base.Awake(); 
        SongTimer = -startupDelay; // Start the song timer with a negative offset to account for startup delay
        
        buttonPressedBinding = new EventBinding<ButtonPressedEvent>(OnButtonPressed);
        EventBus<ButtonPressedEvent>.Register(buttonPressedBinding);
        
        InitializeLogicSplitters();
    }

    private void OnDisable()
    {
        EventBus<ButtonPressedEvent>.Deregister(buttonPressedBinding);
    }

    private void Update()
    {
        SongTimer += Time.deltaTime;
    }

    private void OnButtonPressed(ButtonPressedEvent e)
    {
        List<LogicNote> notesOnBeat = new List<LogicNote>();
        foreach (var note in activeLogicNotes)
        {
            if (note.beatStamp == CurrentBeatStamp && note.laneIndex == e.buttonIndex)
            {
                notesOnBeat.Add(note);
            }
        }
        
        // Check to see if the direction of the button pressed matches the evaluated logic operation
        LogicSplitter splitter = GetLogicSplitter(activeLogicOperation);
        int expectedTruthValue = splitter.EvaluateTruthValue(notesOnBeat.Select(n => n.parity).ToArray()) ? 1 : -1;
        int buttonTruthValue = Mathf.RoundToInt(e.direction.y); // Up is 1, Down is -1
        
        if (expectedTruthValue == buttonTruthValue)
        {
            // Correct input
            foreach (var note in notesOnBeat)
            {
                note.successfullyHit = true;
            }
            notesOnBeat[0].broadcastingNote = true; // Only need to broadcast one note hit event for the group
        }
        else
        {
            // Incorrect input
            foreach (var note in notesOnBeat)
            {
                note.successfullyHit = false;
            }
            notesOnBeat[0].broadcastingNote = true; // Only need to broadcast one note hit event for the group
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
        // Cycle to a random logic operation from the allowed operations in the beat map data
        Array allowedOps = beatMapData.allowedOperations;
        activeLogicOperation = (LogicOperation)allowedOps.GetValue(UnityEngine.Random.Range(0, allowedOps.Length));
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
    public readonly bool isCorrect;

    public LogicNoteHitEvent(LogicNote hitNote, bool isCorrect)
    {
        this.hitNote = hitNote;
        this.isCorrect = isCorrect;
    }
}