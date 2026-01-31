using UnityEngine;

/// <summary>
/// SRP: Circular trajectory configuration
/// Responsibility: Generate beat positions along a circular arc
/// </summary>
[CreateAssetMenu(fileName = "Circular_Trajectory", menuName = "Audition/Trajectories/Circular")]
public class CircularTrajectory : TrajectoryConfig
{
    [Header("Circular Settings")]
    [Tooltip("Bán kính đường tròn")]
    [Range(0f, 500f)]
    public float radius = 100f;

    [Tooltip("Góc bắt đầu (0° = right, 90° = up)")]
    [Range(0f, 360f)]
    public float startAngle = 0f;

    [Tooltip("Góc kết thúc")]
    [Range(0f, 360f)]
    public float endAngle = 360f;

    [Tooltip("Chiều quay (true = clockwise, false = counter-clockwise)")]
    public bool clockwise = true;

    [Header("Center Offset")]
    [Tooltip("Offset tâm đường tròn")]
    public Vector2 centerOffset = Vector2.zero;

    public override Vector2 EvaluatePosition(float t, int index, int totalCount)
    {
        // Calculate angle at time t
        float currentAngle = Mathf.Lerp(startAngle, endAngle, t);

        // Apply clockwise/counter-clockwise
        if (!clockwise)
        {
            currentAngle = -currentAngle;
        }

        // Convert to radians
        float rad = currentAngle * Mathf.Deg2Rad;

        // Calculate position on circle
        Vector2 position = new Vector2(
            Mathf.Cos(rad) * radius,
            Mathf.Sin(rad) * radius
        );

        // Apply center offset
        position += centerOffset;

        // Apply boundary radius if needed
        if (normalizeToRadius && boundaryRadius > 0f)
        {
            float currentDistance = position.magnitude;
            if (currentDistance > boundaryRadius)
            {
                position = position.normalized * boundaryRadius;
            }
        }

        return position;
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();

        radius = Mathf.Max(0f, radius);
        startAngle = Mathf.Repeat(startAngle, 360f);
        endAngle = Mathf.Repeat(endAngle, 360f);

        // Update trajectory name
        float arcLength = Mathf.Abs(endAngle - startAngle);
        string direction = clockwise ? "CW" : "CCW";
        trajectoryName = $"Circular {radius:F0}u ({arcLength:F0}° {direction})";
    }

    public override void DrawGizmos(int sampleCount = 20)
    {
        base.DrawGizmos(sampleCount);

        // Draw center
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(centerOffset, 5f);
    }
#endif
}
