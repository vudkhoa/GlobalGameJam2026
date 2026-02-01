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
    [Tooltip("Danh sách trajectory configs cho phase này (play tuần tự)")]
    public TrajectoryConfig[] trajectoryConfigs;

    [Header("Phase Transition")]
    [Range(0f, 10f)]
    public float pauseDuration = 2f;

    public string animationTrigger = "";

    public float TotalDuration
    {
        get
        {
            if (trajectoryConfigs == null || trajectoryConfigs.Length == 0)
                return 0f;

            float total = 0f;
            foreach (var config in trajectoryConfigs)
            {
                if (config != null)
                    total += config.beatCount * config.beatInterval;
            }
            return total;
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Validation moved to TrajectoryConfig
    }
#endif
}