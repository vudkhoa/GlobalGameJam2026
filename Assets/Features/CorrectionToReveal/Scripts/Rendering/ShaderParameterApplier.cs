using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Applies shader parameters to material dynamically
/// Single Responsibility: Material property management
/// </summary>
public class ShaderParameterApplier
{
    private readonly Material _material;

    public ShaderParameterApplier(Material material)
    {
        _material = material;
    }

    /// <summary>
    /// Apply a single parameter to shader by property name
    /// </summary>
    public void ApplyParameter(string propertyName, float value)
    {
        if (_material != null && _material.HasProperty(propertyName))
        {
            _material.SetFloat(propertyName, value);
        }
        else
        {
            Debug.LogWarning($"Material does not have property: {propertyName}");
        }
    }

    /// <summary>
    /// Apply all parameters from a list
    /// </summary>
    public void ApplyAllParameters(List<ShaderParameter> parameters)
    {
        if (_material == null)
        {
            Debug.LogError("Material is null!");
            return;
        }

        foreach (var param in parameters)
        {
            ApplyParameter(param.PropertyName, param.CurrentValue);
        }
    }
}
