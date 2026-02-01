using Extensions.EventBus;
using PrimeTween;
using UnityEngine;

public class CompositedNoteEffect : MonoBehaviour
{
    public GameObject notePrefab;
    
    public Transform[] noteSpawnPoints;
    public float zOffset;
    
    public EventBinding<NoteReachedEvent> noteReachedEventBinding;
    
    private static readonly int bitValueID = Shader.PropertyToID("_BitValue");
    private static readonly int bitAlphaID = Shader.PropertyToID("_BitAlpha");
    private static readonly int emissionColorID = Shader.PropertyToID("_EmissionColor");
    private static readonly int baseColorID = Shader.PropertyToID("_Color");
    
    private void Awake()
    {
        noteReachedEventBinding = new EventBinding<NoteReachedEvent>(OnNoteReached);
        EventBus<NoteReachedEvent>.Register(noteReachedEventBinding);
    }
    
    private void OnDestroy()
    {
        EventBus<NoteReachedEvent>.Deregister(noteReachedEventBinding);
    }
    
    private void OnNoteReached(NoteReachedEvent e)
    {
        if (e.laneIndex < 0 || e.laneIndex >= noteSpawnPoints.Length)
            return;
        
        Transform spawnPoint = noteSpawnPoints[e.laneIndex];
        Vector3 spawnPosition = spawnPoint.position + new Vector3(0f, 0f, zOffset);
        
        GameObject noteObject = Instantiate(notePrefab, spawnPosition, Quaternion.identity);
        BitAfterImage bitAfterImage = noteObject.GetComponent<BitAfterImage>();
        
        foreach (var meshRenderer in bitAfterImage.allRenderers)
        {
            var color = e.successfullyHit
                ? BeatMapManager.Instance.materialColorShifter.GetCurrentLogicData().InvertedNeonColor
                : Color.black;
            meshRenderer.material.SetColor(baseColorID, color);
        }
        
        foreach (var meshRenderer in bitAfterImage.allRenderers)
        {
            meshRenderer.material = Instantiate(meshRenderer.material);
        }
        
        foreach (var meshRenderer in bitAfterImage.materialsForBitUpdate)
        {
            meshRenderer.material.SetInt(bitValueID, e.truthValue);
        }
        
        foreach (var meshRenderer in bitAfterImage.materialsForAlphaUpdate)
        {
            Sequence.Create()
                .ChainDelay(e.lifeTime - 0.2f)
                .Chain(Tween.MaterialProperty(meshRenderer.material, bitAlphaID, 0f, 0.2f, Ease.InQuad))
                .OnComplete(() => Destroy(noteObject));
        }
    }
    
}

public struct NoteReachedEvent : IEvent
{
    public int laneIndex;
    public int truthValue;
    public float lifeTime;
    public bool successfullyHit;
    
    public NoteReachedEvent(int laneIndex, int truthValue, float lifeTime, bool successfullyHit)
    {
        this.laneIndex = laneIndex;
        this.truthValue = truthValue;
        this.lifeTime = lifeTime;
        this.successfullyHit = successfullyHit;
    }
}