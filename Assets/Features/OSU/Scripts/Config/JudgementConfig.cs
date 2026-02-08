using UnityEngine;

/// <summary>
/// SRP: Store judgement rules & feedback data
/// Responsibility: Define thresholds, scores, feedback
/// </summary>
[CreateAssetMenu(fileName = "Judgement_Config", menuName = "Audition/Judgement Config")]
public class JudgementConfig : ScriptableObject
{
    // ═══════════════════════════════════════════════════════════
    // THRESHOLDS (Accuracy) - NOW PERCENTAGE-BASED
    // ═══════════════════════════════════════════════════════════

    [Header("Thresholds (% of Shrink Range)")]
    [Tooltip("Perfect threshold (% của khoảng cách shrink)\nOsu! standard: ~15-35% (Easy-Medium difficulty)")]
    [Range(0f, 1f)]
    public float perfectThresholdPercent = 0.35f; // 35% of shrink range

    [Tooltip("Good threshold (% của khoảng cách shrink)\nOsu! standard: ~40-60%")]
    [Range(0f, 1f)]
    public float goodThresholdPercent = 0.60f; // 60% of shrink range

    [Tooltip("OK threshold (% của khoảng cách shrink)\nOsu! standard: ~70-85%")]
    [Range(0f, 1f)]
    public float okThresholdPercent = 0.85f; // 85% of shrink range

    // Miss = anything > okThreshold

    // ═══════════════════════════════════════════════════════════
    // SCORING
    // ═══════════════════════════════════════════════════════════

    [Header("Scoring")]
    public int perfectScore = 3;
    public int goodScore = 2;
    public int okScore = 1;
    public int missScore = 0;

    // ═══════════════════════════════════════════════════════════
    // FEEDBACK DATA
    // ═══════════════════════════════════════════════════════════

    [Header("Feedback")]
    public FeedbackData perfectFeedback = new FeedbackData
    {
        text = "PERFECT!",
        color = Color.yellow,
        displayDuration = 0.5f
    };

    public FeedbackData goodFeedback = new FeedbackData
    {
        text = "GOOD",
        color = Color.green,
        displayDuration = 0.5f
    };

    public FeedbackData okFeedback = new FeedbackData
    {
        text = "OK",
        color = Color.blue,
        displayDuration = 0.5f
    };

    public FeedbackData missFeedback = new FeedbackData
    {
        text = "MISS",
        color = Color.red,
        displayDuration = 0.5f
    };

    // ═══════════════════════════════════════════════════════════
    // COMBO
    // ═══════════════════════════════════════════════════════════

    [Header("Combo")]
    [Tooltip("Số perfect/good liên tiếp để trigger combo")]
    public int comboThreshold = 5;

    public Color comboColor = Color.cyan;

    // ═══════════════════════════════════════════════════════════
    // VALIDATION
    // ═══════════════════════════════════════════════════════════

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Ensure thresholds are in order
        if (goodThresholdPercent <= perfectThresholdPercent)
            goodThresholdPercent = perfectThresholdPercent + 0.15f;

        if (okThresholdPercent <= goodThresholdPercent)
            okThresholdPercent = goodThresholdPercent + 0.20f;

        // Clamp to 0-1 range
        perfectThresholdPercent = Mathf.Clamp01(perfectThresholdPercent);
        goodThresholdPercent = Mathf.Clamp01(goodThresholdPercent);
        okThresholdPercent = Mathf.Clamp01(okThresholdPercent);
    }
#endif
}

/// <summary>
/// Feedback data for each judgement type
/// </summary>
[System.Serializable]
public class FeedbackData
{
    public string text;
    public Color color;
    public float displayDuration;
    public AudioClip soundEffect;
    public GameObject particleEffectPrefab;
}