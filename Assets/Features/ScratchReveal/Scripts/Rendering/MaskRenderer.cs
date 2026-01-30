using UnityEngine;

/// <summary>
/// Renders brush strokes to RenderTexture mask
/// Pure rendering logic - no business logic
/// </summary>
public class MaskRenderer
{
    private readonly RenderTexture _maskTexture;
    private readonly float _brushSize;
    private readonly float _brushOpacity;
    private readonly Material _brushMaterial;

    public MaskRenderer(RenderTexture maskTexture, float brushSize, float brushOpacity)
    {
        _maskTexture = maskTexture;
        _brushSize = brushSize;
        _brushOpacity = brushOpacity;

        // Create persistent material for brush rendering
        _brushMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));
    }

    /// <summary>
    /// Draw brush at UV position (0-1 range) on mask texture
    /// </summary>
    public void DrawBrushAtUV(Vector2 uv)
    {
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = _maskTexture;

        GL.PushMatrix();
        GL.LoadPixelMatrix(0, 1, 1, 0);

        // Enable additive blending (accumulate white on black)
        _brushMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        _brushMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
        _brushMaterial.SetInt("_BlendOp", (int)UnityEngine.Rendering.BlendOp.Max);
        _brushMaterial.SetPass(0);

        GL.Begin(GL.QUADS);
        GL.Color(new Color(1, 1, 1, _brushOpacity));

        // Calculate brush size in UV space
        float halfSize = _brushSize / _maskTexture.width * 0.5f;

        // Draw quad (brush) at UV position
        GL.Vertex3(uv.x - halfSize, uv.y - halfSize, 0);
        GL.Vertex3(uv.x + halfSize, uv.y - halfSize, 0);
        GL.Vertex3(uv.x + halfSize, uv.y + halfSize, 0);
        GL.Vertex3(uv.x - halfSize, uv.y + halfSize, 0);

        GL.End();
        GL.PopMatrix();

        RenderTexture.active = previous;
    }

    /// <summary>
    /// Clear mask texture (reset to black)
    /// </summary>
    public void ClearMask()
    {
        RenderTexture.active = _maskTexture;
        GL.Clear(true, true, Color.black);
        RenderTexture.active = null;
    }

    /// <summary>
    /// Cleanup resources
    /// </summary>
    public void Dispose()
    {
        if (_brushMaterial != null)
        {
            Object.Destroy(_brushMaterial);
        }
    }
}
