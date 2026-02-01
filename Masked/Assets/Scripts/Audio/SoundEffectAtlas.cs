using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundEffectAtlas", menuName = "Audio/Sound Effect Atlas", order = 0)]
public class SoundEffectAtlas : ScriptableObject
{
    [Header("Hit Sounds")] 
    public SoundEffect buttonHit;

    public SoundEffect correctHitOne;
    public SoundEffect correctHitZero;

    public SoundEffect incorrectHit;
    public SoundEffect missHit;
    
    [Header("Other Sounds")]
    public SoundEffect finishedSong;
    public SoundEffect gameOver;
    public SoundEffect operationChange;
    
    [Header("UI Sounds")]
    public SoundEffect uiSelect;

    public SoundEffect songBegin;
}