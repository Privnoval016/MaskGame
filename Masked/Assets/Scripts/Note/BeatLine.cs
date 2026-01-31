using System;
using Extensions.EventBus;
using UnityEngine;

public class BeatLine : MonoBehaviour, IPoolable
{
    [Header("Logic Note Settings")]
    public int noteID; // the notes id in the music track
    public float beatStamp; // the beat at which the note should be hit
    public int laneIndex; // the lane index this note is assigned to

    [Header("Visual Settings")] 
    public Material regularMaterial;

    public Material superMaterial;
    private MeshRenderer meshRenderer;
    
    private bool active = false;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    /**
     * <summary>
     * Moves the note along its path using the provided moveAction and returning to the pool upon completion.
     * </summary>
     */
    public void MoveBeatLine(float beat, int lane, Action<Transform, Action> moveAction, bool isSuperLine)
    {
        if (!active) return;

        gameObject.SetActive(true);
        beatStamp = beat;
        laneIndex = lane;
        
        // Set material based on whether it's a super line
        if (meshRenderer != null)
        {
            meshRenderer.material = isSuperLine ? superMaterial : regularMaterial;
        }
        
        moveAction?.Invoke(transform, ReturnToPool);
    }
    
    private void ReturnToPool()
    {
        BeatLinePool.Instance.Return(this);
    }

    public void OnSpawn()
    {
        active = true;
    }
    
    public void OnDespawn()
    {
        active = false;
        gameObject.SetActive(false);
    }
}