using UnityEngine;

/// <summary>
/// SRP: Base class for all trajectory configurations
/// Responsibility: Define interface for beat position generation along trajectories
/// </summary>
public abstract class TrajectoryConfig : ScriptableObject
{
    [Header("Trajectory Settings")]
    [Tooltip("Tên trajectory (để debug/display)")]
    public string trajectoryName = "Trajectory";

    [Tooltip("Giới hạn vùng spawning (boundary radius)")]
    [Range(0f, 500f)]
    public float boundaryRadius = 100f;

    [Tooltip("Có scale trajectory về boundaryRadius không")]
    public bool normalizeToRadius = false;

    [Header("Beat Settings")]
    [Tooltip("Số lượng beat trong trajectory")]
    [Range(1, 100)]
    public int beatCount = 10;

    [Tooltip("Khoảng thời gian giữa các beat (giây)")]
    [Range(0.3f, 3f)]
    public float beatInterval = 1f;

    [Tooltip("Kích thước của beat (sizeDelta của RectTransform)")]
    public Vector2 beatSize = new Vector2(100f, 100f);

    /// <summary>
    /// Evaluate vị trí beat tại thời điểm t trong trajectory
    /// </summary>
    /// <param name="t">Normalized time [0, 1] trong trajectory</param>
    /// <param name="index">Thứ tự beat (0-based)</param>
    /// <param name="totalCount">Tổng số beat trong phase</param>
    /// <returns>Local position của beat</returns>
    public abstract Vector2 EvaluatePosition(float t, int index, int totalCount);

#if UNITY_EDITOR
    /// <summary>
    /// Validate config trong Editor
    /// </summary>
    protected virtual void OnValidate()
    {
        boundaryRadius = Mathf.Max(0f, boundaryRadius);
        beatCount = Mathf.Max(1, beatCount);
        beatInterval = Mathf.Max(0.3f, beatInterval);
        beatSize.x = Mathf.Max(10f, beatSize.x);
        beatSize.y = Mathf.Max(10f, beatSize.y);
    }
#endif
}
