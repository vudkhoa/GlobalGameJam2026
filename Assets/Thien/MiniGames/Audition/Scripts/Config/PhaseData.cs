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

    [Header("Beat Settings")]
    [Range(1, 100)]
    public int beatCount = 10;

    [Range(0.3f, 3f)]
    public float beatInterval = 1f;

    [Range(0f, 500f)]
    public float positionRadius = 100f;

    public int randomSeed = 0;

    [Header("Phase Transition")]
    [Range(0f, 10f)]
    public float pauseDuration = 2f;

    public string animationTrigger = "";

    public float TotalDuration => beatCount * beatInterval;

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