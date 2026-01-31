using System;
using System.Collections.Generic;
using UnityEngine;

/**
 * <summary>
 * Takes a beat from the beatmap and the current logic operation and gives the multiple-note combination that should be spawned.
 * </summary>
 */
public class BeatSplitter : MonoBehaviour
{
    public NoteSpawner noteSpawner;
    
    [Header("Spawn Settings")]
    public float playSpeed = 10f; // Speed at which notes travel toward the player, used to calculate timing of when to spawn notes such that they arrive on beat
    
    public int distanceFromSpawnToHitPoint = 20; // Distance from spawn point to hit point, used to calculate when to spawn notes
    
    public float BeatSpawnOffset => distanceFromSpawnToHitPoint / playSpeed; // Time offset to spawn notes early so they reach the hit point on time.
    
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
    }

    #endregion
    
    private int[] GetNoteSpawnIndicesForBeat(int truthValue)
    {
        LogicSplitter splitter = BeatMapManager.Instance.GetLogicSplitter(BeatMapManager.Instance.activeLogicOperation);
        return splitter.GetNotesForBeat(truthValue);
    }
    
    #region Note Spawning
    
    private void InitializeBeatMap()
    {
        // sort beat data entries by beat stamp to ensure correct order
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
        float adjustedTime = BeatMapManager.Instance.SongTimer + BeatSpawnOffset;
        int currentBeat = Mathf.FloorToInt(adjustedTime / BeatMapManager.Instance.BeatDuration);

        // Spawn notes for all beats that are due
        while (BeatQueue.Count > 0 && BeatQueue.Peek().beatStamp <= currentBeat)
        {
            BeatDataEntry beatEntry = BeatQueue.Dequeue();
            int truthValue = beatEntry.beatStamp % 2; // Example: Using even/odd beat stamps as truth values (0 or 1)
            int[] noteSpawnIndicesForBeat = GetNoteSpawnIndicesForBeat(truthValue);
            SpawnNotesAtBeat(beatEntry.beatStamp, noteSpawnIndicesForBeat, beatEntry.laneIndex);
        }
    }
    
    private void TryUpdateCheckpoints()
    {
        // Calculate the current beat based on the song timer and beat duration
        float adjustedTime = BeatMapManager.Instance.SongTimer + BeatSpawnOffset;
        int currentBeat = Mathf.FloorToInt(adjustedTime / BeatMapManager.Instance.BeatDuration);

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
     */
    private void SpawnNotesAtBeat(int beatStamp, int[] noteSpawnIndices, int lane)
    {
        foreach (int spawnIndex in noteSpawnIndices)
        {
            noteSpawner.SpawnNoteInLane(beatStamp, lane, spawnIndex, 1);
        }

        foreach (int spawnIndex in noteSpawnIndices)
        {
            noteSpawner.SpawnNoteInLane(beatStamp, lane, spawnIndex, 0);
        }
    }
    
    #endregion
}