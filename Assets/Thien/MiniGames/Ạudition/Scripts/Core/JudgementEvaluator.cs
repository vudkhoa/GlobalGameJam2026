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

    public JudgementType Evaluate(float currentSize, float targetSize)
    {
        float difference = Mathf.Abs(currentSize - targetSize);

        if (difference <= _config.perfectThreshold)
            return JudgementType.Perfect;
        else if (difference <= _config.goodThreshold)
            return JudgementType.Good;
        else if (difference <= _config.okThreshold)
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