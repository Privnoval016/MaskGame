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
    public int realTruthValue; // the actual truth value of the note, used for scoring when the player hits a note incorrectly
    
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
    public void MoveNote(float beat, int lane, int spawn, int noteParity, int realValue, Action<Transform, Action> moveAction)
    {
        if (!active) return;

        gameObject.SetActive(true);
        beatStamp = beat;
        laneIndex = lane;
        spawnLocationIndex = spawn;
        truthValue = noteParity;
        realTruthValue = realValue;
        
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
        
        moveAction?.Invoke(transform, () => ReturnToPool(null, ScoreType.Miss));
    }
    
    public void ReturnToPool(bool? successfullyHit, ScoreType scoreType)
    {
        if (!active) return;
        
        StartCoroutine(PauseThenReturn(pauseDuration, successfullyHit, scoreType));
    }
    
    private IEnumerator PauseThenReturn(float duration, bool? successfullyHit, ScoreType scoreType)
    {
        if (successfullyHit == null)
        {
            EventBus<NoteReachedEvent>.Raise(new NoteReachedEvent(laneIndex, realTruthValue, duration, false));
        }
        else if (successfullyHit == true)
        {
            EventBus<NoteReachedEvent>.Raise(new NoteReachedEvent(laneIndex, realTruthValue, duration * 0.5f, true));
        }
        
        foreach (var meshRenderer in noteRenderers)
        {
            meshRenderer.enabled = false;
        }
        
        yield return new WaitForSeconds(successfullyHit == null ? duration : 0f); // Only pause if the note was missed
        
        if (!active) yield break; // Prevent double returning to pool
        
        BeatMapManager.Instance.UnregisterLogicNote(this);
        
        EventBus<LogicNoteHitEvent>.Raise(new LogicNoteHitEvent(this, successfullyHit, scoreType, realTruthValue));
        
        NotePool.Instance.Return(this);
    }

    public void OnSpawn()
    {
        active = true;
        foreach (var meshRenderer in noteRenderers)
        {
            meshRenderer.enabled = true;
        }
    }
    
    public void OnDespawn()
    {
        active = false;
        gameObject.SetActive(false);
    }
}