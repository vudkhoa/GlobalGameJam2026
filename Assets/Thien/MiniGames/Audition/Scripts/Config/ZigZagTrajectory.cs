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

    [Tooltip("Độ dài tổng của trajectory")]
    [Range(0f, 500f)]
    public float totalLength = 200f;

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

        // Progress along main direction
        float mainProgress = t * totalLength;

        // Calculate zigzag offset
        float zigzagOffset;
        float zigzagPhase = t * zigzagCount;

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
        Vector2 position = mainDirection * (mainProgress - totalLength * 0.5f)
                         + perpendicular * zigzagOffset;

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
        totalLength = Mathf.Max(0f, totalLength);
        zigzagCount = Mathf.Max(1, zigzagCount);

        // Update trajectory name
        string patternName = pattern == ZigZagPattern.Sharp ? "Sharp" : "Smooth";
        trajectoryName = $"ZigZag {zigzagCount}x ({patternName})";
    }
#endif
}
