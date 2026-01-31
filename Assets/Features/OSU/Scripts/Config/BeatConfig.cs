using UnityEngine;

/// <summary>
/// SRP: Store beat visual & timing configuration
/// Responsibility: Define ring sizes, colors, shrink duration
/// </summary>
[CreateAssetMenu(fileName = "Beat_Config", menuName = "Audition/Beat Config")]
public class BeatConfig : ScriptableObject
{
    // ═══════════════════════════════════════════════════════════
    // VISUAL - RING SIZES
    // ═══════════════════════════════════════════════════════════

    [Header("Visual - Ring Sizes")]
    [Tooltip("Kích thước outer ring (target area)")]
    [Range(50f, 500f)]
    public float outerRingSize = 200f;

    [Tooltip("Kích thước ban đầu của inner ring")]
    [Range(100f, 600f)]
    public float innerRingStartSize = 300f;

    [Tooltip("Độ dày của ring border")]
    [Range(1f, 20f)]
    public float ringThickness = 10f;

    // ═══════════════════════════════════════════════════════════
    // VISUAL - COLORS
    // ═══════════════════════════════════════════════════════════

    [Header("Visual - Colors")]
    [Tooltip("Màu sắc outer ring (target)")]
    public Color outerRingColor = Color.white;

    [Tooltip("Màu sắc inner ring (shrinking)")]
    public Color innerRingColor = new Color(1f, 0.5f, 0f, 1f); // Orange

    [Tooltip("Alpha transparency của rings")]
    [Range(0f, 1f)]
    public float ringAlpha = 0.8f;

    // ═══════════════════════════════════════════════════════════
    // TIMING
    // ═══════════════════════════════════════════════════════════

    [Header("Timing")]
    [Tooltip("Thời gian để inner ring shrink về outer ring (giây)")]
    [Range(0.5f, 5f)]
    public float shrinkDuration = 1.5f;

    // ═══════════════════════════════════════════════════════════
    // VALIDATION (Editor Only)
    // ═══════════════════════════════════════════════════════════

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Ensure inner ring starts larger than outer ring
        if (innerRingStartSize <= outerRingSize)
        {
            innerRingStartSize = outerRingSize + 50f;
            Debug.LogWarning("[BeatConfig] innerRingStartSize must be > outerRingSize. Auto-adjusted.");
        }

        // Clamp alpha
        ringAlpha = Mathf.Clamp01(ringAlpha);
    }
#endif
}