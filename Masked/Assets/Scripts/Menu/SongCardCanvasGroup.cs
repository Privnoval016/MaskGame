using UnityEngine;

/// <summary>
/// Automatically ensures a CanvasGroup is present on the SongCard for fade animations
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class SongCardCanvasGroup : MonoBehaviour
{
    private void Awake()
    {
        // Ensure canvas group exists
        if (!TryGetComponent<CanvasGroup>(out _))
        {
            gameObject.AddComponent<CanvasGroup>();
        }
    }
}

