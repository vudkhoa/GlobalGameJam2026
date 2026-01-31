using UnityEngine;

/// <summary>
/// Shader parameter metadata
/// Defines properties of a shader parameter for UI generation
/// </summary>
[System.Serializable]
public class ShaderParameter
{
    public string PropertyName;
    public string DisplayName;
    public float MinValue;
    public float MaxValue;
    public float CurrentValue;
    public float TargetValue;

    public ShaderParameter(string propertyName, string displayName, float minValue, float maxValue, float currentValue, float targetValue)
    {
        PropertyName = propertyName;
        DisplayName = displayName;
        MinValue = minValue;
        MaxValue = maxValue;
        CurrentValue = currentValue;
        TargetValue = targetValue;
    }

    /// <summary>
    /// Check if current value is close to target
    /// </summary>
    public bool IsCorrect(float tolerance)
    {
        Debug.Log("[ShaderParameter] Checking Parameter: " + PropertyName);
        Debug.Log("[ShaderParameter] Checking Current Value: " + CurrentValue);
        Debug.Log("[ShaderParameter] Checking TargetValue Value: " + TargetValue);
        Debug.Log("[ShaderParameter] IsCorrect: " + Mathf.Abs(CurrentValue - TargetValue) + " <= " + tolerance);
        return Mathf.Abs(CurrentValue - TargetValue) <= tolerance;
    }

    /// <summary>
    /// Get progress towards target (0-1)
    /// </summary>
    public float GetProgress()
    {
        float range = MaxValue - MinValue;
        float error = Mathf.Abs(CurrentValue - TargetValue);
        return 1.0f - Mathf.Clamp01(error / range);
    }
}
