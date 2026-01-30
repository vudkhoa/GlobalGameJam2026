using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Factory for creating shader parameters
/// Makes it easy to extend with more parameters
/// </summary>
public static class ShaderParameterFactory
{
    /// <summary>
    /// Create blur parameter
    /// </summary>
    public static ShaderParameter CreateBlurParameter(float current, float target)
    {
        return new ShaderParameter(
            propertyName: "_BlurAmount",
            displayName: "Blur Amount",
            minValue: 0f,
            maxValue: 10f,
            currentValue: current,
            targetValue: target
        );
    }

    /// <summary>
    /// Create horizontal scale parameter
    /// </summary>
    public static ShaderParameter CreateHorizontalScaleParameter(float current, float target)
    {
        return new ShaderParameter(
            propertyName: "_HorizontalScale",
            displayName: "Horizontal Scale",
            minValue: 0.1f,
            maxValue: 3f,
            currentValue: current,
            targetValue: target
        );
    }

    /// <summary>
    /// Create all default parameters from settings
    /// </summary>
    public static List<ShaderParameter> CreateDefaultParameters(CorrectionSettings settings)
    {
        var parameters = new List<ShaderParameter>
        {
            CreateBlurParameter(settings.InitialBlurAmount, settings.TargetBlurAmount),
            CreateHorizontalScaleParameter(settings.InitialHorizontalScale, settings.TargetHorizontalScale)
        };

        return parameters;
    }
}
