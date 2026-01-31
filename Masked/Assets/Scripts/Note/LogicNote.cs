using System;
using UnityEngine;

public class LogicNote : MonoBehaviour
{
    [Header("Logic Note Settings")]
    public int noteID; // the notes id in the music track
    public int laneIndex; // the lane index this note is traveling toward
    public NotePath path; // the path this note follows

    private void Update()
    {
        throw new NotImplementedException();
    }
}