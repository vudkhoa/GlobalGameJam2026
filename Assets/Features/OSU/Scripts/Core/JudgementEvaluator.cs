using UnityEngine;

/// <summary>
/// SRP: Evaluate tap accuracy
/// Responsibility: Calculate judgement from size difference
/// </summary>
public class JudgementEvaluator
{
    private JudgementConfig _config;

    public JudgementEvaluator(JudgementConfig config)
    {
        _config = config;
    }

    public JudgementType Evaluate(Vector2 currentSize, Vector2 targetSize)
    {
        // Calculate difference for both x and y, then average
        float diffX = Mathf.Abs(currentSize.x - targetSize.x);
        float diffY = Mathf.Abs(currentSize.y - targetSize.y);
        float difference = (diffX + diffY) * 0.5f;

        // Convert world-scale difference to pixel difference
        // (BeatConfig uses pixels/100 for world scale, thresholds are in pixels)
        float pixelDifference = difference * 100f;

        // Calculate max possible shrink range (outer - inner) in pixels
        // This is the total distance the ring can shrink
        float maxShrinkRange = (targetSize.x + targetSize.y) * 0.5f * 100f; // Average target size in pixels

        // Calculate dynamic thresholds based on percentage of shrink range
        float perfectThreshold = maxShrinkRange * _config.perfectThresholdPercent;
        float goodThreshold = maxShrinkRange * _config.goodThresholdPercent;
        float okThreshold = maxShrinkRange * _config.okThresholdPercent;

        // Evaluate judgement using dynamic thresholds
        if (pixelDifference <= perfectThreshold)
            return JudgementType.Perfect;
        else if (pixelDifference <= goodThreshold)
            return JudgementType.Good;
        else if (pixelDifference <= okThreshold)
            return JudgementType.OK;
        else
            return JudgementType.Miss;
    }

    public int GetScore(JudgementType judgement)
    {
        return judgement switch
        {
            JudgementType.Perfect => _config.perfectScore,
            JudgementType.Good => _config.goodScore,
            JudgementType.OK => _config.okScore,
            _ => _config.missScore
        };
    }

    public FeedbackData GetFeedback(JudgementType judgement)
    {
        return judgement switch
        {
            JudgementType.Perfect => _config.perfectFeedback,
            JudgementType.Good => _config.goodFeedback,
            JudgementType.OK => _config.okFeedback,
            _ => _config.missFeedback
        };
    }
}