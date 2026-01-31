using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New BeatMap Data", menuName = "BeatMap/BeatMap Data")]
public class BeatMapData : ScriptableObject
{
    [Header("Song Data")]
    public int bpm; // Beats per minute of the song

    public int beats; // Total number of beats in the song
    public AudioClip clip;
    
    [Header("Beat Data")]
    public BeatDataEntry[] beatDataEntries;
    public BeatMapCheckpoint[] checkpoints;

    private void OnValidate()
    {
        // sort beat data entries by beatStamp
        Array.Sort(beatDataEntries, (a, b) => a.beatStamp.CompareTo(b.beatStamp));
        
        // sort checkpoints by beatStamp
        Array.Sort(checkpoints, (a, b) => a.beatStamp.CompareTo(b.beatStamp));
    }
}

[Serializable]
public class BeatDataEntry
{
    public int beatStamp; // The beat at which the note should be hit
    public int laneIndex; // The lane index this note is assigned to
}

[Serializable]
public class BeatMapCheckpoint
{
    public string checkpointName; // A name or identifier for the checkpoint
    public int beatStamp; // The beat at which the checkpoint occurs
}