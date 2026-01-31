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
    
    public void PlayMusic(AudioClip clip, bool loop = false)
    {
        if (audioSource == null || clip == null) return;
        
        audioSource.clip = clip;
        audioSource.loop = loop;
        audioSource.Play();
    }
}