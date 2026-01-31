using System;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// ScriptableObject config mapping SoundType to AudioClip
/// </summary>
[CreateAssetMenu(fileName = "SoundConfig", menuName = "Audio/Sound Config")]
public class SoundConfig : ScriptableObject
{
    [Serializable]
    public class SoundMapping
    {
        [HorizontalGroup, HideLabel]
        public SoundType soundType;
        [HorizontalGroup, HideLabel]
        public AudioClip audioClip;
        [Range(0f, 1f)]
        public float volume = 1f;
    }

    [Header("Sound Mappings")]
    [ListDrawerSettings(DraggableItems = false)]
    public SoundMapping[] soundMappings;

    public AudioClip GetClip(SoundType type)
    {
        foreach (var mapping in soundMappings)
        {
            if (mapping.soundType == type)
                return mapping.audioClip;
        }

        Debug.LogWarning($"[SoundConfig] No AudioClip found for {type}");
        return null;
    }

    public float GetVolume(SoundType type)
    {
        foreach (var mapping in soundMappings)
        {
            if (mapping.soundType == type)
                return mapping.volume;
        }

        return 1f;
    }
}
