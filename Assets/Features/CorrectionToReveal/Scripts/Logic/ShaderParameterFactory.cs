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

        // Parameter 1: Blur Amount
        parameters.Add(new ShaderParameter(
            propertyName: "_BlurAmount",
            displayName: "Blur Amount (Độ mờ)",
            minValue: 0f,
            maxValue: 10f,
            currentValue: settings.InitialBlurAmount,
            targetValue: settings.TargetBlurAmount
        ));

        // Parameter 2: Horizontal Scale
        parameters.Add(new ShaderParameter(
            propertyName: "_HorizontalScale",
            displayName: "Horizontal Scale (Scale ngang)",
            minValue: 0.1f,
            maxValue: 3f,
            currentValue: settings.InitialHorizontalScale,
            targetValue: settings.TargetHorizontalScale
        ));

        // TODO: Add more parameters here as needed
        // Example:
        // parameters.Add(new ShaderParameter(
        //     propertyName: "_NewParameter",
        //     displayName: "New Parameter",
        //     minValue: 0f,
        //     maxValue: 1f,
        //     currentValue: settings.InitialNewParameter,
        //     targetValue: settings.TargetNewParameter
        // ));

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
