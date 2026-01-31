using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Main logic handler for correction mini-game
/// Coordinates between data, validation, and rendering
/// Works with dynamic shader parameters
/// </summary>
public class CorrectionLogicHandler
{
    private CorrectionData _data;
    private CorrectionValidator _validator;
    private ShaderParameterApplier _shaderApplier;

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
    /// Load new level mechanics (Swaps dependencies)
    /// </summary>
    public void LoadLevel(CorrectionData data, CorrectionValidator validator, ShaderParameterApplier applier)
    {
        _data = data;
        _validator = validator;
        _shaderApplier = applier;
        _isComplete = false;
        Initialize();
    }

    /// <summary>
    /// Update any shader parameter by property name
    /// </summary>
    public void UpdateParameter(string propertyName, float value)
    {
        _data.UpdateParameter(propertyName, value);
        _shaderApplier.ApplyParameter(propertyName, value);
        CheckCompletion().Forget();
    }

    /// <summary>
    /// Initialize shader with all current parameter values
    /// </summary>
    public void Initialize()
    {
        _shaderApplier.ApplyAllParameters(_data.Parameters);
        _isComplete = false;
    }

    /// <summary>
    /// Check if correction is complete and trigger events
    /// </summary>
    private async UniTask CheckCompletion()
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
