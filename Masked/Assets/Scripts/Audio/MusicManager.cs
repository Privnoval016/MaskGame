using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioSource audioSource;
    
    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }
    
    public void PlayMusic(AudioClip clip, double time, bool loop = false)
    {
        if (audioSource == null || clip == null) return;
        
        Debug.Log(clip.name);
        
        audioSource.clip = clip;
        audioSource.loop = loop;
        audioSource.volume = 1f;
        audioSource.PlayScheduled(time);
    }
}