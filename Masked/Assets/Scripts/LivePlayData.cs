using UnityEngine;

/// <summary>
/// Runtime data for the current play session, set by menu and used during gameplay
/// </summary>
[System.Serializable]
public class LivePlayData
{
    public BeatMapData selectedBeatMap;
    public float playSpeed = 5f; // 1-10 range
    public LogicOperation[] enabledOperations; // Player-selected operations for difficulty
    
    public LivePlayData(BeatMapData beatMap, float speed, LogicOperation[] operations)
    {
        selectedBeatMap = beatMap;
        playSpeed = speed;
        enabledOperations = operations;
    }
}

