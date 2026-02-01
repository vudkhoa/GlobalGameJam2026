using UnityEngine;

/// <summary>
/// SRP: Store judgement rules & feedback data
/// Responsibility: Define thresholds, scores, feedback
/// </summary>
[CreateAssetMenu(fileName = "Judgement_Config", menuName = "Audition/Judgement Config")]
public class JudgementConfig : ScriptableObject
{
    // ═══════════════════════════════════════════════════════════
    // THRESHOLDS (Accuracy)
    // ═══════════════════════════════════════════════════════════

    [Header("Thresholds (Pixel Difference)")]
    [Tooltip("Threshold cho Perfect (pixel difference)")]
    [Range(0f, 50f)]
    public float perfectThreshold = 20f;

    [Tooltip("Threshold cho Good (pixel difference)")]
    [Range(20f, 100f)]
    public float goodThreshold = 50f;

    [Tooltip("Threshold cho OK (pixel difference)")]
    [Range(50f, 150f)]
    public float okThreshold = 80f;

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
        if (goodThreshold <= perfectThreshold)
            goodThreshold = perfectThreshold + 10f;

        if (okThreshold <= goodThreshold)
            okThreshold = goodThreshold + 20f;
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