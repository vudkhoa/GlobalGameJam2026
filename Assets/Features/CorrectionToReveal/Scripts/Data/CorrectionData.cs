using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Data class for Correction mini-game
/// Stores shader parameters dynamically
/// </summary>
[System.Serializable]
public class CorrectionData
{
    [Header("Shader Parameters")]
    [Tooltip("List of all adjustable shader parameters")]
    public List<ShaderParameter> Parameters = new List<ShaderParameter>();

    [Header("Tolerance")]
    [Tooltip("Acceptable error margin for winning")]
    [Range(0.01f, 1.0f)]
    public float Tolerance = 0.1f;

    /// <summary>
    /// Initialize with parameters from factory
    /// </summary>
    public void Initialize(List<ShaderParameter> parameters, float tolerance)
    {
        Parameters = parameters;
        Tolerance = tolerance;
    }

    /// <summary>
    /// Get parameter by property name
    /// </summary>
    public ShaderParameter GetParameter(string propertyName)
    {
        return Parameters.FirstOrDefault(p => p.PropertyName == propertyName);
    }

    /// <summary>
    /// Update parameter value by property name
    /// </summary>
    public void UpdateParameter(string propertyName, float value)
    {
        var param = GetParameter(propertyName);
        if (param != null)
        {
            param.CurrentValue = value;
        }
    }

    /// <summary>
    /// Check if ALL parameters are correct
    /// </summary>
    public bool IsCorrect()
    {
        return Parameters.All(p => p.IsCorrect(Tolerance));
    }

    /// <summary>
    /// Get overall progress (0-1) across all parameters
    /// </summary>
    public float GetOverallProgress()
    {
        if (Parameters.Count == 0) return 0f;

        float totalProgress = Parameters.Sum(p => p.GetProgress());
        return totalProgress / Parameters.Count;
    }

    /// <summary>
    /// Get count of adjustable parameters
    /// </summary>
    public int GetParameterCount()
    {
        return Parameters.Count;
    }
}
