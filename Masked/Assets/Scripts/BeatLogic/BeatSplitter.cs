using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;
using UnityEngine;
using Extensions.Timers;

/**
 * <summary>
 * Takes a beat from the beatmap and the current logic operation and gives the multiple-note combination that should be spawned.
 * </summary>
 */
public class BeatSplitter : MonoBehaviour
{
    public BeatMapData beatMapData;
    
    [Header("Game Settings")]
    public float playSpeed = 10f; // Speed at which notes travel toward the player, used to calculate timing of when to spawn notes such that they arrive on beat
    public int numberOfLanes = 4; // Total number of lanes available (valid note indices: 0 to numberOfLanes - 1)
    public int numberOfSpawnLocations = 2; // Number of different spawn locations for notes (right now top and bottom)
    
    public float startupDelay = 2f; // Delay before the first beat spawns, to give player time to prepare and account for initial note travel time
    
    public int distanceFromSpawnToHitPoint = 20; // Distance from spawn point to hit point, used to calculate when to spawn notes

    [Header("Live Settings")] 
    public LogicOperation activeLogicOperation = LogicOperation.Or; // Current logic operation being used to split beats into notes.
    
    public float BeatDuration => 60f / beatMapData.bpm; // Duration of a single beat in seconds.
    
    public Dictionary<LogicOperation, LogicSplitter> logicSplitters = new Dictionary<LogicOperation, LogicSplitter>();
    
    public float BeatSpawnOffset => distanceFromSpawnToHitPoint / playSpeed; // Time offset to spawn notes early so they reach the hit point on time.

    private float songTimer;
    
    public Queue<BeatDataEntry> BeatQueue { get; } = new();
    
    #region Monobehaviour Methods

    private void Awake()
    {
        InitializeLogicSplitters();
        InitializeBeatMap();
    }

    private void Update()
    {
        TrySpawnNotes();
    }

    #endregion
    
    #region Logic Splitting
    
    public LogicSplitter GetLogicSplitter(LogicOperation operation)
    {
        if (logicSplitters.TryGetValue(operation, out var splitter))
        {
            return splitter;
        }
        throw new KeyNotFoundException($"LogicSplitter for operation {operation} not found.");
    }
    
    private void InitializeLogicSplitters()
    {
        logicSplitters[LogicOperation.And] = new AndSplitter(numberOfLanes);
        logicSplitters[LogicOperation.Or] = new OrSplitter(numberOfLanes);
        logicSplitters[LogicOperation.Nand] = new NandSplitter(numberOfLanes);
        logicSplitters[LogicOperation.Nor] = new NorSplitter(numberOfLanes);
        logicSplitters[LogicOperation.Xor] = new XorSplitter(numberOfLanes);
        logicSplitters[LogicOperation.Xnor] = new XnorSplitter(numberOfLanes);
    }
    
    #endregion
    
    private int[] GetNoteIndicesForBeat(int truthValue)
    {
        LogicSplitter splitter = GetLogicSplitter(activeLogicOperation);
        return splitter.GetNotesForBeat(truthValue);
    }
    
    #region Note Spawning
    
    private void InitializeBeatMap()
    {
        // sort beat data entries by beat stamp to ensure correct order
        Array.Sort(beatMapData.beatDataEntries, (a, b) => a.beatStamp.CompareTo(b.beatStamp));
        
        foreach (var beat in beatMapData.beatDataEntries)
        {
            BeatQueue.Enqueue(beat); // Enqueue all beat stamps from the beat map data, so when a certain 
        }
        
        songTimer = -startupDelay; // Start the song timer with a negative offset to account for startup delay
    }

    private void TrySpawnNotes()
    {
        songTimer += Time.deltaTime;

        // Calculate the current beat based on the song timer and beat duration
        float adjustedTime = songTimer + BeatSpawnOffset;
        int currentBeat = Mathf.FloorToInt(adjustedTime / BeatDuration);

        // Spawn notes for all beats that are due
        while (BeatQueue.Count > 0 && BeatQueue.Peek().beatStamp <= currentBeat)
        {
            BeatDataEntry beatEntry = BeatQueue.Dequeue();
            int truthValue = beatEntry.beatStamp % 2; // Example: Using even/odd beat stamps as truth values (0 or 1)
            int[] noteIndices = GetNoteIndicesForBeat(truthValue);
            SpawnNotesAtBeat(beatEntry.beatStamp, noteIndices, beatEntry.laneIndex);
        }
    }
    
    private void SpawnNotesAtBeat(int beatStamp, int[] noteIndices, int lane)
    {
        
    }
    
    #endregion
}

public interface LogicSplitter
{
    /** <summary>
     * Given a truth value, returns the array of note lane indices that should be spawned for that beat.
     * If there are multiple note combinations that could result in the same truth value, return a random valid combination.
     * </summary>
     *
     * <param name="truthValue">The truth value for the current beat. Can be 0 or 1.</param>
     * <returns>An array of note lane indices to spawn for the given truth value.</returns>
     */
    public int[] GetNotesForBeat(int truthValue);
}