using System;
using System.Collections.Generic;
using UnityEngine;

public class NoteSpawner : MonoBehaviour
{
    [Header("Note Spawner Settings")]
    public NotePool notePool; // reference to the note pool
    public BeatLinePool beatLinePool;

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
     * <param name="realValue">The real value of the note (e.g., 0 or 1 for binary notes).</param>
     * <param name="beatTravelTime">The time it takes for the note to travel from spawn to hit point in seconds.</param>
     */
    public GameObject SpawnNoteInLane(float beatStamp, int laneIndex, int spawnLocationIndex, int parity, int realValue, float beatTravelTime)
    {
        LogicNote note = notePool.Get();
        
        
        NotePathCollection pathCollection = notePaths[spawnLocationIndex];
        NotePath path = Array.Find(pathCollection.paths, p => p.laneIndex == laneIndex);
        if (path == null) 
        {
            Debug.LogError($"No NotePath found for laneIndex {laneIndex} in spawnLocationIndex {spawnLocationIndex}");
            notePool.Return(note);
            return null;
        }

        Action<Transform, Action> moveAction = path.GenerateNotePath(beatTravelTime - 0.05f);
        note.MoveNote(beatStamp, laneIndex, spawnLocationIndex, parity, realValue, moveAction);
        return note.gameObject;
    }
    
    public GameObject SpawnBeatLineInLane(float beatStamp, int laneIndex, int spawnLocationIndex, bool isSuperLine, float beatTravelTime)
    {
        BeatLine beatLine = beatLinePool.Get();
        
        
        NotePathCollection pathCollection = notePaths[spawnLocationIndex];
        NotePath path = Array.Find(pathCollection.paths, p => p.laneIndex == laneIndex);
        if (path == null) 
        {
            Debug.LogError($"No NotePath found for laneIndex {laneIndex} in spawnLocationIndex {spawnLocationIndex}");
            beatLinePool.Return(beatLine);
            return null;
        }
        
        Action<Transform, Action> moveAction = path.GenerateNotePath(beatTravelTime, 0.22f);
        beatLine.MoveBeatLine(beatStamp, laneIndex, moveAction, isSuperLine);
        return beatLine.gameObject;
    }
}