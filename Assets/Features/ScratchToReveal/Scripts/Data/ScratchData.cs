using System;
using UnityEngine;

/// <summary>
/// Runtime data for scratch reveal system
/// Manages RenderTexture lifecycle
/// </summary>
public class ScratchData : IDisposable
{
    public RenderTexture MaskTexture { get; private set; }

    private readonly int _resolution;

    public ScratchData(int resolution)
    {
        _resolution = resolution;

        // Detect best supported format for mask texture
        RenderTextureFormat format = GetBestMaskFormat();

        // Create RenderTexture for mask
        MaskTexture = new RenderTexture(resolution, resolution, 0, format)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        // Initialize with black (no scratch)
        RenderTexture.active = MaskTexture;
        GL.Clear(true, true, Color.black);
        RenderTexture.active = null;
    }

    /// <summary>
    /// Detect best supported RenderTexture format for mask
    /// Priority: R8 (1 byte/pixel) > ARGB32 (4 bytes/pixel)
    /// </summary>
    private static RenderTextureFormat GetBestMaskFormat()
    {
        // Try R8 first (most memory efficient)
        if (SystemInfo.IsFormatSupported(UnityEngine.Experimental.Rendering.GraphicsFormat.R8_UNorm,
            UnityEngine.Experimental.Rendering.GraphicsFormatUsage.Render))
        {
            return RenderTextureFormat.R8;
        }

        // Fallback to ARGB32 (widely supported)
        return RenderTextureFormat.ARGB32;
    }

    public void Dispose()
    {
        if (MaskTexture != null)
        {
            MaskTexture.Release();
            UnityEngine.Object.Destroy(MaskTexture);
        }
    }
}
