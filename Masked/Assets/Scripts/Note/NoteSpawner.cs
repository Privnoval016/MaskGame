using UnityEngine;

public class NoteSpawner : MonoBehaviour
{
    [Header("Note Spawner Settings")]
    public GameObject notePrefab; // the prefab for the notes to spawn
    public Transform spawnPoint; // the point where notes are spawned

    public void SpawnNote(int noteID, int laneIndex, NotePath path)
    {
        GameObject noteObject = Instantiate(notePrefab, spawnPoint.position, Quaternion.identity);
        LogicNote logicNote = noteObject.GetComponent<LogicNote>();
        logicNote.noteID = noteID;
        logicNote.laneIndex = laneIndex;
        logicNote.path = path;
    }
}

