using UnityEngine;

/// <summary>
/// Runtime data for the current play session, set by menu and used during gameplay
/// </summary>
[System.Serializable]
public class LivePlayData
{
    public BeatMapData selectedBeatMap;
    public Difficulty selectedDifficulty = Difficulty.Medium; // Selected difficulty level
    public float playSpeed = 5f; // 1-10 range
    public LogicOperation[] enabledOperations; // Player-selected operations for difficulty
    public bool autoPlayEnabled = false; // If true, AutoPlayer takes over and hits all notes perfectly
    public bool simpleModeEnabled = false; // If true, disables logic gate mechanics - notes pass through directly
    
    public LivePlayData(BeatMapData beatMap, Difficulty difficulty, float speed, LogicOperation[] operations, bool autoPlay = false, bool simpleMode = false)
    {
        selectedBeatMap = beatMap;
        selectedDifficulty = difficulty;
        playSpeed = speed;
        enabledOperations = operations;
        autoPlayEnabled = autoPlay;
        simpleModeEnabled = simpleMode;
    }
}

