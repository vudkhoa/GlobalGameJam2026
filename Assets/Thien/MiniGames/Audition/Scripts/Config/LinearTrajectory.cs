using UnityEngine;

/// <summary>
/// SRP: Linear trajectory configuration
/// Responsibility: Generate beat positions along a straight line
/// </summary>
[CreateAssetMenu(fileName = "Linear_Trajectory", menuName = "Audition/Trajectories/Linear")]
public class LinearTrajectory : TrajectoryConfig
{
    [Header("Linear Settings")]
    [Tooltip("Điểm bắt đầu của đường thẳng")]
    public Vector2 startPoint = new Vector2(-100f, 0f);

    [Tooltip("Điểm kết thúc của đường thẳng")]
    public Vector2 endPoint = new Vector2(100f, 0f);

    [Header("Alternative: Use Angle")]
    [Tooltip("Sử dụng angle thay vì explicit points")]
    public bool useAngle = false;

    [Tooltip("Góc của đường thẳng (0° = right, 90° = up)")]
    [Range(0f, 360f)]
    public float angle = 0f;

    [Tooltip("Độ dài của đường thẳng khi dùng angle")]
    [Range(0f, 500f)]
    public float length = 200f;

    public override Vector2 EvaluatePosition(float t, int index, int totalCount)
    {
        Vector2 start, end;

        if (useAngle)
        {
            // Tính start/end từ angle và length
            float rad = angle * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

            start = -direction * (length * 0.5f);
            end = direction * (length * 0.5f);
        }
        else
        {
            start = startPoint;
            end = endPoint;
        }

        // Linear interpolation
        Vector2 position = Vector2.Lerp(start, end, t);

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

        length = Mathf.Max(0f, length);
        angle = Mathf.Repeat(angle, 360f);

        // Update trajectory name
        if (useAngle)
            trajectoryName = $"Linear {angle:F0}° ({length:F0}u)";
        else
            trajectoryName = $"Linear ({startPoint} → {endPoint})";
    }
#endif
}
