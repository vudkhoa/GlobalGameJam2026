using UnityEngine;

/// <summary>
/// SRP: Base class for all trajectory configurations
/// Responsibility: Define interface for beat position generation along trajectories
/// ✅ NOW: Each trajectory can specify its own sprite set
/// ✅ Beat SIZE is controlled by BeatConfig, NOT trajectory
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

    [Header("Visual Settings")]
    [Tooltip("Sprite set cho tất cả beats trong trajectory này (optional)")]
    public BeatSpriteSet beatSpriteSet;

    [Tooltip("Config để nối từ beat cuối trajectory này đến beat đầu trajectory tiếp theo")]
    public BeatConnectorData connectorToNext;

    /// <summary>
    /// Evaluate vị trí beat tại thời điểm t trong trajectory
    /// </summary>
    public abstract Vector2 EvaluatePosition(float t, int index, int totalCount);

    /// <summary>
    /// Get vị trí của beat cuối cùng trong trajectory
    /// </summary>
    public Vector2 GetLastBeatPosition()
    {
        return EvaluatePosition(1f, beatCount - 1, beatCount);
    }

    /// <summary>
    /// Get vị trí của beat đầu tiên trong trajectory
    /// </summary>
    public Vector2 GetFirstBeatPosition()
    {
        return EvaluatePosition(0f, 0, beatCount);
    }

#if UNITY_EDITOR
    protected virtual void OnValidate()
    {
        boundaryRadius = Mathf.Max(0f, boundaryRadius);
        beatCount = Mathf.Max(1, beatCount);
        beatInterval = Mathf.Max(0.3f, beatInterval);
    }
#endif
}