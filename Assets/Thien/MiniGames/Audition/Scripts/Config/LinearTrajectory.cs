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

        // Get the size component along the direction (width for horizontal, height for vertical)
        // For angle 0° (horizontal), use beatSize.x
        // For angle 90° (vertical), use beatSize.y
        // For other angles, use the projection
        float sizeAlongDirection = Mathf.Abs(Mathf.Cos(rad)) * beatSize.x + Mathf.Abs(Mathf.Sin(rad)) * beatSize.y;

        // Calculate total length:
        // Total = (beatCount * beatSize) + ((beatCount - 1) * gap)
        float totalLength = (totalCount * sizeAlongDirection) + ((totalCount - 1) * gap);

        // Calculate start position (centered)
        Vector2 start = -direction * (totalLength * 0.5f);

        // Calculate position for this beat
        // Position = start + (index * (beatSize + gap))
        float offset = index * (sizeAlongDirection + gap);
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

        // Calculate total length for display
        float rad = angle * Mathf.Deg2Rad;
        float sizeAlongDirection = Mathf.Abs(Mathf.Cos(rad)) * beatSize.x + Mathf.Abs(Mathf.Sin(rad)) * beatSize.y;
        float totalLength = (beatCount * sizeAlongDirection) + ((beatCount - 1) * gap);

        trajectoryName = $"Linear {angle:F0}° (size={sizeAlongDirection:F0}, gap={gap:F0}, total={totalLength:F0})";
    }
#endif
}
