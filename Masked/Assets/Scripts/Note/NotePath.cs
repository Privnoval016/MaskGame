using System;
using UnityEngine;
using PrimeTween;

public class NotePath : MonoBehaviour
{
    [Header("Note Path Settings")]
    public Transform[] waypoints; // the waypoints that define the path
    public int laneIndex; // the index of the lane this path belongs to

    /**
     * Returns an action that moves a Transform along the note path defined by the waypoints.
     */
    public Action<Transform, Action> GenerateNotePath(float totalTravelTime)
    {
        float segmentTime = totalTravelTime / (waypoints.Length - 1);
        return (target, onComplete) =>
        {
            target.position = waypoints[0].position; // Start at the first waypoint
            // Create a tween sequence to move through waypoints
            Sequence sequence = Sequence.Create();
            
            for (int i = 1; i < waypoints.Length; i++)
            {
                sequence.Chain(Tween.Position(target, waypoints[i].position, segmentTime, Ease.Linear));
            }
            
            sequence.OnComplete(() =>
            {
                onComplete?.Invoke();
            });
        };
    }
}