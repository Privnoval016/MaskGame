using System;
using System.Collections.Generic;
using UnityEngine;
using PrimeTween;

public class NotePath : MonoBehaviour
{
    [Header("Path Settings")]
    public Transform[] waypoints;

    public int laneIndex;
    public int samplesPerSegment = 100; // higher = smoother curve, more accurate constant speed

    private Vector3[] pathSamples;
    private float[] cumulativeDistances;
    private float totalPathLength;

    private void Awake()
    {
        PrecomputePath();
    }

    public void PrecomputePath()
    {
        if (waypoints.Length < 2) return;

        List<Vector3> samples = new List<Vector3>();
        
        // For each segment between waypoints, create a smooth bezier curve
        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            Vector3 p0 = waypoints[i].position;
            Vector3 p3 = waypoints[i + 1].position;
            
            // Create control points for smooth cubic bezier
            Vector3 p1, p2;
            
            if (waypoints.Length == 2)
            {
                // Just two points - use linear interpolation
                p1 = Vector3.Lerp(p0, p3, 0.33f);
                p2 = Vector3.Lerp(p0, p3, 0.66f);
            }
            else
            {
                // Calculate tangent direction for smooth curve
                Vector3 tangent1, tangent2;
                
                if (i == 0)
                {
                    // First segment - tangent points toward next waypoint
                    tangent1 = (waypoints[i + 1].position - waypoints[i].position).normalized;
                }
                else
                {
                    // Use previous and next points to calculate tangent
                    tangent1 = (waypoints[i + 1].position - waypoints[i - 1].position).normalized;
                }
                
                if (i == waypoints.Length - 2)
                {
                    // Last segment - tangent points from previous waypoint
                    tangent2 = (waypoints[i + 1].position - waypoints[i].position).normalized;
                }
                else
                {
                    // Use previous and next points
                    tangent2 = (waypoints[i + 2].position - waypoints[i].position).normalized;
                }
                
                // Control points positioned along tangent directions
                float distance = Vector3.Distance(p0, p3);
                p1 = p0 + tangent1 * (distance * 0.33f);
                p2 = p3 - tangent2 * (distance * 0.33f);
            }
            
            // Sample the cubic bezier curve
            int startIdx = samples.Count;
            for (int s = 0; s <= samplesPerSegment; s++)
            {
                float t = s / (float)samplesPerSegment;
                Vector3 point = CubicBezier(p0, p1, p2, p3, t);
                
                // Avoid duplicate points at segment boundaries
                if (s == 0 && startIdx > 0) continue;
                
                samples.Add(point);
            }
        }

        pathSamples = samples.ToArray();

        // Compute cumulative arc-length distance along the path
        cumulativeDistances = new float[pathSamples.Length];
        cumulativeDistances[0] = 0f;
        totalPathLength = 0f;
        
        for (int i = 1; i < pathSamples.Length; i++)
        {
            totalPathLength += Vector3.Distance(pathSamples[i - 1], pathSamples[i]);
            cumulativeDistances[i] = totalPathLength;
        }

        // Normalize to [0, 1] range
        if (totalPathLength > 0)
        {
            for (int i = 0; i < cumulativeDistances.Length; i++)
            {
                cumulativeDistances[i] /= totalPathLength;
            }
        }
    }

    public Action<Transform, Action> GenerateNotePath(float totalTravelTime, float lastSegmentZOffset = 0f)
    {
        if (pathSamples == null || pathSamples.Length == 0) PrecomputePath();

        return (target, onComplete) =>
        {
            Tween.Custom(0f, 1f, totalTravelTime, tLinear =>
            {
                // Clamp to valid range
                tLinear = Mathf.Clamp01(tLinear);
                
                // Find the two samples that bracket this normalized distance
                int index = FindIndexForNormalizedDistance(tLinear);
                
                if (index >= pathSamples.Length - 1)
                {
                    target.position = pathSamples[pathSamples.Length - 1];
                    return;
                }
                
                // Interpolate between the two nearest samples
                float t0 = cumulativeDistances[index];
                float t1 = cumulativeDistances[index + 1];
                float segmentT = Mathf.InverseLerp(t0, t1, tLinear);
                
                target.position = Vector3.Lerp(pathSamples[index], pathSamples[index + 1], segmentT);
                
                // Apply last segment Z offset if specified
                if (lastSegmentZOffset != 0f && index >= pathSamples.Length - 2)
                {
                    target.position += new Vector3(0f, 0f, lastSegmentZOffset * segmentT);
                }

            }, ease: Ease.Linear).OnComplete(() => onComplete?.Invoke());
        };
    }

    private int FindIndexForNormalizedDistance(float normalizedDistance)
    {
        // Binary search for the segment containing this normalized distance
        if (normalizedDistance <= 0f) return 0;
        if (normalizedDistance >= 1f) return pathSamples.Length - 1;
        
        int low = 0;
        int high = cumulativeDistances.Length - 1;
        
        while (low < high - 1)
        {
            int mid = (low + high) / 2;
            if (cumulativeDistances[mid] <= normalizedDistance)
                low = mid;
            else
                high = mid;
        }
        
        return low;
    }

    private Vector3 CubicBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float u = 1f - t;
        float uu = u * u;
        float uuu = uu * u;
        float tt = t * t;
        float ttt = tt * t;

        Vector3 point = uuu * p0;
        point += 3f * uu * t * p1;
        point += 3f * u * tt * p2;
        point += ttt * p3;

        return point;
    }
}