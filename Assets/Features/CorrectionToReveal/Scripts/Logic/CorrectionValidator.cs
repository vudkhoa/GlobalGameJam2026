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
    /// Check if ALL parameters are correct
    /// </summary>
    public bool IsCorrect()
    {
        return _data.IsCorrect();
    }

    /// <summary>
    /// Get overall progress (0-1) across all parameters
    /// </summary>
    public float GetProgress()
    {
        return _data.GetOverallProgress();
    }
}
