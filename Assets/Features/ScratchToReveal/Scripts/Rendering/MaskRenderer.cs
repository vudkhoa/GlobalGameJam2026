using UnityEngine;

/// <summary>
/// Renders brush strokes to RenderTexture mask
/// Uses circular brush with randomized opacity for realistic scratch feel
/// </summary>
public class MaskRenderer
{
    private readonly RenderTexture _maskTexture;
    private readonly float _brushSize;
    private readonly float _brushOpacity;
    private readonly Material _brushMaterial;
    private readonly RenderTexture _tempTexture;

    public MaskRenderer(RenderTexture maskTexture, float brushSize, float brushOpacity)
    {
        _maskTexture = maskTexture;
        _brushSize = brushSize;
        _brushOpacity = brushOpacity;

        // Create temp texture for blitting
        _tempTexture = new RenderTexture(maskTexture.width, maskTexture.height, 0, maskTexture.format);

        // Use circular brush shader
        Shader brushShader = Shader.Find("Hidden/CircularBrush");
        if (brushShader == null)
        {
            Debug.LogError("CircularBrush shader not found! Falling back to Internal-Colored");
            brushShader = Shader.Find("Hidden/Internal-Colored");
        }
        _brushMaterial = new Material(brushShader);
    }

    /// <summary>
    /// Draw circular brush at UV position with randomized opacity
    /// </summary>
    public void DrawBrushAtUV(Vector2 uv)
    {
        // Randomize opacity for realistic scratch-off feel
        // Each stroke has slightly different strength
        float randomOpacity = _brushOpacity * Random.Range(0.7f, 1.0f);

        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = _maskTexture;

        GL.PushMatrix();
        GL.LoadPixelMatrix(0, 1, 1, 0);

        // Setup blend mode for accumulation
        _brushMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        _brushMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
        _brushMaterial.SetInt("_BlendOp", (int)UnityEngine.Rendering.BlendOp.Max);
        _brushMaterial.SetPass(0);

        GL.Begin(GL.QUADS);

        // Randomized color for variation (subtle)
        float colorVar = Random.Range(0.95f, 1.0f);
        GL.Color(new Color(colorVar, colorVar, colorVar, randomOpacity));

        // Calculate brush size in UV space
        float halfSize = _brushSize / _maskTexture.width * 0.5f;

        // Add slight random offset for organic feel
        float offsetX = Random.Range(-0.002f, 0.002f);
        float offsetY = Random.Range(-0.002f, 0.002f);
        Vector2 finalUV = new Vector2(uv.x + offsetX, uv.y + offsetY);

        // Draw quad with UV coordinates for circular shader
        GL.TexCoord2(0, 0); GL.Vertex3(finalUV.x - halfSize, finalUV.y - halfSize, 0);
        GL.TexCoord2(1, 0); GL.Vertex3(finalUV.x + halfSize, finalUV.y - halfSize, 0);
        GL.TexCoord2(1, 1); GL.Vertex3(finalUV.x + halfSize, finalUV.y + halfSize, 0);
        GL.TexCoord2(0, 1); GL.Vertex3(finalUV.x - halfSize, finalUV.y + halfSize, 0);

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
        if (_tempTexture != null)
        {
            _tempTexture.Release();
            Object.Destroy(_tempTexture);
        }
    }
}
