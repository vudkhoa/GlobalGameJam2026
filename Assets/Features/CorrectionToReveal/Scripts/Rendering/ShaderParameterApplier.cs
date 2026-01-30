using UnityEngine;

/// <summary>
/// Applies shader parameters to material
/// Single Responsibility: Material property management
/// </summary>
public class ShaderParameterApplier
{
    private readonly Material _material;
    private readonly string _blurPropertyName;
    private readonly string _scalePropertyName;
    
    public ShaderParameterApplier(Material material, string blurPropertyName, string scalePropertyName)
    {
        _material = material;
        _blurPropertyName = blurPropertyName;
        _scalePropertyName = scalePropertyName;
    }
    
    /// <summary>
    /// Apply blur amount to shader
    /// </summary>
    public void ApplyBlurAmount(float value)
    {
        if (_material != null)
        {
            _material.SetFloat(_blurPropertyName, value);
        }
    }
    
    /// <summary>
    /// Apply horizontal scale to shader
    /// </summary>
    public void ApplyHorizontalScale(float value)
    {
        if (_material != null)
        {
            _material.SetFloat(_scalePropertyName, value);
        }
    }
    
    /// <summary>
    /// Apply all parameters from CorrectionData
    /// </summary>
    public void ApplyAllParameters(CorrectionData data)
    {
        ApplyBlurAmount(data.BlurAmount);
        ApplyHorizontalScale(data.HorizontalScale);
    }
}
