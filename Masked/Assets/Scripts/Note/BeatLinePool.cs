using System.Collections.Generic;
using Extensions.Patterns;
using UnityEngine;

public class BeatLinePool : Singleton<BeatLinePool>
{
    [SerializeField] private BeatLine notePrefab;
    [SerializeField] private int initialSize = 64;

    private Queue<BeatLine> pool = new Queue<BeatLine>();

    protected override void Awake()
    {
        base.Awake();
        
        for (int i = 0; i < initialSize; i++)
        {
            CreateNewNote();
        }
    }

    private BeatLine CreateNewNote()
    {
        BeatLine note = Instantiate(notePrefab, transform);
        note.gameObject.SetActive(false);
        pool.Enqueue(note);
        return note;
    }

    public BeatLine Get()
    {
        if (pool.Count == 0)
            CreateNewNote();

        BeatLine note = pool.Dequeue();
        note.OnSpawn();
        return note;
    }

    public void Return(BeatLine note)
    {
        note.OnDespawn();
        if (!pool.Contains(note))
            pool.Enqueue(note);
    }
}