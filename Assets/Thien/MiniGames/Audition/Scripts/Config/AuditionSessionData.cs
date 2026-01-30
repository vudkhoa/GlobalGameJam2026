using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SRP: Store game session (list of phases)
/// Responsibility: Hold phase references, provide default audio
/// </summary>
[CreateAssetMenu(fileName = "Audition_Session", menuName = "Audition/Session Data")]
public class AuditionSessionData : ScriptableObject
{
    [Header("Session Info")]
    public string sessionName = "Chapter 1 - Audition";

    [Tooltip("Audio chung (phases có thể override)")]
    public AudioClip defaultAudioClip;

    [Header("Phases")]
    [Tooltip("Danh sách phases")]
    public List<PhaseData> phases = new List<PhaseData>();

    // ═══════════════════════════════════════════════════════════
    // VALIDATION
    // ═══════════════════════════════════════════════════════════

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (phases == null || phases.Count == 0)
        {
            Debug.LogWarning($"[{name}] No phases assigned!");
            return;
        }

        // Log total duration
        float totalDuration = 0f;
        int totalBeats = 0;

        foreach (var phase in phases)
        {
            if (phase != null)
            {
                totalDuration += phase.TotalDuration + phase.pauseDuration;
                totalBeats += phase.beatCount;
            }
        }

        Debug.Log($"[{name}] {phases.Count} phases | {totalBeats} total beats | {totalDuration:F1}s duration");
    }
#endif
}