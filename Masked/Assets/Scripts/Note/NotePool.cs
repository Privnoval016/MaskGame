using Extensions.Patterns;

using System.Collections.Generic;
using UnityEngine;

public class NotePool : Singleton<NotePool>
{

    [SerializeField] private LogicNote notePrefab;
    [SerializeField] private int initialSize = 64;

    private Queue<LogicNote> pool = new Queue<LogicNote>();

    protected override void Awake()
    {
        base.Awake();
        
        for (int i = 0; i < initialSize; i++)
        {
            CreateNewNote();
        }
    }

    private LogicNote CreateNewNote()
    {
        LogicNote note = Instantiate(notePrefab, transform);
        note.gameObject.SetActive(false);
        pool.Enqueue(note);
        return note;
    }

    public LogicNote Get()
    {
        if (pool.Count == 0)
            CreateNewNote();

        LogicNote note = pool.Dequeue();
        note.OnSpawn();
        return note;
    }

    public void Return(LogicNote note)
    {
        note.OnDespawn();
        if (!pool.Contains(note))
            pool.Enqueue(note);
    }
}

public interface IPoolable
{
    void OnSpawn();
    void OnDespawn();
}
