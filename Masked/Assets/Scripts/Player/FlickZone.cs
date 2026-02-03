using Extensions.EventBus;
using UnityEngine;
using PrimeTween;

/// <summary>
/// Defines a zone where flick input is detected.
/// Attach this to button GameObjects with colliders.
/// The collider should be on the "FlickZone" layer.
/// </summary>
[RequireComponent(typeof(Collider))]
public class FlickZone : MonoBehaviour
{
    [Header("Lane Assignment")]
    [Tooltip("Which lane/button this zone represents (0-3)")]
    public int laneIndex;
    
    [Header("Visual Feedback")]
    [Tooltip("Material to apply color feedback (optional)")]
    public Renderer feedbackRenderer;
    
    [Tooltip("Color for upward flick feedback")]
    public Color flickUpColor = new Color(0.3f, 1f, 0.3f); // Green
    
    [Tooltip("Color for downward flick feedback")]
    public Color flickDownColor = new Color(1f, 0.3f, 0.3f); // Red
    
    [Tooltip("Duration of color flash feedback")]
    public float feedbackDuration = 0.2f;
    
    [Header("State")]
    [Tooltip("Enable/disable flick detection for this zone")]
    public bool IsEnabled = true;
    
    private EventBinding<FlickInputEvent> flickEventBinding;
    private Color originalColor;
    private Tween colorTween;
    private static readonly int emissionColorID = Shader.PropertyToID("_EmissionColor");
    private MaterialPropertyBlock propertyBlock;
    
    private void Awake()
    {
        // Ensure the GameObject is on the FlickZone layer
        if (gameObject.layer != LayerMask.NameToLayer("FlickZone"))
        {
            Debug.LogWarning($"FlickZone on {gameObject.name} is not on 'FlickZone' layer. Auto-assigning...");
            gameObject.layer = LayerMask.NameToLayer("FlickZone");
        }
        
        // Ensure collider exists and is trigger
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true; // Flick zones should be triggers
        }
        
        // Setup material property block for efficient color changes
        if (feedbackRenderer != null)
        {
            propertyBlock = new MaterialPropertyBlock();
            feedbackRenderer.GetPropertyBlock(propertyBlock);
            
            // Try to get original emission color
            if (propertyBlock.HasProperty(emissionColorID))
            {
                originalColor = propertyBlock.GetColor(emissionColorID);
            }
            else
            {
                originalColor = Color.white;
            }
        }
        
        // Register for flick events
        flickEventBinding = new EventBinding<FlickInputEvent>(OnFlickInput);
        EventBus<FlickInputEvent>.Register(flickEventBinding);
    }
    
    private void OnDestroy()
    {
        EventBus<FlickInputEvent>.Deregister(flickEventBinding);
        colorTween.Stop();
    }
    
    /// <summary>
    /// Handle flick input event - provide visual feedback if it's for this zone
    /// </summary>
    private void OnFlickInput(FlickInputEvent e)
    {
        // Only respond to flicks in this zone
        if (e.laneIndex != laneIndex)
            return;
        
        // Provide visual feedback
        ShowFlickFeedback(e.direction);
        
        // Forward to game logic as button press with direction
        // This integrates with your existing ButtonPressedEvent system
        Vector2 direction = e.flickValue == 1 ? Vector2.up : Vector2.down;
        
        // Play sound effect
        if (SoundEffectManager.Instance != null)
        {
            SoundEffectManager.Instance.Play(SoundEffectManager.Instance.soundEffectAtlas.buttonHit);
        }
        
        // Raise button pressed event (integrates with existing system)
        if (GameManager.Instance?.livePlayData?.autoPlayEnabled != true)
        {
            EventBus<ButtonPressedEvent>.Raise(new ButtonPressedEvent(laneIndex, direction));
        }
    }
    
    /// <summary>
    /// Show visual feedback for flick direction
    /// </summary>
    private void ShowFlickFeedback(FlickDirection direction)
    {
        if (feedbackRenderer == null || propertyBlock == null)
            return;
        
        colorTween.Stop();
        
        Color targetColor = direction == FlickDirection.Up ? flickUpColor : flickDownColor;
        
        // Flash the emission color
        propertyBlock.SetColor(emissionColorID, targetColor);
        feedbackRenderer.SetPropertyBlock(propertyBlock);
        
        // Tween back to original color
        colorTween = Tween.Custom(0f, 1f, feedbackDuration, onValueChange: t =>
        {
            Color currentColor = Color.Lerp(targetColor, originalColor, t);
            propertyBlock.SetColor(emissionColorID, currentColor);
            feedbackRenderer.SetPropertyBlock(propertyBlock);
        }, ease: Ease.OutQuad);
    }
    
    /// <summary>
    /// Enable flick detection for this zone
    /// </summary>
    public void Enable()
    {
        IsEnabled = true;
    }
    
    /// <summary>
    /// Disable flick detection for this zone
    /// </summary>
    public void Disable()
    {
        IsEnabled = false;
    }
    
    private void OnDrawGizmos()
    {
        // Visualize the flick zone in editor
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = IsEnabled ? new Color(0f, 1f, 0f, 0.3f) : new Color(1f, 0f, 0f, 0.3f);
            Gizmos.matrix = transform.localToWorldMatrix;
            
            if (col is BoxCollider box)
            {
                Gizmos.DrawCube(box.center, box.size);
            }
            else if (col is SphereCollider sphere)
            {
                Gizmos.DrawSphere(sphere.center, sphere.radius);
            }
        }
    }
}

