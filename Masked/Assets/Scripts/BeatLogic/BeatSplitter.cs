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
    [Tooltip("Base travel time in beats at speed 5. Used to calculate actual travel time based on playSpeed")]
    public float baseTravelTimeInBeats = 2f; // At playSpeed = 5, notes take 2 beats to travel
    
    // Get play speed from GameManager's LivePlayData, fallback to 5 if not set
    private float PlaySpeed => GameManager.Instance?.livePlayData?.playSpeed ?? 5f;
    
    // Calculate travel time inversely proportional to playSpeed
    // Higher playSpeed = shorter travel time = faster notes
    private float TravelTimeInBeats => baseTravelTimeInBeats * (5f / PlaySpeed);
    
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


    private void Update()
    {
        TrySpawnNotes();
        TryUpdateCheckpoints();
        TrySpawnBeatLines();
    }

    #endregion
    
    private int[] GetNoteSpawnIndicesForBeat(int truthValue)
    {
        // Check if simple mode is enabled
        bool simpleModeEnabled = GameManager.Instance?.livePlayData?.simpleModeEnabled ?? false;
        
        if (simpleModeEnabled)
        {
            // In simple mode, both spawn locations get the same truth value
            // This makes the note pass through directly (if input is 1, both notes are 1; if input is 0, both notes are 0)
            int[] allIndices = new int[BeatMapManager.Instance.numberOfSpawnLocations];
            
            for (int i = 0; i < allIndices.Length; i++)
            {
                allIndices[i] = truthValue; // Set all spawn locations to the same value
            }
            
            return allIndices;
        }
        else
        {
            // Normal mode - use logic splitter
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
    }
    
    #region Note Spawning
    
    private void InitializeBeatMap()
    {
        // Get beat data for selected difficulty
        Difficulty selectedDifficulty = GameManager.Instance?.livePlayData?.selectedDifficulty ?? Difficulty.Medium;
        BeatDataEntry[] beatData = BeatMapData.GetBeatDataForDifficulty(selectedDifficulty);
        
        // Sort beat data entries in ascending order of beatStamp
        Array.Sort(beatData, (a, b) => a.beatStamp.CompareTo(b.beatStamp));
        
        foreach (var beat in beatData)
        {
            BeatQueue.Enqueue(beat); // Enqueue all beat stamps from the beat map data
        }
        
        // Initialize checkpoints
        if (BeatMapData.checkpoints != null)
        {
            Array.Sort(BeatMapData.checkpoints, (a, b) => a.beatStamp.CompareTo(b.beatStamp));
            foreach (var checkpoint in BeatMapData.checkpoints)
            {
                CheckpointQueue.Enqueue(checkpoint); // Enqueue all checkpoints from the beat map data
            }
        }
    }

    private void TrySpawnNotes()
    {
        // Allow spawning when we're within travel time of the song start
        // This ensures notes for beat 0 spawn early enough to travel to the hit zone
        float earliestAllowedSpawnTime = -(BeatSpawnOffset * BeatMapManager.Instance.BeatDuration);
        if (BeatMapManager.Instance.SongTimer < earliestAllowedSpawnTime) return;
        
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
        // Allow spawning when we're within travel time of the song start
        float earliestAllowedSpawnTime = -(BeatSpawnOffset * BeatMapManager.Instance.BeatDuration);
        if (BeatMapManager.Instance.SongTimer < earliestAllowedSpawnTime) return;
        
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