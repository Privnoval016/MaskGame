using Extensions.Patterns;
using UnityEngine;

public class SoundEffectManager : Singleton<SoundEffectManager>
{
    public AudioSource audioSource;
    [Header("Hit Sounds")] 
    public AudioClip perfectHit;
    public AudioClip goodHit;
    public AudioClip missHit;
    
    [Header("Other Sounds")]
    public AudioClip buttonClick;
    public AudioClip comboBreak;
    public AudioClip levelComplete;
    public AudioClip niceCombo;
    public AudioClip operationChange;
    
    protected override void Awake()
    {
        base.Awake();
        
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }
    
    public void PlaySoundEffect(AudioClip clip)
    {
        if (audioSource == null || clip == null) return;
        
        audioSource.PlayOneShot(clip);
    }
    
}