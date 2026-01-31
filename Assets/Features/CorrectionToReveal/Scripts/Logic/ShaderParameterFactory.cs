using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Factory for creating shader parameters dynamically
/// Automatically detects and creates parameters from settings
/// </summary>
public static class ShaderParameterFactory
{
    /// <summary>
    /// Create all shader parameters from settings
    /// This is the SINGLE SOURCE OF TRUTH for all adjustable parameters
    /// Add new parameters here to automatically spawn rulers for them
    /// </summary>
    public static List<ShaderParameter> CreateAllParameters(CorrectionSettings settings)
    {
        var parameters = new List<ShaderParameter>();

        if (settings == null || settings.ShaderProperties == null)
            return parameters;

        foreach (var prop in settings.ShaderProperties)
        {
            parameters.Add(new ShaderParameter(
                propertyName: prop.PropertyName,
                displayName: prop.DisplayName,
                minValue: prop.MinValue,
                maxValue: prop.MaxValue,
                currentValue: prop.InitialValue,
                targetValue: prop.TargetValue
            ));
        }

        return parameters;
    }

    /// <summary>
    /// Get the count of adjustable parameters
    /// </summary>
    public static int GetParameterCount(CorrectionSettings settings)
    {
        return CreateAllParameters(settings).Count;
    }
}
