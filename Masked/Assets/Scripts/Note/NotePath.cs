using System;
using UnityEngine;

public class NotePath : MonoBehaviour
{
    [Header("Note Path Settings")]
    public Transform[] waypoints; // the waypoints that define the path
    public int laneIndex; // the index of the lane this path belongs to

    /**
     * Returns an action that moves a Transform along the note path defined by the waypoints.
     */
    public Action<Transform, Action> GenerateNotePath()
    {
        // placeholder for now
        return (transform, onComplete) =>
        {
            // TODO: placeholder movement logic
            
            onComplete?.Invoke();
        };
    }
}