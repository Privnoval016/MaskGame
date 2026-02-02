using System;
using UnityEngine;

public enum TruthValue
{
    False = 0,
    True = 1,
    Random = 2
}

public enum Difficulty
{
    Easy = 0,
    Medium = 1,
    Hard = 2,
    Expert = 3,
    SuperExpert = 4
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

    [Header("Beat Data - Deprecated (Use difficultyBeatMaps instead)")]
    [SerializeField] private BeatDataEntry[] beatDataEntries; // Keep for backwards compatibility
    
    [Header("Difficulty BeatMaps")]
    public DifficultyBeatMap[] difficultyBeatMaps = new DifficultyBeatMap[5]; // One for each difficulty
    
    [Header("Playable Difficulties")]
    [Tooltip("Check which difficulties are ready and playable")]
    public bool easyPlayable = false;
    public bool mediumPlayable = true; // Default to medium being playable
    public bool hardPlayable = false;
    public bool expertPlayable = false;
    public bool superExpertPlayable = false;
    
    public BeatMapCheckpoint[] checkpoints;
    
    // Migration helper - called by Unity when values are deserialized
    private void OnValidate()
    {
        MigrateLegacyData();
    }
    
    /// <summary>
    /// Migrate legacy beatDataEntries to the new difficulty-based structure
    /// </summary>
    private void MigrateLegacyData()
    {
        // Initialize difficulty beatmaps array if needed
        if (difficultyBeatMaps == null || difficultyBeatMaps.Length != 5)
        {
            difficultyBeatMaps = new DifficultyBeatMap[5];
            for (int i = 0; i < 5; i++)
            {
                difficultyBeatMaps[i] = new DifficultyBeatMap
                {
                    difficulty = (Difficulty)i,
                    beatDataEntries = new BeatDataEntry[0]
                };
            }
        }
        
        // Ensure all difficulty beatmap elements are not null
        for (int i = 0; i < difficultyBeatMaps.Length && i < 5; i++)
        {
            if (difficultyBeatMaps[i] == null)
            {
                difficultyBeatMaps[i] = new DifficultyBeatMap
                {
                    difficulty = (Difficulty)i,
                    beatDataEntries = new BeatDataEntry[0]
                };
            }
            
            if (difficultyBeatMaps[i].beatDataEntries == null)
            {
                difficultyBeatMaps[i].beatDataEntries = new BeatDataEntry[0];
            }
            
            difficultyBeatMaps[i].difficulty = (Difficulty)i;
        }
        
        // Only migrate if we have legacy data
        if (beatDataEntries != null && beatDataEntries.Length > 0)
        {
            // Check if Medium difficulty is empty (likely not migrated yet)
            if (difficultyBeatMaps[1] != null && 
                (difficultyBeatMaps[1].beatDataEntries == null || difficultyBeatMaps[1].beatDataEntries.Length == 0))
            {
                // Copy legacy data to Medium difficulty as default
                difficultyBeatMaps[1].beatDataEntries = new BeatDataEntry[beatDataEntries.Length];
                for (int i = 0; i < beatDataEntries.Length; i++)
                {
                    difficultyBeatMaps[1].beatDataEntries[i] = new BeatDataEntry
                    {
                        beatStamp = beatDataEntries[i].beatStamp,
                        laneIndex = beatDataEntries[i].laneIndex,
                        truthValue = beatDataEntries[i].truthValue
                    };
                }
                
                Debug.Log($"[BeatMapData] Migrated {beatDataEntries.Length} legacy entries to Medium difficulty for '{songTitle}'");
                
                // Clear legacy data after migration
                beatDataEntries = new BeatDataEntry[0];
            }
        }
    }
    
    /// <summary>
    /// Get beat data entries for a specific difficulty
    /// </summary>
    public BeatDataEntry[] GetBeatDataForDifficulty(Difficulty difficulty)
    {
        if (difficultyBeatMaps == null || difficultyBeatMaps.Length == 0)
        {
            return new BeatDataEntry[0];
        }
        
        int index = (int)difficulty;
        if (index >= 0 && index < difficultyBeatMaps.Length)
        {
            return difficultyBeatMaps[index].beatDataEntries ?? new BeatDataEntry[0];
        }
        
        return new BeatDataEntry[0];
    }
    
    /// <summary>
    /// Check if a specific difficulty is playable
    /// </summary>
    public bool IsDifficultyPlayable(Difficulty difficulty)
    {
        switch (difficulty)
        {
            case Difficulty.Easy: return easyPlayable;
            case Difficulty.Medium: return mediumPlayable;
            case Difficulty.Hard: return hardPlayable;
            case Difficulty.Expert: return expertPlayable;
            case Difficulty.SuperExpert: return superExpertPlayable;
            default: return false;
        }
    }
}

[Serializable]
public class BeatDataEntry
{
    public float beatStamp; // The beat at which the note should be hit
    public int laneIndex; // The lane index this note is assigned to
    public TruthValue truthValue = TruthValue.Random; // The truth value for this note (0, 1, or random)
}

[Serializable]
public class DifficultyBeatMap
{
    public Difficulty difficulty;
    public BeatDataEntry[] beatDataEntries = new BeatDataEntry[0];
}

[Serializable]
public class BeatMapCheckpoint
{
    public string checkpointName; // A name or identifier for the checkpoint
    public float beatStamp; // The beat at which the checkpoint occurs
}