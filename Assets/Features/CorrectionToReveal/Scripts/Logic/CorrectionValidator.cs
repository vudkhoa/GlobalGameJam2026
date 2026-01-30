using UnityEngine;

/// <summary>
/// Handles validation logic for correction parameters
/// Single Responsibility: Game rule validation
/// </summary>
public class CorrectionValidator
{
    private readonly CorrectionData _data;

    public CorrectionValidator(CorrectionData data)
    {
        _data = data;
    }

    /// <summary>
    /// Check if correction is complete
    /// </summary>
    public bool IsCorrect()
    {
        return _data.IsCorrect();
    }

    /// <summary>
    /// Get overall progress (0-1)
    /// </summary>
    public float GetProgress()
    {
        return _data.GetOverallProgress();
    }

    /// <summary>
    /// Get individual parameter progress
    /// </summary>
    public (float blurProgress, float scaleProgress) GetIndividualProgress()
    {
        return (_data.GetBlurProgress(), _data.GetScaleProgress());
    }

    /// <summary>
    /// Check if blur is correct
    /// </summary>
    public bool IsBlurCorrect()
    {
        return Mathf.Abs(_data.BlurAmount - _data.TargetBlurAmount) <= _data.Tolerance;
    }

    /// <summary>
    /// Check if scale is correct
    /// </summary>
    public bool IsScaleCorrect()
    {
        return Mathf.Abs(_data.HorizontalScale - _data.TargetHorizontalScale) <= _data.Tolerance;
    }
}
