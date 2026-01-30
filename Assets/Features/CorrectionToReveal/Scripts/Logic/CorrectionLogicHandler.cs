using System;
using UnityEngine;

/// <summary>
/// Main logic handler for correction mini-game
/// Coordinates between data, validation, and rendering
/// </summary>
public class CorrectionLogicHandler
{
    private readonly CorrectionData _data;
    private readonly CorrectionValidator _validator;
    private readonly ShaderParameterApplier _shaderApplier;

    public event Action OnCorrectionComplete;
    public event Action<float> OnProgressChanged;

    private bool _isComplete;

    public CorrectionLogicHandler(
        CorrectionData data,
        CorrectionValidator validator,
        ShaderParameterApplier shaderApplier)
    {
        _data = data;
        _validator = validator;
        _shaderApplier = shaderApplier;
        _isComplete = false;
    }

    /// <summary>
    /// Update blur amount and check for completion
    /// </summary>
    public void UpdateBlurAmount(float value)
    {
        _data.BlurAmount = value;
        _shaderApplier.ApplyBlurAmount(value);
        CheckCompletion();
    }

    /// <summary>
    /// Update horizontal scale and check for completion
    /// </summary>
    public void UpdateHorizontalScale(float value)
    {
        _data.HorizontalScale = value;
        _shaderApplier.ApplyHorizontalScale(value);
        CheckCompletion();
    }

    /// <summary>
    /// Initialize shader with current data values
    /// </summary>
    public void Initialize()
    {
        _shaderApplier.ApplyAllParameters(_data);
        _isComplete = false;
    }

    /// <summary>
    /// Check if correction is complete and trigger events
    /// </summary>
    private void CheckCompletion()
    {
        float progress = _validator.GetProgress();
        OnProgressChanged?.Invoke(progress);

        if (!_isComplete && _validator.IsCorrect())
        {
            _isComplete = true;
            OnCorrectionComplete?.Invoke();
        }
    }

    /// <summary>
    /// Get current progress (0-1)
    /// </summary>
    public float GetProgress()
    {
        return _validator.GetProgress();
    }

    /// <summary>
    /// Check if game is complete
    /// </summary>
    public bool IsComplete()
    {
        return _isComplete;
    }
}
