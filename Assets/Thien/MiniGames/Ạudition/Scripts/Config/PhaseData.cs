using UnityEngine;

/// <summary>
/// SRP: Store PURE phase configuration data
/// Responsibility: Hold phase data, validate in Editor
/// </summary>
[CreateAssetMenu(fileName = "Phase_Data", menuName = "Audition/Phase Data")]
public class PhaseData : ScriptableObject
{
    // ═══════════════════════════════════════════════════════════
    // PHASE INFO
    // ═══════════════════════════════════════════════════════════

    [Header("Phase Info")]
    [Tooltip("Tên phase")]
    public string phaseName = "Phase 1";

    [Tooltip("Audio clip riêng (null = dùng default)")]
    public AudioClip audioClip;

    // ═══════════════════════════════════════════════════════════
    // BEAT SETTINGS
    // ═══════════════════════════════════════════════════════════

    [Header("Beat Settings")]
    [Tooltip("Số lượng beats trong phase")]
    [Range(1, 100)]
    public int beatCount = 10;

    [Tooltip("Thời gian giữa các beats (giây)")]
    [Range(0.3f, 3f)]
    public float beatInterval = 1f;

    [Tooltip("Vùng random vị trí (radius từ center)")]
    [Range(0f, 500f)]
    public float positionRadius = 100f;

    [Tooltip("Random seed (0 = truly random)")]
    public int randomSeed = 0;

    // ═══════════════════════════════════════════════════════════
    // PHASE TRANSITION
    // ═══════════════════════════════════════════════════════════

    [Header("Phase Transition")]
    [Tooltip("Pause sau phase (giây)")]
    [Range(0f, 10f)]
    public float pauseDuration = 2f;

    [Tooltip("Animation trigger")]
    public string animationTrigger = "";

    // ═══════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Total duration of this phase (seconds)
    /// </summary>
    public float TotalDuration => beatCount * beatInterval;

    // ═══════════════════════════════════════════════════════════
    // VALIDATION
    // ═══════════════════════════════════════════════════════════

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (beatCount <= 0)
            beatCount = 1;

        if (beatInterval <= 0f)
            beatInterval = 0.5f;
    }
#endif
}