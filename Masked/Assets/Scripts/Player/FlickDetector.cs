using System.Collections.Generic;
using Extensions.EventBus;
using UnityEngine;

/// <summary>
/// Production-ready flick detection system for mobile rhythm games.
/// Detects vertical flick gestures (up/down) within defined zones.
/// Optimized for responsive feel while avoiding false positives.
/// </summary>
public class FlickDetector : MonoBehaviour
{
    [Header("Flick Detection Settings")]
    [Tooltip("Minimum distance in screen units to register as a flick")]
    public float minimumFlickDistance = 50f;
    
    [Tooltip("Maximum time window for a flick (faster = more responsive)")]
    public float maximumFlickTime = 0.3f;
    
    [Tooltip("Minimum velocity (units/second) required to register as a flick")]
    public float minimumFlickVelocity = 300f;
    
    [Tooltip("Initial dead zone to ignore micro-movements")]
    public float touchDeadZone = 10f;
    
    [Header("Angle Constraints")]
    [Tooltip("Maximum angle deviation from vertical (0-90 degrees)")]
    [Range(0f, 90f)]
    public float maxAngleDeviation = 30f;
    
    [Header("Debug")]
    public bool showDebugInfo = false;
    
    // Track individual touches for multi-touch support
    private Dictionary<int, TouchData> activeTouches = new Dictionary<int, TouchData>();
    
    // Camera for raycasting
    private Camera mainCamera;
    
    // Layer mask for flick zones
    private int flickZoneLayer;
    
    private class TouchData
    {
        public Vector2 startPosition;
        public Vector2 currentPosition;
        public float startTime;
        public bool hasExceededDeadZone;
        public FlickZone activeZone;
    }
    
    private void Awake()
    {
        mainCamera = Camera.main;
        flickZoneLayer = LayerMask.GetMask("FlickZone"); // You'll need to create this layer
    }
    
    private void Update()
    {
        // Handle both touch input (mobile) and mouse input (testing)
        if (Input.touchCount > 0)
        {
            HandleTouchInput();
        }
        else
        {
            HandleMouseInput(); // For editor testing
        }
    }
    
    /// <summary>
    /// Handle native touch input for mobile devices
    /// </summary>
    private void HandleTouchInput()
    {
        foreach (Touch touch in Input.touches)
        {
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    OnTouchBegan(touch.fingerId, touch.position);
                    break;
                    
                case TouchPhase.Moved:
                    OnTouchMoved(touch.fingerId, touch.position);
                    break;
                    
                case TouchPhase.Ended:
                    OnTouchEnded(touch.fingerId, touch.position);
                    break;
                    
                case TouchPhase.Canceled:
                    OnTouchCanceled(touch.fingerId);
                    break;
            }
        }
    }
    
    /// <summary>
    /// Handle mouse input for editor testing
    /// </summary>
    private void HandleMouseInput()
    {
        const int mouseId = -1; // Use -1 as mouse identifier
        
        if (Input.GetMouseButtonDown(0))
        {
            OnTouchBegan(mouseId, Input.mousePosition);
        }
        else if (Input.GetMouseButton(0))
        {
            OnTouchMoved(mouseId, Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            OnTouchEnded(mouseId, Input.mousePosition);
        }
    }
    
    /// <summary>
    /// Touch began - check if it's in a flick zone and start tracking
    /// </summary>
    private void OnTouchBegan(int touchId, Vector2 screenPosition)
    {
        // Check if touch is within a FlickZone
        FlickZone zone = GetFlickZoneAtPosition(screenPosition);
        
        if (zone != null && zone.IsEnabled)
        {
            // Start tracking this touch
            TouchData touchData = new TouchData
            {
                startPosition = screenPosition,
                currentPosition = screenPosition,
                startTime = Time.time,
                hasExceededDeadZone = false,
                activeZone = zone
            };
            
            activeTouches[touchId] = touchData;
            
            if (showDebugInfo)
            {
                Debug.Log($"Touch {touchId} started in zone {zone.laneIndex}");
            }
        }
    }
    
    /// <summary>
    /// Touch moved - update tracking and check for early flick detection
    /// </summary>
    private void OnTouchMoved(int touchId, Vector2 screenPosition)
    {
        if (!activeTouches.ContainsKey(touchId))
            return;
        
        TouchData touchData = activeTouches[touchId];
        touchData.currentPosition = screenPosition;
        
        // Check if we've exceeded the dead zone
        if (!touchData.hasExceededDeadZone)
        {
            float distance = Vector2.Distance(touchData.startPosition, screenPosition);
            if (distance > touchDeadZone)
            {
                touchData.hasExceededDeadZone = true;
            }
        }
    }
    
    /// <summary>
    /// Touch ended - evaluate if it was a valid flick
    /// </summary>
    private void OnTouchEnded(int touchId, Vector2 screenPosition)
    {
        if (!activeTouches.ContainsKey(touchId))
            return;
        
        TouchData touchData = activeTouches[touchId];
        touchData.currentPosition = screenPosition;
        
        // Calculate flick parameters
        Vector2 delta = screenPosition - touchData.startPosition;
        float distance = delta.magnitude;
        float duration = Time.time - touchData.startTime;
        float velocity = duration > 0 ? distance / duration : 0f;
        
        // Check if this qualifies as a flick
        if (ValidateFlick(touchData, delta, distance, duration, velocity, out FlickDirection direction))
        {
            // Valid flick detected!
            int flickValue = direction == FlickDirection.Up ? 1 : 0;
            
            if (showDebugInfo)
            {
                Debug.Log($"Flick detected! Zone: {touchData.activeZone.laneIndex}, Direction: {direction}, Value: {flickValue}, " +
                         $"Distance: {distance:F1}px, Duration: {duration:F3}s, Velocity: {velocity:F1}px/s");
            }
            
            // Raise flick event
            EventBus<FlickInputEvent>.Raise(new FlickInputEvent(
                touchData.activeZone.laneIndex,
                flickValue,
                direction,
                velocity
            ));
            
            // Provide haptic feedback (if available)
            if (SystemInfo.supportsVibration)
            {
                Handheld.Vibrate();
            }
        }
        else if (showDebugInfo && touchData.hasExceededDeadZone)
        {
            Debug.Log($"Touch ended but didn't qualify as flick. Distance: {distance:F1}px, Duration: {duration:F3}s, Velocity: {velocity:F1}px/s");
        }
        
        // Clean up tracking
        activeTouches.Remove(touchId);
    }
    
    /// <summary>
    /// Touch canceled - cleanup
    /// </summary>
    private void OnTouchCanceled(int touchId)
    {
        activeTouches.Remove(touchId);
    }
    
    /// <summary>
    /// Validate if the touch qualifies as a flick based on all criteria
    /// </summary>
    private bool ValidateFlick(TouchData touchData, Vector2 delta, float distance, float duration, float velocity, out FlickDirection direction)
    {
        direction = FlickDirection.None;
        
        // 1. Must have exceeded dead zone
        if (!touchData.hasExceededDeadZone)
            return false;
        
        // 2. Check minimum distance
        if (distance < minimumFlickDistance)
            return false;
        
        // 3. Check time window (must be quick)
        if (duration > maximumFlickTime)
            return false;
        
        // 4. Check velocity threshold
        if (velocity < minimumFlickVelocity)
            return false;
        
        // 5. Check angle - must be primarily vertical
        float angle = Vector2.Angle(delta, Vector2.up);
        float angleDown = Vector2.Angle(delta, Vector2.down);
        
        bool isVertical = angle <= maxAngleDeviation || angleDown <= maxAngleDeviation;
        if (!isVertical)
            return false;
        
        // 6. Determine direction (up or down)
        if (angle <= maxAngleDeviation)
        {
            direction = FlickDirection.Up;
            return true;
        }
        else if (angleDown <= maxAngleDeviation)
        {
            direction = FlickDirection.Down;
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Raycast to find FlickZone at screen position
    /// </summary>
    private FlickZone GetFlickZoneAtPosition(Vector2 screenPosition)
    {
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        RaycastHit hit;
        
        // Raycast against FlickZone layer
        if (Physics.Raycast(ray, out hit, 100f, flickZoneLayer))
        {
            return hit.collider.GetComponent<FlickZone>();
        }
        
        return null;
    }
    
    private void OnDestroy()
    {
        activeTouches.Clear();
    }
}

/// <summary>
/// Event raised when a valid flick is detected
/// </summary>
public struct FlickInputEvent : IEvent
{
    public int laneIndex;      // Which button/lane (0-3)
    public int flickValue;     // 0 for down, 1 for up
    public FlickDirection direction;
    public float velocity;     // Flick velocity for potential visual feedback
    
    public FlickInputEvent(int laneIndex, int flickValue, FlickDirection direction, float velocity)
    {
        this.laneIndex = laneIndex;
        this.flickValue = flickValue;
        this.direction = direction;
        this.velocity = velocity;
    }
}

/// <summary>
/// Flick direction enum
/// </summary>
public enum FlickDirection
{
    None,
    Up,
    Down
}

