using System;
using UnityEngine;

/// <summary>
/// Main scratch logic handler
/// Processes screen position inputs and coordinates scratch operations
/// Action-based method naming for clarity
/// </summary>
public class ScratchLogicHandler
{
    // Events
    public event Action<float> OnProgressChanged;

    // Dependencies (injected)
    private readonly MaskRenderer _maskRenderer;
    private readonly ScratchProgressCalculator _progressCalculator;
    private readonly CoordinateConverter _coordinateConverter;

    // State
    private int _frameCounter;
    private Vector2 _lastUV;
    private bool _hasLastUV;
    private const int UPDATE_PROGRESS_EVERY_N_FRAMES = 10;

    // Constructor (DI)
    public ScratchLogicHandler(
        MaskRenderer maskRenderer,
        ScratchProgressCalculator progressCalculator,
        CoordinateConverter coordinateConverter)
    {
        _maskRenderer = maskRenderer;
        _progressCalculator = progressCalculator;
        _coordinateConverter = coordinateConverter;
    }

    /// <summary>
    /// Start scratch operation at screen position
    /// Called when pointer first touches the scratch area
    /// </summary>
    public void StartScratchAtPosition(Vector2 screenPosition)
    {
        if (_coordinateConverter.ScreenToUV(screenPosition, out Vector2 uv))
        {
            _maskRenderer.DrawBrushAtUV(uv);
            _lastUV = uv;
            _hasLastUV = true;
        }
    }

    /// <summary>
    /// Continue scratch operation at screen position
    /// Interpolates between last position for smooth strokes
    /// </summary>
    public void ContinueScratchAtPosition(Vector2 screenPosition)
    {
        if (_coordinateConverter.ScreenToUV(screenPosition, out Vector2 uv))
        {
            // Interpolate between last position and current for smooth continuous stroke
            if (_hasLastUV)
            {
                float distance = Vector2.Distance(_lastUV, uv);
                int steps = Mathf.Max(1, Mathf.CeilToInt(distance * 100)); // More steps for longer distances

                for (int i = 0; i <= steps; i++)
                {
                    float t = i / (float)steps;
                    Vector2 interpolatedUV = Vector2.Lerp(_lastUV, uv, t);
                    _maskRenderer.DrawBrushAtUV(interpolatedUV);
                }
            }
            else
            {
                _maskRenderer.DrawBrushAtUV(uv);
            }

            _lastUV = uv;
            _hasLastUV = true;

            // Periodically update progress (optimization - avoid calling every frame)
            _frameCounter++;
            if (_frameCounter >= UPDATE_PROGRESS_EVERY_N_FRAMES)
            {
                _frameCounter = 0;
                UpdateProgress();
            }
        }
    }

    /// <summary>
    /// Finalize scratch operation
    /// Called when pointer is released
    /// </summary>
    public void FinalizeScratch()
    {
        _hasLastUV = false; // Reset for next stroke
        UpdateProgress();
    }

    /// <summary>
    /// Calculate and emit progress event
    /// </summary>
    private void UpdateProgress()
    {
        float percent = _progressCalculator.CalculatePercent();
        OnProgressChanged?.Invoke(percent);
    }

    /// <summary>
    /// Reset scratch to initial state
    /// </summary>
    public void Reset()
    {
        _maskRenderer.ClearMask();
        _frameCounter = 0;
        OnProgressChanged?.Invoke(0f);
    }

    /// <summary>
    /// Get current progress (0-100)
    /// </summary>
    public float GetProgress()
    {
        return _progressCalculator.CalculatePercent();
    }
}
