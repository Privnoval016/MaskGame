using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New BeatMap Data", menuName = "BeatMap/BeatMap Data")]
public class BeatMapData : ScriptableObject
{
    [Header("Song Data")]
    public int bpm; // Beats per minute of the song

    public int beats; // Total number of beats in the song
    public AudioClip clip;

    [Header("Visual Data")] 
    public int beatLineRate = 1; // spawn a beat line every n beats
    public int superBeatLineRate = 4; // spawn a super beat line every n beats
    
    [Header("Beat Data")]
    public BeatDataEntry[] beatDataEntries;
    public BeatMapCheckpoint[] checkpoints;
    
    [Header("Logic Data")]
    public LogicOperation[] allowedOperations; // Logic operations allowed in this beatmap
}

[Serializable]
public class BeatDataEntry
{
    public float beatStamp; // The beat at which the note should be hit
    public int laneIndex; // The lane index this note is assigned to
}

[Serializable]
public class BeatMapCheckpoint
{
    public string checkpointName; // A name or identifier for the checkpoint
    public float beatStamp; // The beat at which the checkpoint occurs
}