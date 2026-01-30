using UnityEngine;

/// <summary>
/// Converts coordinates between different spaces
/// Pure logic - no MonoBehaviour dependencies
/// </summary>
public class CoordinateConverter
{
    private readonly RectTransform _rectTransform;
    private readonly Camera _camera;

    public CoordinateConverter(RectTransform rectTransform, Camera camera)
    {
        _rectTransform = rectTransform;
        _camera = camera;
    }

    /// <summary>
    /// Convert screen position to local position in RectTransform
    /// </summary>
    public bool ScreenToLocal(Vector2 screenPosition, out Vector2 localPosition)
    {
        return RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _rectTransform,
            screenPosition,
            _camera,
            out localPosition
        );
    }

    /// <summary>
    /// Convert local position to UV coordinates (0-1 range)
    /// Note: Flips V coordinate because Unity UI Y goes up but texture V goes down
    /// </summary>
    public Vector2 LocalToUV(Vector2 localPosition)
    {
        Rect rect = _rectTransform.rect;
        float u = (localPosition.x - rect.xMin) / rect.width;
        float v = (localPosition.y - rect.yMin) / rect.height;

        // Flip V because Unity UI Y-axis is inverted relative to texture coordinates
        v = 1.0f - v;

        return new Vector2(u, v);
    }

    /// <summary>
    /// Convert screen position directly to UV (combined operation)
    /// </summary>
    public bool ScreenToUV(Vector2 screenPosition, out Vector2 uv)
    {
        if (ScreenToLocal(screenPosition, out Vector2 localPos))
        {
            uv = LocalToUV(localPos);
            return true;
        }

        uv = Vector2.zero;
        return false;
    }
}
