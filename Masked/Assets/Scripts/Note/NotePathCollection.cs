using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class NotePathCollection : MonoBehaviour
{
    [Header("Paths")]
    public NotePath[] paths; // array of note paths for a certain spawn row
    
    [Header("Mesh Settings")]
    public float width = 0.5f;
    public Vector3 referenceNormal = Vector3.up; // reference normal
    public bool generateMeshOnAwake = true;

    private void Awake()
    {
        HideMeshesInChildren();
        
        if (generateMeshOnAwake)
        {
            GeneratePathBetweenWaypoints();
        }
    }

    [Button("Hide Meshes In Children")]
    private void HideMeshesInChildren()
    {
        MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();
        foreach (var mr in meshRenderers)
        {
            mr.enabled = false;
        }
    }
    
    [Button("Show Meshes In Children")]
    private void ShowMeshesInChildren()
    {
        MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();
        foreach (var mr in meshRenderers)
        {
            mr.enabled = true;
        }
    }

    [Button("Generate Path Between Waypoints")]
    private void GeneratePathBetweenWaypoints()
    {
        foreach (var path in paths)
        {
            path.GeneratePathBetweenWaypoints(width, referenceNormal);
        }
    }


}