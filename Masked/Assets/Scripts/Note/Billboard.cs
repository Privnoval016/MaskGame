using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
    }

    private void LateUpdate()
    {
        if (cam == null) return;

        // Make the note billboard face the camera
        Vector3 direction = transform.position - cam.transform.position;
        direction.y = 0; // Keep only the horizontal direction
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = targetRotation;
        }
    }
}