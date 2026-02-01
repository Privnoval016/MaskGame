using System;
using System.Collections;
using Extensions.EventBus;
using UnityEngine;

public class LogicNote : MonoBehaviour, IPoolable
{
    [Header("Logic Note Settings")]
    public int noteID; // the notes id in the music track
    public float beatStamp; // the beat at which the note should be hit
    public int laneIndex; // the lane index this note is assigned to
    public int spawnLocationIndex; // the spawn location index this note is assigned to
    public int truthValue; // whether the note is a 0 bit or a 1 bit
    
    float pauseDuration = 0.2f; // brief pause before returning to pool to account for hitting the note slightly early or late
    
    private bool active = false;

    public MeshRenderer[] noteRenderers;
    public Material[] zeroMaterials;
    public Material[] oneMaterials;

    /**
     * <summary>
     * Moves the note along its path using the provided moveAction and returning to the pool upon completion.
     * </summary>
     */
    public void MoveNote(float beat, int lane, int spawn, int noteParity, Action<Transform, Action> moveAction)
    {
        if (!active) return;

        gameObject.SetActive(true);
        beatStamp = beat;
        laneIndex = lane;
        spawnLocationIndex = spawn;
        truthValue = noteParity;

        // Set material based on truth value, assigning index 0 to meshrenderer index 0, etc.
        for (int i = 0; i < noteRenderers.Length; i++)
        {
            if (truthValue == 0)
            {
                noteRenderers[i].material = zeroMaterials[i];
            }
            else
            {
                noteRenderers[i].material = oneMaterials[i];
            }
        }
        
        
        BeatMapManager.Instance.RegisterLogicNote(this);
        
        moveAction?.Invoke(transform, () => ReturnToPool(null, ScoreType.Miss, -1));
    }
    
    public void ReturnToPool(bool? successfullyHit, ScoreType scoreType, int actualValue)
    {
        if (!active) return;
        
        StartCoroutine(PauseThenReturn(pauseDuration, successfullyHit, scoreType, actualValue));
    }
    
    private IEnumerator PauseThenReturn(float duration, bool? successfullyHit, ScoreType scoreType, int actualValue)
    {
        yield return new WaitForSeconds(successfullyHit == null ? duration : 0f); // Only pause if the note was missed
        
        if (!active) yield break; // Prevent double returning to pool
        
        BeatMapManager.Instance.UnregisterLogicNote(this);
        
        EventBus<LogicNoteHitEvent>.Raise(new LogicNoteHitEvent(this, successfullyHit, scoreType, actualValue));
        
        NotePool.Instance.Return(this);
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