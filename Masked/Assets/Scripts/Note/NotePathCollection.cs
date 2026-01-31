using Sirenix.OdinInspector;
using UnityEngine;

public class NotePathCollection : MonoBehaviour
{
    public NotePath[] paths; // array of note paths for a certain spawn row

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
}