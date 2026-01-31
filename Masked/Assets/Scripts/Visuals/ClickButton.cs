using Extensions.EventBus;
using UnityEngine;
using PrimeTween;

public class ClickButton : MonoBehaviour
{
    [Header("Lane")]
    public int laneIndex;

    [Header("Pulse Settings")]
    public Vector3 pressedScale = new Vector3(1f, 1f, 0.85f);
    
    public float scaleInDuration = 0.05f;
    public float scaleOutDuration = 0.07f;
    public Ease scaleInEase = Ease.OutQuad;
    public Ease scaleOutEase = Ease.InQuad;

    [Header("Afterimage")]
    public GameObject bitAfterimagePrefab;
    public Vector3 afterimageOffset = new Vector3(0f, 0f, -0.2f);
    public Vector3 afterimageScaleTarget = new Vector3(1.5f, 1.5f, 1.5f);
    public float afterimageLifetime = 0.25f;
    public float afterimageRiseDistance = 0.15f;
    
    private static readonly int bitValueID = Shader.PropertyToID("_BitValue");
    private static readonly int bitAlphaID = Shader.PropertyToID("_BitAlpha");
    private static readonly int emissionColorID = Shader.PropertyToID("_EmissionColor");
    

    private EventBinding<ButtonPressedEvent> buttonPressedEventBinding;

    private Sequence scaleTween;
    private Tween emissionTween;

    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = transform.localScale;

        buttonPressedEventBinding = new EventBinding<ButtonPressedEvent>(OnButtonPressed);
        EventBus<ButtonPressedEvent>.Register(buttonPressedEventBinding);
    }

    private void OnDestroy()
    {
        EventBus<ButtonPressedEvent>.Deregister(buttonPressedEventBinding);
    }

    private void OnButtonPressed(ButtonPressedEvent e)
    {
        if (e.buttonIndex != laneIndex)
            return;

        PulseButton();
        SpawnBitAfterimage(Mathf.RoundToInt(e.direction.y) == 1);
    }

    private void PulseButton()
    {
        scaleTween.Stop();

        Transform t = transform;
        
        Vector3 scaleTarget = new Vector3(pressedScale.x * originalScale.x,
            pressedScale.y * originalScale.y,
            pressedScale.z * originalScale.z);
        
         Vector3 scaleOriginal = new Vector3(originalScale.x,
            originalScale.y,
            originalScale.z);

        scaleTween = Sequence.Create()
            .Chain(Tween.Scale(t, scaleTarget, scaleInDuration, scaleInEase))
            .Chain(Tween.Scale(t, scaleOriginal, scaleOutDuration, scaleOutEase));
    }

    private void SpawnBitAfterimage(bool bitValue)
    {
        if (bitAfterimagePrefab == null)
            return;

        GameObject go = Instantiate(
            bitAfterimagePrefab,
            transform.position + afterimageOffset,
            Quaternion.identity
        );

        BitAfterImage bitAfterImage = go.GetComponent<BitAfterImage>();

        foreach (var meshRenderer in bitAfterImage.allRenderers)
        {
            meshRenderer.material = Instantiate(meshRenderer.material);
        }
        
        foreach (var meshRenderer in bitAfterImage.materialsForBitUpdate)
        {
            meshRenderer.material.SetInt(bitValueID, bitValue ? 1 : 0);
        }

        foreach (var meshRenderer in bitAfterImage.materialsForEmissionUpdate)
        {
            meshRenderer.material.SetColor(emissionColorID, BeatMapManager.Instance.materialColorShifter.GetCurrentLogicData().baseColor);
        }

        Transform t = go.transform;
        Vector3 startPos = t.position;
        Vector3 endPos = startPos + Vector3.up * afterimageRiseDistance * (bitValue ? 1f : -1f); // Move up for 1 bit, down for 0 bit
        Vector3 targetScale = new Vector3(t.localScale.x * afterimageScaleTarget.x,
            t.localScale.y * afterimageScaleTarget.y, 
            t.localScale.z * afterimageScaleTarget.z);

        foreach (var meshRenderer in bitAfterImage.materialsForAlphaUpdate)
        {
            Sequence.Create()
                .Chain(Tween.Position(t, endPos, afterimageLifetime, Ease.OutQuad))
                .Chain(Tween.Scale(t, targetScale, afterimageLifetime, Ease.OutQuad))
                .Chain(Tween.MaterialProperty(meshRenderer.material, bitAlphaID, 0f, afterimageLifetime, Ease.InQuad))
                .OnComplete(() => Destroy(go));
        }
    }
}