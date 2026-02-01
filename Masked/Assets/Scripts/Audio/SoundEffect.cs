using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Sound Effect")]
public class SoundEffect : ScriptableObject
{
    public AudioClip clip;

    [Header("Playback")]
    [Range(0f, 1f)] public float volume = 1f;
    public float cooldown = 0f;
    public int maxSimultaneous = 4;
    public bool important = false;

    [Header("Pitch Variation")]
    public float minPitch = 1f;
    public float maxPitch = 1f;
}