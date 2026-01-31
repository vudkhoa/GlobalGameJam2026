using UnityEngine;

[CreateAssetMenu(fileName = "Phase_Data", menuName = "Audition/Phase Data")]
public class PhaseData : ScriptableObject
{
    [Header("Phase Info")]
    public string phaseName = "Phase 1";
    public AudioClip audioClip;

    // ✅ NEW: BeatConfig riêng cho phase này
    [Header("Beat Configuration")]
    [Tooltip("BeatConfig cho phase này (null = dùng default)")]
    public BeatConfig beatConfig;

    [Header("Trajectory")]
    [Tooltip("Trajectory config cho phase này (REQUIRED)")]
    public TrajectoryConfig trajectoryConfig;

    [Header("Phase Transition")]
    [Range(0f, 10f)]
    public float pauseDuration = 2f;

    public string animationTrigger = "";

    public float TotalDuration => trajectoryConfig != null
        ? trajectoryConfig.beatCount * trajectoryConfig.beatInterval
        : 0f;

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Validation moved to TrajectoryConfig
    }
#endif
}