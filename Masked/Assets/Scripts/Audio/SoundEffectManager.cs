using Extensions.Patterns;
using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

public class SoundEffectManager : Singleton<SoundEffectManager>
{
    public SoundEffectAtlas soundEffectAtlas;
    [Header("Mixer")]
    public AudioMixerGroup sfxMixer;

    [Header("Pool")]
    public int poolSize = 12;

    private AudioSource[] sources;
    private int sourceIndex;

    private Dictionary<SoundEffect, float> lastPlayTime = new();
    private int activePlaysThisFrame;

    protected override void Awake()
    {
        base.Awake();

        sources = new AudioSource[poolSize];
        for (int i = 0; i < poolSize; i++)
        {
            var src = gameObject.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = false;
            src.spatialBlend = 0f;
            src.outputAudioMixerGroup = sfxMixer;
            sources[i] = src;
        }
    }

    private AudioSource GetNextSource()
    {
        var src = sources[sourceIndex];
        sourceIndex = (sourceIndex + 1) % poolSize;
        return src;
    }

    public void Play(SoundEffect sfx)
    {
        if (sfx == null || sfx.clip == null) return;

        float now = Time.unscaledTime;

        // Cooldown
        if (sfx.cooldown > 0f &&
            lastPlayTime.TryGetValue(sfx, out float last) &&
            now - last < sfx.cooldown)
            return;

        // Max simultaneous
        int playing = 0;
        foreach (var src in sources)
        {
            if (src.isPlaying && src.clip == sfx.clip)
                playing++;
        }

        if (!sfx.important && playing >= sfx.maxSimultaneous)
            return;

        lastPlayTime[sfx] = now;

        // Density attenuation
        float densityFactor = Mathf.Clamp01(1f - activePlaysThisFrame * 0.08f);

        var source = GetNextSource();
        source.clip = sfx.clip;
        source.volume = sfx.volume * densityFactor;
        source.pitch = Random.Range(sfx.minPitch, sfx.maxPitch);
        source.Play();

        activePlaysThisFrame++;
    }

    private void LateUpdate()
    {
        activePlaysThisFrame = 0;
    }
}