using UnityEngine;

/// <summary>
/// SRP: Base class for all trajectory configurations
/// Responsibility: Define interface & dynamic screen bound calculations
/// </summary>
public abstract class TrajectoryConfig : ScriptableObject
{
    [Header("Trajectory Settings")]
    public string trajectoryName = "Trajectory";

    [Header("Dynamic Screen Settings")]
    [Tooltip("Khoảng cách an toàn thụt vào từ mép màn hình (World Unit)")]
    [Range(0f, 2f)]
    public float safePadding = 0.5f;

    // Các biến cũ giữ lại fallback
    [HideInInspector] public float maxX = 8.5f;
    [HideInInspector] public float maxY = 4.5f;
    [HideInInspector] public float screenPadding = 0.5f;

    [Header("Beat Settings")]
    [Range(1, 100)]
    public int beatCount = 10;

    [Range(0.3f, 3f)]
    public float beatInterval = 1f;

    [Header("Visual Settings")]
    public BeatSpriteSet beatSpriteSet;

    // =========================================================================
    // ⚡ TỐI ƯU HIỆU NĂNG (FRAME CACHING)
    // =========================================================================
    private Vector2? _cachedBounds;
    private int _lastFrameCalculated = -1;

    protected Vector2 GetDynamicScreenBounds()
    {
        if (_cachedBounds.HasValue && Time.frameCount == _lastFrameCalculated)
        {
            return _cachedBounds.Value;
        }

        Camera cam = Camera.main;

        if (cam == null)
        {
            return new Vector2(maxX, maxY);
        }

        Vector3 topRight = cam.ViewportToWorldPoint(new Vector3(1, 1, 0));

        _cachedBounds = new Vector2(topRight.x - safePadding, topRight.y - safePadding);
        _lastFrameCalculated = Time.frameCount;

        return _cachedBounds.Value;
    }

    public abstract Vector2 EvaluatePosition(float t, int index, int totalCount);

    protected Vector2 ClampToScreenBounds(Vector2 position)
    {
        Vector2 bounds = GetDynamicScreenBounds();

        float clampedX = Mathf.Clamp(position.x, -bounds.x, bounds.x);
        float clampedY = Mathf.Clamp(position.y, -bounds.y, bounds.y);

        return new Vector2(clampedX, clampedY);
    }

#if UNITY_EDITOR
    protected virtual void OnValidate()
    {
        beatCount = Mathf.Max(1, beatCount);
        beatInterval = Mathf.Max(0.1f, beatInterval);

        _cachedBounds = null;
        _lastFrameCalculated = -1;
    }
#endif
}