using UnityEngine;

/// <summary>
/// SRP: Linear trajectory configuration
/// Responsibility: Generate beat positions along a straight line
/// </summary>
[CreateAssetMenu(fileName = "Linear_Trajectory", menuName = "Audition/Trajectories/Linear")]
public class LinearTrajectory : TrajectoryConfig
{
    [Header("Linear Settings")]
    [Tooltip("Góc của đường thẳng (0° = right, 90° = up)")]
    [Range(0f, 360f)]
    public float angle = 0f;

    [Tooltip("Khoảng cách giữa các beat (gap)")]
    [Range(10f, 200f)]
    public float gap = 50f;

    public override Vector2 EvaluatePosition(float t, int index, int totalCount)
    {
        // Calculate direction from angle
        float rad = angle * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

        // ✅ Calculate total length based ONLY on gap (beat size handled by BeatConfig)
        float totalLength = (totalCount - 1) * gap;

        // Calculate start position (centered)
        Vector2 start = -direction * (totalLength * 0.5f);

        // Calculate position for this beat
        float offset = index * gap;
        Vector2 position = start + direction * offset;

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

        gap = Mathf.Max(0f, gap);
        angle = Mathf.Repeat(angle, 360f);

        // Calculate total length for display (gap only)
        float totalLength = (beatCount - 1) * gap;

        trajectoryName = $"Linear {angle:F0}° (gap={gap:F0}, beats={beatCount}, len={totalLength:F0})";
    }
#endif
}