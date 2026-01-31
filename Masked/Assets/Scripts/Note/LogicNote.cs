using System;
using Extensions.EventBus;
using UnityEngine;

public class LogicNote : MonoBehaviour, IPoolable
{
    [Header("Logic Note Settings")]
    public int noteID; // the notes id in the music track
    public int beatStamp; // the beat at which the note should be hit
    public int laneIndex; // the lane index this note is assigned to
    public int parity; // whether the note is a 0 bit or a 1 bit
    
    private bool active = false;
    public bool? successfullyHit;
    public bool broadcastingNote = false;

    /**
     * <summary>
     * Moves the note along its path using the provided moveAction and returning to the pool upon completion.
     * </summary>
     */
    public void MoveNote(int beat, int lane, int noteParity, Action<Transform, Action> moveAction)
    {
        if (!active) return;

        gameObject.SetActive(true);
        beatStamp = beat;
        laneIndex = lane;
        parity = noteParity;
        
        BeatMapManager.Instance.RegisterLogicNote(this);
        
        moveAction?.Invoke(transform, ReturnToPool);
    }
    
    private void ReturnToPool()
    {
        BeatMapManager.Instance.UnregisterLogicNote(this);
        
        if (successfullyHit == null)
            EventBus<LogicNoteHitEvent>.Raise(new LogicNoteHitEvent(this, false)); // Missed note TODO: Broadcasts twice when fully missed?
        else if (broadcastingNote)
            EventBus<LogicNoteHitEvent>.Raise(new LogicNoteHitEvent(this, successfullyHit ?? false)); // Broadcast hit/miss
        
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