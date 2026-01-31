using System;
using System.Collections.Generic;
using UnityEngine;

public class NoteSpawner : MonoBehaviour
{
    [Header("Note Spawner Settings")]
    public NotePool notePool; // reference to the note pool

    [SerializeField] private NotePathCollection[] notePaths; // array of note path collections for different spawn rows

    private void OnValidate()
    {
        if (notePaths == null) return;
        for (int i = 0; i < notePaths.Length; i++)
        {            
            Array.Sort(notePaths[i].paths, (a, b) => a.laneIndex.CompareTo(b.laneIndex));
        }
    }

    /**
     * <summary>
     * Spawns a note in the specified lane and spawn location.
     * </summary>
     *
     * <param name="beatStamp">The beat stamp at which the note should be hit.</param>
     * <param name="laneIndex">The lane index in which to spawn the note.</param>
     * <param name="spawnLocationIndex">The index of the spawn location (row) to use.</param>
     * <param name="parity">The parity of the note (e.g., for distinguishing between different types of notes).</param>
     * <param name="beatTravelTime">The time it takes for the note to travel from spawn to hit point in seconds.</param>
     */
    public GameObject SpawnNoteInLane(int beatStamp, int laneIndex, int spawnLocationIndex, int parity, float beatTravelTime)
    {
        LogicNote note = notePool.Get();
        
        Debug.Log($"Spawning note at beatStamp {beatStamp}, laneIndex {laneIndex}, spawnLocationIndex {spawnLocationIndex}, parity {parity}");
        
        NotePathCollection pathCollection = notePaths[spawnLocationIndex];
        NotePath path = Array.Find(pathCollection.paths, p => p.laneIndex == laneIndex);
        if (path == null) 
        {
            Debug.Log($"Available lane indices for spawnLocationIndex {spawnLocationIndex}: {string.Join(", ", Array.ConvertAll(pathCollection.paths, p => p.laneIndex.ToString()))}");
            Debug.LogError($"No NotePath found for laneIndex {laneIndex} in spawnLocationIndex {spawnLocationIndex}");
            notePool.Return(note);
            return null;
        }
        
        Debug.Log($"Spawning note at beatStamp {beatStamp}, pathCollection: {path}");
        
        Action<Transform, Action> moveAction = path.GenerateNotePath(beatTravelTime);
        note.MoveNote(beatStamp, laneIndex, parity, moveAction);
        Debug.Log($"Spawned note at beatStamp {beatStamp}, laneIndex {laneIndex}, spawnLocationIndex {spawnLocationIndex}, parity {parity} at real position {note.transform.position}");
        return note.gameObject;
    }
}