using UnityEngine;

/// <summary>
/// Calculate scratch progress percentage from mask texture
/// Pure logic - no dependencies on other systems
/// </summary>
public class ScratchProgressCalculator
{
    private readonly RenderTexture _maskTexture;
    private readonly Texture2D _readTexture;
    private readonly int _totalPixels;

    public ScratchProgressCalculator(RenderTexture maskTexture, int resolution)
    {
        _maskTexture = maskTexture;
        _totalPixels = resolution * resolution;
        _readTexture = new Texture2D(resolution, resolution, TextureFormat.R8, false);
    }

    /// <summary>
    /// Calculate scratched percentage (0-100)
    /// Expensive operation - call sparingly (ReadPixels is slow)
    /// </summary>
    public float CalculatePercent()
    {
        RenderTexture.active = _maskTexture;
        _readTexture.ReadPixels(new Rect(0, 0, _readTexture.width, _readTexture.height), 0, 0);
        _readTexture.Apply();
        RenderTexture.active = null;

        Color[] pixels = _readTexture.GetPixels();
        int scratchedPixels = 0;

        foreach (Color pixel in pixels)
        {
            // Count pixels where red channel > 0.5 as scratched
            if (pixel.r > 0.5f)
            {
                scratchedPixels++;
            }
        }

        return (scratchedPixels / (float)_totalPixels) * 100f;
    }

    public void Dispose()
    {
        if (_readTexture != null)
        {
            Object.Destroy(_readTexture);
        }
    }
}
