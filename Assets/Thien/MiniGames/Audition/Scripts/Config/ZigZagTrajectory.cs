using UnityEngine;

/// <summary>
/// SRP: ZigZag trajectory configuration
/// Responsibility: Generate beat positions in a zigzag pattern
/// </summary>
[CreateAssetMenu(fileName = "ZigZag_Trajectory", menuName = "Audition/Trajectories/ZigZag")]
public class ZigZagTrajectory : TrajectoryConfig
{
    [Header("ZigZag Settings")]
    [Tooltip("Hướng chính của zigzag (0° = right, 90° = up)")]
    [Range(0f, 360f)]
    public float direction = 0f;

    [Tooltip("Biên độ zigzag (độ rộng của mỗi zag)")]
    [Range(0f, 200f)]
    public float amplitude = 50f;

    [Tooltip("Số lần zigzag trong toàn bộ trajectory")]
    [Range(1, 20)]
    public int zigzagCount = 3;

    [Tooltip("Khoảng cách giữa các beat (gap)")]
    [Range(0f, 200f)]
    public float gap = 50f;

    [Header("Pattern")]
    [Tooltip("Kiểu zigzag (Sharp = góc nhọn, Smooth = sin wave)")]
    public ZigZagPattern pattern = ZigZagPattern.Sharp;

    public enum ZigZagPattern
    {
        Sharp,      // Triangle wave (góc nhọn)
        Smooth      // Sin wave (mượt)
    }

    public override Vector2 EvaluatePosition(float t, int index, int totalCount)
    {
        // Calculate main direction vector
        float rad = direction * Mathf.Deg2Rad;
        Vector2 mainDirection = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        Vector2 perpendicular = new Vector2(-mainDirection.y, mainDirection.x);

        // Get the size component along the direction
        float sizeAlongDirection = Mathf.Abs(Mathf.Cos(rad)) * beatSize.x + Mathf.Abs(Mathf.Sin(rad)) * beatSize.y;

        // Calculate total length from beatCount, beatSize, and gap
        float totalLength = (totalCount * sizeAlongDirection) + ((totalCount - 1) * gap);

        // Calculate start position (centered)
        Vector2 start = -mainDirection * (totalLength * 0.5f);

        // Calculate position for this beat along main direction
        float offset = index * (sizeAlongDirection + gap);
        float mainProgress = offset;

        // Calculate zigzag offset based on progress ratio
        float progressRatio = totalCount > 1 ? (float)index / (totalCount - 1) : 0.5f;
        float zigzagOffset;
        float zigzagPhase = progressRatio * zigzagCount;

        if (pattern == ZigZagPattern.Sharp)
        {
            // Triangle wave: -1 → 1 → -1
            float triangleWave = Mathf.PingPong(zigzagPhase * 2f, 2f) - 1f;
            zigzagOffset = triangleWave * amplitude;
        }
        else // Smooth
        {
            // Sin wave
            zigzagOffset = Mathf.Sin(zigzagPhase * Mathf.PI * 2f) * amplitude;
        }

        // Combine main direction + zigzag offset
        Vector2 position = start + mainDirection * mainProgress + perpendicular * zigzagOffset;

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

        direction = Mathf.Repeat(direction, 360f);
        amplitude = Mathf.Max(0f, amplitude);
        gap = Mathf.Max(0f, gap);
        zigzagCount = Mathf.Max(1, zigzagCount);

        // Calculate total length for display
        float rad = direction * Mathf.Deg2Rad;
        float sizeAlongDirection = Mathf.Abs(Mathf.Cos(rad)) * beatSize.x + Mathf.Abs(Mathf.Sin(rad)) * beatSize.y;
        float totalLength = (beatCount * sizeAlongDirection) + ((beatCount - 1) * gap);

        // Update trajectory name
        string patternName = pattern == ZigZagPattern.Sharp ? "Sharp" : "Smooth";
        trajectoryName = $"ZigZag {zigzagCount}x ({patternName}, gap={gap:F0}, total={totalLength:F0})";
    }
#endif
}
