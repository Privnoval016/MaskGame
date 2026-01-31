using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class TransformData
{
    public Vector3 localPosition;
    public Quaternion localRotation;
    public Vector3 localScale;
    
    public TransformData(Transform t)
    {
        localPosition = t.localPosition;
        localRotation = t.localRotation;
        localScale = t.localScale;
    }
    
    public void ApplyTo(Transform t)
    {
        t.localPosition = localPosition;
        t.localRotation = localRotation;
        t.localScale = localScale;
    }
}

public class PathScalerHelper : MonoBehaviour
{
    [Header("References")]
    public Camera cam;
    
    [Tooltip("The root transforms containing the paths (e.g., each lane's path collection)")]
    public Transform[] pathRoots;
    
    [Tooltip("The hit area plane that stretches horizontally")]
    public Transform hitAreaPlane;
    
    [Header("Reference Dimensions")]
    [Tooltip("The reference aspect ratio this game was designed for (e.g., 16:9 = 1.7778)")]
    public float referenceAspectRatio = 16f / 9f;
    
    [Header("Scaling Options")]
    [Tooltip("Should paths scale horizontally to fit screen width?")]
    public bool scaleHorizontally = true;
    
    [Tooltip("Should paths scale vertically based on aspect ratio changes?")]
    public bool scaleVertically;
    
    [Tooltip("Distribute NotePaths evenly across screen with n+2 slots (buffer on edges)")]
    public bool distributeLanesEvenly = true;
    
    [Tooltip("Z-distance from camera to use for lane distribution calculation")]
    public float laneDistributionDepth = 5f;
    
    [Tooltip("Automatically scale on Awake")]
    public bool autoScaleOnStart = true;
    
    [Tooltip("Scale in editor when this component is modified")]
    public bool autoScaleInEditor = true;
    
    // Store original transforms for all objects
    private Dictionary<Transform, TransformData> originalTransforms = new Dictionary<Transform, TransformData>();
    private bool originalsSaved;

    private void Awake()
    {
        // Save original transforms first time
        if (!originalsSaved)
        {
            SaveOriginalTransforms();
        }
        
        if (autoScaleOnStart)
        {
            ScalePath();
        }
    }

    private void OnValidate()
    {
#if UNITY_EDITOR
        if (autoScaleInEditor && Application.isPlaying == false)
        {
            // Delay to ensure all references are set
            EditorApplication.delayCall += () =>
            {
                if (this != null)
                {
                    // Save originals if not already saved
                    if (!originalsSaved)
                    {
                        SaveOriginalTransforms();
                    }
                    ScalePath();
                }
            };
        }
#endif
    }
    
    [Button("Save Current as Original")]
    private void SaveOriginalTransforms()
    {
        originalTransforms.Clear();
        
        // Save path roots and all their children
        if (pathRoots != null)
        {
            foreach (Transform pathRoot in pathRoots)
            {
                if (pathRoot != null)
                {
                    SaveTransformRecursive(pathRoot);
                }
            }
        }
        
        // Save hit area
        if (hitAreaPlane != null)
        {
            originalTransforms[hitAreaPlane] = new TransformData(hitAreaPlane);
        }
        
        originalsSaved = true;
        Debug.Log($"Saved original transforms for {originalTransforms.Count} objects");
    }
    
    private void SaveTransformRecursive(Transform t)
    {
        originalTransforms[t] = new TransformData(t);
        
        for (int i = 0; i < t.childCount; i++)
        {
            SaveTransformRecursive(t.GetChild(i));
        }
    }

    [Button("Scale Path to Screen")]
    public void ScalePath()
    {
        if (cam == null)
        {
            Debug.LogError("Camera reference is missing!");
            return;
        }
        
        if (!originalsSaved)
        {
            SaveOriginalTransforms();
        }

        // Calculate current aspect ratio
        float currentAspectRatio = (float)cam.pixelWidth / cam.pixelHeight;
        
        // Calculate scale factors
        float aspectRatioDifference = currentAspectRatio / referenceAspectRatio;
        float horizontalScaleFactor = scaleHorizontally ? aspectRatioDifference : 1f;
        float verticalScaleFactor = scaleVertically ? (1f / aspectRatioDifference) : 1f;
        
        // Distribute lanes evenly if enabled
        if (distributeLanesEvenly)
        {
            DistributeLanesAcrossScreen();
        }
        else
        {
            // Apply scaling to all path roots without distribution
            if (pathRoots != null)
            {
                foreach (Transform pathRoot in pathRoots)
                {
                    if (pathRoot != null && originalTransforms.ContainsKey(pathRoot))
                    {
                        // Restore original first
                        TransformData original = originalTransforms[pathRoot];
                        pathRoot.localPosition = original.localPosition;
                        pathRoot.localRotation = original.localRotation;
                        
                        // Apply new scale
                        Vector3 newScale = original.localScale;
                        newScale.x *= horizontalScaleFactor;
                        newScale.y *= verticalScaleFactor;
                        pathRoot.localScale = newScale;
                        
                        // Also restore all children to their originals
                        RestoreChildrenRecursive(pathRoot);
                    }
                }
            }
        }
        
        // Scale hit area
        if (hitAreaPlane != null && originalTransforms.ContainsKey(hitAreaPlane))
        {
            TransformData original = originalTransforms[hitAreaPlane];
            hitAreaPlane.localPosition = original.localPosition;
            hitAreaPlane.localRotation = original.localRotation;
            
            Vector3 newScale = original.localScale;
            newScale.x *= horizontalScaleFactor;
            newScale.z *= horizontalScaleFactor; // Z for plane depth
            hitAreaPlane.localScale = newScale;
        }
        
        Debug.Log($"Scaled paths - Aspect: {currentAspectRatio:F3} (ref: {referenceAspectRatio:F3}), H-Scale: {horizontalScaleFactor:F3}, V-Scale: {verticalScaleFactor:F3}");
    }
    
    private void DistributeLanesAcrossScreen()
    {
        if (pathRoots == null || pathRoots.Length == 0) return;
        
        // Get visible world width at the distribution depth
        float visibleWidth = GetWorldWidthAtDistance(laneDistributionDepth);
        
        // Collect all NotePaths from all NotePathCollections
        List<NotePath> allPaths = new List<NotePath>();
        foreach (Transform pathRoot in pathRoots)
        {
            if (pathRoot != null)
            {
                NotePathCollection collection = pathRoot.GetComponent<NotePathCollection>();
                if (collection != null && collection.paths != null)
                {
                    // Filter out null paths
                    foreach (NotePath path in collection.paths)
                    {
                        if (path != null)
                        {
                            allPaths.Add(path);
                        }
                    }
                }
            }
        }
        
        if (allPaths.Count == 0)
        {
            Debug.LogWarning("No valid NotePaths found to distribute");
            return;
        }
        
        // n paths need n+2 slots (1 buffer on each side)
        int numPaths = allPaths.Count;
        int numSlots = numPaths + 2;
        
        // Calculate slot width and starting position
        float slotWidth = visibleWidth / numSlots;
        float startX = -visibleWidth / 2f + slotWidth; // Start at slot 1 (skip slot 0)
        
        // Sort paths by lane index to ensure consistent ordering (with null check)
        allPaths.Sort((a, b) => 
        {
            if (a == null && b == null) return 0;
            if (a == null) return 1;
            if (b == null) return -1;
            return a.laneIndex.CompareTo(b.laneIndex);
        });
        
        // Position each path in its slot
        for (int i = 0; i < numPaths; i++)
        {
            NotePath path = allPaths[i];
            if (path == null) continue;
            
            Transform pathTransform = path.transform;
            
            if (originalTransforms.ContainsKey(pathTransform))
            {
                TransformData original = originalTransforms[pathTransform];
                
                // Calculate target X position (center of slot i+1, since we skip slot 0)
                float targetX = startX + (i * slotWidth);
                
                // Restore original transform
                pathTransform.localPosition = original.localPosition;
                pathTransform.localRotation = original.localRotation;
                pathTransform.localScale = original.localScale;
                
                // Apply horizontal positioning
                Vector3 newPos = pathTransform.localPosition;
                newPos.x = targetX;
                pathTransform.localPosition = newPos;
                
                // Restore all children to their originals
                RestoreChildrenRecursive(pathTransform);
            }
        }
    }
    
    private float GetWorldWidthAtDistance(float distance)
    {
        if (cam.orthographic)
        {
            return cam.orthographicSize * 2f * cam.aspect;
        }
        else
        {
            float frustumHeight = 2f * distance * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
            return frustumHeight * cam.aspect;
        }
    }
    
    private void RestoreChildrenRecursive(Transform parent)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (originalTransforms.ContainsKey(child))
            {
                originalTransforms[child].ApplyTo(child);
            }
            
            if (child.childCount > 0)
            {
                RestoreChildrenRecursive(child);
            }
        }
    }

    [Button("Reset to Original Scale")]
    public void ResetScale()
    {
        if (!originalsSaved)
        {
            Debug.LogWarning("No original transforms saved to reset to!");
            return;
        }
        
        // Restore all saved transforms
        foreach (var kvp in originalTransforms)
        {
            if (kvp.Key != null)
            {
                kvp.Value.ApplyTo(kvp.Key);
            }
        }
        
        Debug.Log("Reset all paths to original scale");
    }


#if UNITY_EDITOR
    [Button("Auto-Configure from Scene")]
    private void AutoConfigure()
    {
        // Try to find camera if not set
        if (cam == null)
        {
            cam = Camera.main;
            if (cam == null)
            {
                cam = FindFirstObjectByType<Camera>();
            }
        }
        
        // Try to find path collections
        if (pathRoots == null || pathRoots.Length == 0)
        {
            NotePathCollection[] collections = FindObjectsByType<NotePathCollection>(FindObjectsSortMode.None);
            if (collections.Length > 0)
            {
                pathRoots = new Transform[collections.Length];
                for (int i = 0; i < collections.Length; i++)
                {
                    pathRoots[i] = collections[i].transform;
                }
                Debug.Log($"Found {collections.Length} path collections");
            }
        }
        
        // Try to find hit area plane
        if (hitAreaPlane == null)
        {
            GameObject hitObj = GameObject.Find("HitArea");
            if (hitObj == null)
            {
                hitObj = GameObject.Find("Hit Area");
            }
            if (hitObj == null)
            {
                hitObj = GameObject.Find("HitPlane");
            }
            if (hitObj != null)
            {
                hitAreaPlane = hitObj.transform;
                Debug.Log($"Found hit area: {hitObj.name}");
            }
        }
        
        // Save the current state as the reference (16:9)
        SaveOriginalTransforms();
        
        EditorUtility.SetDirty(this);
        Debug.Log("Auto-configuration complete!");
    }
#endif
}

