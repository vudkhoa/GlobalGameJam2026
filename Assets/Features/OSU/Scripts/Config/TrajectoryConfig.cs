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
    public BeatConnectorData connectorToNext;

    // =========================================================================
    // ⚡ TỐI ƯU HIỆU NĂNG (FRAME CACHING)
    // =========================================================================
    // Biến lưu trữ kết quả tính toán của frame hiện tại
    private Vector2? _cachedBounds;
    // Biến đánh dấu frame nào đã tính toán rồi
    private int _lastFrameCalculated = -1;

    /// <summary>
    /// Hàm cốt lõi: Lấy giới hạn màn hình thực tế (World Space)
    /// Đã tối ưu để chỉ gọi Camera.main 1 lần duy nhất mỗi frame
    /// </summary>
    protected Vector2 GetDynamicScreenBounds()
    {
        // 1. Kiểm tra Cache: Nếu frame này đã tính rồi thì trả về luôn (Siêu nhanh)
        if (_cachedBounds.HasValue && Time.frameCount == _lastFrameCalculated)
        {
            return _cachedBounds.Value;
        }

        // 2. Nếu chưa tính (Frame mới), thực hiện tính toán nặng
        Camera cam = Camera.main;

        // Fallback nếu không tìm thấy cam (để tránh lỗi trong Editor khi chưa Play)
        if (cam == null)
        {
            return new Vector2(maxX, maxY);
        }

        // Viewport (1,1) là góc trên phải. Chuyển sang World Space.
        Vector3 topRight = cam.ViewportToWorldPoint(new Vector3(1, 1, 0));

        // Lưu kết quả vào Cache
        _cachedBounds = new Vector2(topRight.x - safePadding, topRight.y - safePadding);
        _lastFrameCalculated = Time.frameCount;

        return _cachedBounds.Value;
    }

    public abstract Vector2 EvaluatePosition(float t, int index, int totalCount);

    /// <summary>
    /// Clamp vị trí vào trong giới hạn dynamic
    /// </summary>
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

        // Reset cache khi chỉnh sửa trong Editor để cập nhật ngay lập tức
        _cachedBounds = null;
        _lastFrameCalculated = -1;
    }
#endif
}