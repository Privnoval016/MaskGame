using Extensions.Patterns;
using UnityEngine;
using PrimeTween;

public class MusicManager : Singleton<MusicManager>
{
    public AudioSource audioSource;
    
    protected override void Awake()
    {
        base.Awake();
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
        audioSource.pitch = 1f; // Ensure pitch starts at 1
        audioSource.PlayScheduled(time);
    }
    
    /// <summary>
    /// Slow down the music with a pitch down effect over a duration
    /// </summary>
    public void SlowDownMusic(float duration, float targetPitch = 0f)
    {
        if (audioSource == null) return;
        
        // Tween pitch from current value to target (usually 0)
        Tween.Custom(audioSource.pitch, targetPitch, duration, onValueChange: pitch =>
        {
            if (audioSource != null)
            {
                audioSource.pitch = pitch;
            }
        }, ease: Ease.OutQuad, useUnscaledTime: true);
    }
    
    /// <summary>
    /// Stop the music immediately
    /// </summary>
    public void StopMusic()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.pitch = 1f; // Reset pitch
        }
    }
}

