using UnityEngine;

/// <summary>
/// Data class for Correction mini-game
/// Stores shader parameters and their target values
/// </summary>
[System.Serializable]
public class CorrectionData
{
    [Header("Shader Parameters")]
    [Tooltip("Current blur amount (0-10)")]
    public float BlurAmount = 5.0f;
    
    [Tooltip("Current horizontal scale (0.1-3)")]
    public float HorizontalScale = 1.5f;
    
    [Header("Target Values")]
    [Tooltip("Target blur amount for winning condition")]
    public float TargetBlurAmount = 0.0f;
    
    [Tooltip("Target horizontal scale for winning condition")]
    public float TargetHorizontalScale = 1.0f;
    
    [Header("Tolerance")]
    [Tooltip("Acceptable error margin for winning")]
    [Range(0.01f, 1.0f)]
    public float Tolerance = 0.1f;
    
    /// <summary>
    /// Check if current values are close enough to target values
    /// </summary>
    public bool IsCorrect()
    {
        bool blurCorrect = Mathf.Abs(BlurAmount - TargetBlurAmount) <= Tolerance;
        bool scaleCorrect = Mathf.Abs(HorizontalScale - TargetHorizontalScale) <= Tolerance;
        
        return blurCorrect && scaleCorrect;
    }
    
    /// <summary>
    /// Get progress percentage (0-1) for blur parameter
    /// </summary>
    public float GetBlurProgress()
    {
        float maxError = 10.0f; // Max blur range
        float currentError = Mathf.Abs(BlurAmount - TargetBlurAmount);
        return 1.0f - Mathf.Clamp01(currentError / maxError);
    }
    
    /// <summary>
    /// Get progress percentage (0-1) for scale parameter
    /// </summary>
    public float GetScaleProgress()
    {
        float maxError = 2.9f; // Max scale range (3 - 0.1)
        float currentError = Mathf.Abs(HorizontalScale - TargetHorizontalScale);
        return 1.0f - Mathf.Clamp01(currentError / maxError);
    }
    
    /// <summary>
    /// Get overall progress percentage (0-1)
    /// </summary>
    public float GetOverallProgress()
    {
        return (GetBlurProgress() + GetScaleProgress()) / 2.0f;
    }
}
