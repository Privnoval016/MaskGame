using System;
using UnityEngine;

public enum TruthValue
{
    False = 0,
    True = 1,
    Random = 2
}

[CreateAssetMenu(fileName = "New BeatMap Data", menuName = "BeatMap/BeatMap Data")]
public class BeatMapData : ScriptableObject
{
    [Header("Song Info")]
    public string songTitle = "Untitled Song";
    public string author = "Unknown Artist";
    public Sprite coverArt; // Song cover image for menu display
    
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
}

[Serializable]
public class BeatDataEntry
{
    public float beatStamp; // The beat at which the note should be hit
    public int laneIndex; // The lane index this note is assigned to
    public TruthValue truthValue = TruthValue.Random; // The truth value for this note (0, 1, or random)
}

[Serializable]
public class BeatMapCheckpoint
{
    public string checkpointName; // A name or identifier for the checkpoint
    public float beatStamp; // The beat at which the checkpoint occurs
}