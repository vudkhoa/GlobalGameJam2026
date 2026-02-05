using UnityEngine;

/// <summary>
/// SRP: Store beat visual & timing configuration
/// Responsibility: Define ring sizes, colors, shrink duration
/// </summary>
[CreateAssetMenu(fileName = "Beat_Config", menuName = "Audition/Beat Config")]
public class BeatConfig : ScriptableObject
{
    // ═══════════════════════════════════════════════════════════
    // VISUAL - RING SIZES (PIXEL VALUES - Editor-friendly)
    // ═══════════════════════════════════════════════════════════

    [Header("Visual - Ring Sizes (Pixel Units)")]
    [Tooltip("Kích thước base của beat (pixel size)\nSẽ tự động convert sang world scale")]
    [Range(50f, 500f)]
    public float beatSizePixels = 200f;

    [Tooltip("Kích thước inner ring (vòng cố định ở giữa - target area)\nTheo % của beatSize")]
    [Range(0.5f, 1.5f)]
    public float innerRingScale = 1.0f;

    [Tooltip("Kích thước ban đầu của outer ring (vòng bên ngoài sẽ shrink vào)\nTheo % của beatSize")]
    [Range(1.0f, 3.0f)]
    public float outerRingStartScale = 1.5f;

    [Tooltip("Độ dày của ring border")]
    [Range(1f, 20f)]
    public float ringThickness = 10f;

    // ═══════════════════════════════════════════════════════════
    // VISUAL - COLORS
    // ═══════════════════════════════════════════════════════════

    [Header("Visual - Colors")]
    [Tooltip("Màu sắc inner ring (vòng cố định ở giữa)")]
    public Color innerRingColor = Color.white;

    [Tooltip("Màu sắc outer ring (vòng thu vào)")]
    public Color outerRingColor = new Color(1f, 0.5f, 0f, 1f); // Orange

    [Tooltip("Alpha transparency của rings")]
    [Range(0f, 1f)]
    public float ringAlpha = 0.8f;

    // ═══════════════════════════════════════════════════════════
    // TIMING
    // ═══════════════════════════════════════════════════════════

    [Header("Timing")]
    [Tooltip("Thời gian để outer ring shrink về inner ring (giây)")]
    [Range(0.5f, 5f)]
    public float shrinkDuration = 1.5f;

    // ═══════════════════════════════════════════════════════════
    // HELPER PROPERTIES (AUTO-CONVERT TO WORLD SCALE)
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Get base beat size in world units (pixels / 100)
    /// Use this for setting BeatData.size
    /// </summary>
    public float BeatSizeWorld => beatSizePixels / 100f;

    /// <summary>
    /// Get inner ring size in world units (ready to use in transform.localScale)
    /// </summary>
    public float InnerRingWorldScale => (beatSizePixels * innerRingScale) / 100f;

    /// <summary>
    /// Get outer ring start size in world units (ready to use in transform.localScale)
    /// </summary>
    public float OuterRingStartWorldScale => (beatSizePixels * outerRingStartScale) / 100f;

    // ═══════════════════════════════════════════════════════════
    // VALIDATION (Editor Only)
    // ═══════════════════════════════════════════════════════════

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Ensure outer ring starts larger than inner ring
        if (outerRingStartScale <= innerRingScale)
        {
            outerRingStartScale = innerRingScale + 0.5f;
        }

        // Clamp values
        beatSizePixels = Mathf.Max(50f, beatSizePixels);
        ringAlpha = Mathf.Clamp01(ringAlpha);
    }
#endif
}