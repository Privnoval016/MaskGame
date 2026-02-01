using System;
using System.Collections.Generic;
using UnityEngine;
using PrimeTween;
using Sirenix.OdinInspector;

public class NotePath : MonoBehaviour
{
    [Header("Path Settings")]
    public Transform[] waypoints;

    public int laneIndex;
    public int samplesPerSegment = 100; // higher = smoother curve, more accurate constant speed
    public float zOffset = -2.1f; // how far backwards to offset the generated mesh along the Z axis

    private Vector3[] pathSamples; // sampled points along the path in world space
    private Vector3[] meshPathSamples; // sampled points along the path in local space for mesh generation
    private float[] cumulativeDistances;
    private float totalPathLength;

    private void Awake()
    {
        PrecomputePath();
    }

    [Button("Precompute Path")]
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
        meshPathSamples = new Vector3[pathSamples.Length];
        for (int i = 0; i < pathSamples.Length; i++)
        {
            meshPathSamples[i] = pathSamples[i] + new Vector3(0f, 0f, zOffset);
        }

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
    
    #region In Between Mesh
    
    [Button("Generate All Path Meshes")]
    public void GeneratePathBetweenWaypoints(float width, Vector3 up)
    {
        if (meshPathSamples == null || meshPathSamples.Length == 0) PrecomputePath();

        Mesh mesh = new Mesh();
        mesh.name = "PathRibbonMesh";

        int segmentCount = meshPathSamples.Length - 1;

        var vertices = new List<Vector3>(segmentCount * 2);
        var triangles = new List<int>(segmentCount * 6);
        var uvs = new List<Vector2>(segmentCount * 2);

        // Precompute total path length for continuous UV mapping
        float totalLength = 0f;
        var segmentLengths = new float[segmentCount];
        for (int i = 0; i < segmentCount; i++)
        {
            segmentLengths[i] = Vector3.Distance(meshPathSamples[i], meshPathSamples[i + 1]);
            totalLength += segmentLengths[i];
        }

        // Build a shared vertex strip instead of isolated quads.
        // Each sample point becomes two vertices (left and right edge),
        // and adjacent rows are stitched with two triangles.
        float accumulatedLength = 0f;

        for (int i = 0; i <= segmentCount; i++)
        {
            Vector3 point = transform.InverseTransformPoint(meshPathSamples[i]);

            // Compute the forward direction at this sample point
            Vector3 dir;
            if (i == 0)
                dir = (transform.InverseTransformPoint(meshPathSamples[1]) - point).normalized;
            else if (i == segmentCount)
                dir = (point - transform.InverseTransformPoint(meshPathSamples[i - 1])).normalized;
            else
                dir = (transform.InverseTransformPoint(meshPathSamples[i + 1]) - transform.InverseTransformPoint(meshPathSamples[i - 1])).normalized;

            if (dir.sqrMagnitude < 0.0001f)
                dir = Vector3.forward;

            Vector3 side = Vector3.Cross(up, dir).normalized * (width * 0.5f);

            // UV.x = continuous normalized distance along the full path [0, 1]
            // UV.y = 0 on left edge, 1 on right edge
            float u = totalLength > 0f ? accumulatedLength / totalLength : 0f;

            int baseIndex = vertices.Count;
            vertices.Add(point - side); // left edge
            vertices.Add(point + side); // right edge
            uvs.Add(new Vector2(u, 0f));
            uvs.Add(new Vector2(u, 1f));

            // Stitch triangles to the previous row (skip the very first row)
            if (i > 0)
            {
                int prev = baseIndex - 2;
                // Triangle 1: prev-left, curr-left, prev-right
                triangles.Add(prev + 0);
                triangles.Add(baseIndex + 0);
                triangles.Add(prev + 1);
                // Triangle 2: curr-left, curr-right, prev-right
                triangles.Add(baseIndex + 0);
                triangles.Add(baseIndex + 1);
                triangles.Add(prev + 1);
            }

            // Accumulate length for the next iteration
            if (i < segmentCount)
                accumulatedLength += segmentLengths[i];
        }

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetUVs(0, uvs);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        
        //GetComponent<MeshRenderer>().material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent - 1; // 3000 - 1 = 2999

        GetComponent<MeshFilter>().sharedMesh = mesh;
        GetComponent<MeshRenderer>().enabled = true;
    }
    
    #endregion
}