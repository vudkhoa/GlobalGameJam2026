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
    }

    /// <summary>
    /// Preview trajectory trong Scene view (optional)
    /// </summary>
    public virtual void DrawGizmos(int sampleCount = 20)
    {
        if (sampleCount <= 1) return;

        Vector2 prevPos = EvaluatePosition(0f, 0, sampleCount);

        for (int i = 1; i < sampleCount; i++)
        {
            float t = (float)i / (sampleCount - 1);
            Vector2 currentPos = EvaluatePosition(t, i, sampleCount);

            Gizmos.color = Color.Lerp(Color.green, Color.red, t);
            Gizmos.DrawLine(prevPos, currentPos);

            prevPos = currentPos;
        }

        // Draw boundary
        Gizmos.color = Color.yellow;
        DrawCircle(Vector2.zero, boundaryRadius, 32);
    }

    private void DrawCircle(Vector2 center, float radius, int segments)
    {
        float angleStep = 360f / segments;
        Vector2 prevPoint = center + new Vector2(radius, 0);

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector2 newPoint = center + new Vector2(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius
            );

            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }
#endif
}
