using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/**
 * <summary>
 * Takes a beat from the beatmap and the current logic operation and gives the multiple-note combination that should be spawned.
 * </summary>
 */
public class BeatSplitter : MonoBehaviour
{
    public NoteSpawner noteSpawner;
    
    [Header("Spawn Settings")]
    [Tooltip("Note speed - Higher = faster notes (more difficult), Lower = slower notes (easier). Range: 1-10")]
    [Range(1f, 10f)]
    public float playSpeed = 5f; // Speed at which notes travel
    
    [Tooltip("Base travel time in beats at speed 5. Used to calculate actual travel time based on playSpeed")]
    public float baseTravelTimeInBeats = 2f; // At playSpeed = 5, notes take 2 beats to travel
    
    // Calculate travel time inversely proportional to playSpeed
    // Higher playSpeed = shorter travel time = faster notes
    private float TravelTimeInBeats => baseTravelTimeInBeats * (5f / playSpeed);
    
    public float BeatSpawnOffset => TravelTimeInBeats; // Spawn notes this many beats early so they reach the hit point on time
    
    public float TravelTimeInSeconds => TravelTimeInBeats * BeatMapManager.Instance.BeatDuration; // Actual travel time in seconds
    
    public Queue<BeatDataEntry> BeatQueue { get; } = new();
    public Queue<BeatMapCheckpoint> CheckpointQueue { get; } = new();
    
    private BeatMapData BeatMapData => BeatMapManager.Instance?.beatMapData;
    
    
    #region Monobehaviour Methods

    private void Awake()
    {
        InitializeBeatMap();
    }

    private void Start()
    {
        // Pre-spawn notes that should be visible at the start
        // We need to spawn any notes that fall within the initial spawn window
        SpawnInitialNotes();
    }

    private void SpawnInitialNotes()
    {
        // At song start (SongTimer = 0), we need to spawn notes up to BeatSpawnOffset beats
        float initialSpawnBeat = TravelTimeInBeats;
        
        // Create a temporary list to hold notes we need to spawn initially
        List<BeatDataEntry> initialNotes = new List<BeatDataEntry>();
        
        // Check which notes from the queue should be pre-spawned
        while (BeatQueue.Count > 0 && BeatQueue.Peek().beatStamp <= initialSpawnBeat)
        {
            initialNotes.Add(BeatQueue.Dequeue());
        }
        
        // Spawn all initial notes
        foreach (var beatEntry in initialNotes)
        {
            // Determine truth value based on the entry's setting
            int truthValue;
            if (beatEntry.truthValue == TruthValue.Random)
            {
                truthValue = Random.Range(0, 2);
            }
            else
            {
                truthValue = (int)beatEntry.truthValue;
            }
            
            int[] noteSpawnIndicesForBeat = GetNoteSpawnIndicesForBeat(truthValue);
            SpawnNotesAtBeat(beatEntry.beatStamp, noteSpawnIndicesForBeat, beatEntry.laneIndex, truthValue);
        }
    }

    private void Update()
    {
        TrySpawnNotes();
        TryUpdateCheckpoints();
        TrySpawnBeatLines();
    }

    #endregion
    
    private int[] GetNoteSpawnIndicesForBeat(int truthValue)
    {
        LogicSplitter splitter = BeatMapManager.Instance.GetLogicSplitter(BeatMapManager.Instance.activeLogicOperation);
        var oneLocations = splitter.GetNotesForBeat(truthValue);
        
        int[] allIndices = new int[BeatMapManager.Instance.numberOfSpawnLocations];
        
        for (int i = 0; i < oneLocations.Length; i++)
        {
            if (oneLocations[i] == 1) // set spawn index to 1 if we need to spawn a note there
                allIndices[i] = 1;
        }
        
        return allIndices; // returns an array where each index corresponds to a spawn location, with 1 indicating a note should be spawned there
    }
    
    #region Note Spawning
    
    private void InitializeBeatMap()
    {
        // sort beat data entries in ascending order of beatStamp
        Array.Sort(BeatMapData.beatDataEntries, (a, b) => a.beatStamp.CompareTo(b.beatStamp));
        
        foreach (var beat in BeatMapData.beatDataEntries)
        {
            BeatQueue.Enqueue(beat); // Enqueue all beat stamps from the beat map data, so when a certain 
        }
        
        Array.Sort(BeatMapData.checkpoints, (a, b) => a.beatStamp.CompareTo(b.beatStamp));
        foreach (var checkpoint in BeatMapData.checkpoints)
        {
            CheckpointQueue.Enqueue(checkpoint); // Enqueue all checkpoints from the beat map data
        }
    }

    private void TrySpawnNotes()
    {
        // Calculate the current beat based on the song timer and beat duration
        float adjustedTime = BeatMapManager.Instance.SongTimer + BeatSpawnOffset * BeatMapManager.Instance.BeatDuration;
        float currentBeat = adjustedTime / BeatMapManager.Instance.BeatDuration;

        // Spawn notes for all beats that are due
        while (BeatQueue.Count > 0 && BeatQueue.Peek().beatStamp <= currentBeat)
        {
            BeatDataEntry beatEntry = BeatQueue.Dequeue();
            
            // Determine truth value based on the entry's setting
            int truthValue;
            if (beatEntry.truthValue == TruthValue.Random)
            {
                truthValue = Random.Range(0, 2); // Randomly assign truth value (0 or 1)
            }
            else
            {
                truthValue = (int)beatEntry.truthValue; // Use specified value (0 or 1)
            }
            
            int[] noteSpawnIndicesForBeat = GetNoteSpawnIndicesForBeat(truthValue);
            SpawnNotesAtBeat(beatEntry.beatStamp, noteSpawnIndicesForBeat, beatEntry.laneIndex, truthValue);
        }
    }
    
    private void TrySpawnBeatLines()
    {
        // Calculate the current beat based on the song timer and beat duration
        float adjustedTime = BeatMapManager.Instance.SongTimer + BeatSpawnOffset * BeatMapManager.Instance.BeatDuration;
        float currentBeat = adjustedTime / BeatMapManager.Instance.BeatDuration;
        
        if (Mathf.Abs(currentBeat % 1) > 0.01f) return; // Only spawn on integer beats
        
        int currentBeatInt = Mathf.RoundToInt(currentBeat);
        
        if (currentBeatInt % BeatMapManager.Instance.beatMapData.beatLineRate != 0) return;

        // Spawn beat lines on every beat on all lanes on all spawn locations
        
        bool isSuperLine = currentBeatInt % BeatMapManager.Instance.beatMapData.superBeatLineRate == 0;

        for (int lane = 0; lane < BeatMapManager.Instance.numberOfLanes; lane++)
        {
            for (int spawnLocation = 0; spawnLocation < BeatMapManager.Instance.numberOfSpawnLocations; spawnLocation++)
            {
                noteSpawner.SpawnBeatLineInLane(currentBeat, lane, spawnLocation, isSuperLine, TravelTimeInSeconds);
            }
        }
    }
    
    private void TryUpdateCheckpoints()
    {
        // Calculate the current beat based on the song timer and beat duration
        float currentBeat = BeatMapManager.Instance.CurrentBeatStamp;

        // Update checkpoints for all that are due
        while (CheckpointQueue.Count > 0 && CheckpointQueue.Peek().beatStamp <= currentBeat)
        {
            BeatMapCheckpoint checkpoint = CheckpointQueue.Dequeue();
            BeatMapManager.Instance.CycleOperation(checkpoint);
        }
    }
    
    /**
     * <summary>
     * Spawns notes at the specified beat stamp in the given lane for the provided note indices
     * </summary>
     *
     * <param name="beatStamp">The beat timestamp at which to spawn the notes</param>
     * <param name="noteSpawnIndices">An array defining which spawn locations to use for the notes</param>
     * <param name="lane">The lane index in which to spawn the notes</param>
     * <param name="realValue">The real value of the note (e.g., 0 or 1 for binary notes)</param>
     */
    private void SpawnNotesAtBeat(float beatStamp, int[] noteSpawnIndices, int lane, int realValue)
    {
        for (int i = 0; i < noteSpawnIndices.Length; i++)
        {
            noteSpawner.SpawnNoteInLane(beatStamp, lane, i, noteSpawnIndices[i], realValue, TravelTimeInSeconds);
        }

    }
    
    #endregion
}