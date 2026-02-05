using UnityEngine;

/// <summary>
/// SRP: Wave trajectory configuration
/// Responsibility: Generate beat positions in a flowing wave pattern (left to right or right to left)
/// </summary>
[CreateAssetMenu(fileName = "Wave_Trajectory", menuName = "Audition/Trajectories/Wave")]
public class WaveTrajectory : TrajectoryConfig
{
    [Header("Wave Settings")]
    [Tooltip("Hướng di chuyển (true = right, false = left)")]
    public bool moveRight = true;

    [Tooltip("Khoảng cách giữa các beat (gap)")]
    [Range(0f, 200f)]
    public float gap = 50f;

    [Tooltip("Biên độ dao động theo trục Y")]
    [Range(0f, 300f)]
    public float waveAmplitude = 100f;

    [Tooltip("Tần số dao động (số lần lên xuống)")]
    [Range(0.1f, 5f)]
    public float waveFrequency = 1f;

    [Header("Path Constraints")]
    [Tooltip("Giới hạn Y tối thiểu (không đi quá thấp)")]
    [Range(-500f, 0f)]
    public float minY = -200f;

    [Tooltip("Giới hạn Y tối đa (không đi quá cao)")]
    [Range(0f, 500f)]
    public float maxY = 200f;

    [Header("Randomness")]
    [Tooltip("Seed cho random path (0 = random mỗi lần)")]
    public int pathSeed = 0;

    [Tooltip("Độ mạnh của random variation (0 = smooth, 1 = chaotic)")]
    [Range(0f, 1f)]
    public float randomness = 0.3f;

    public override Vector2 EvaluatePosition(float t, int index, int totalCount)
    {
        // Initialize random with seed
        Random.State oldState = Random.state;
        if (pathSeed != 0)
        {
            Random.InitState(pathSeed + index);
        }

        // ✅ Calculate total length based ONLY on gap
        float totalLength = (totalCount - 1) * gap;

        // Calculate X position (linear progression)
        float xOffset = index * gap;
        float xPosition;

        if (moveRight)
        {
            // Start from left, move right
            xPosition = -totalLength * 0.5f + xOffset;
        }
        else
        {
            // Start from right, move left
            xPosition = totalLength * 0.5f - xOffset;
        }

        // Calculate Y position (wave with randomness)
        float baseWave = Mathf.Sin(t * waveFrequency * Mathf.PI * 2f) * waveAmplitude;

        // Add Perlin noise for organic variation
        float noiseValue = Mathf.PerlinNoise(index * 0.5f, pathSeed * 0.1f);
        float randomVariation = (noiseValue - 0.5f) * waveAmplitude * randomness;

        float yPosition = baseWave + randomVariation;

        // Clamp Y to stay within bounds
        yPosition = Mathf.Clamp(yPosition, minY, maxY);

        // ✅ Smooth connection between beats (use gap instead of beatSize)
        if (index > 0)
        {
            // Get previous beat position
            float prevT = totalCount > 1 ? (float)(index - 1) / (totalCount - 1) : 0f;
            Vector2 prevPosition = GetPreviousPosition(prevT, index - 1, totalCount);

            // Limit Y change to create smooth flow (use gap as reference)
            float maxYChange = gap * 1.5f; // Max vertical jump
            float yDiff = yPosition - prevPosition.y;

            if (Mathf.Abs(yDiff) > maxYChange)
            {
                yPosition = prevPosition.y + Mathf.Sign(yDiff) * maxYChange;
            }

            // Re-clamp after smoothing
            yPosition = Mathf.Clamp(yPosition, minY, maxY);
        }

        Vector2 position = new Vector2(xPosition, yPosition);

        // Restore random state
        Random.state = oldState;

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

    // Helper to get previous beat position for smooth flow
    private Vector2 GetPreviousPosition(float t, int index, int totalCount)
    {
        Random.State oldState = Random.state;
        if (pathSeed != 0)
        {
            Random.InitState(pathSeed + index);
        }

        float totalLength = (totalCount - 1) * gap;
        float xOffset = index * gap;

        float xPosition = moveRight
            ? -totalLength * 0.5f + xOffset
            : totalLength * 0.5f - xOffset;

        float baseWave = Mathf.Sin(t * waveFrequency * Mathf.PI * 2f) * waveAmplitude;
        float noiseValue = Mathf.PerlinNoise(index * 0.5f, pathSeed * 0.1f);
        float randomVariation = (noiseValue - 0.5f) * waveAmplitude * randomness;
        float yPosition = Mathf.Clamp(baseWave + randomVariation, minY, maxY);

        Random.state = oldState;
        return new Vector2(xPosition, yPosition);
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();

        gap = Mathf.Max(0f, gap);
        waveAmplitude = Mathf.Max(0f, waveAmplitude);
        waveFrequency = Mathf.Max(0.1f, waveFrequency);

        // Ensure min < max
        if (minY > maxY)
        {
            float temp = minY;
            minY = maxY;
            maxY = temp;
        }

        // Calculate total length for display (gap only)
        float totalLength = (beatCount - 1) * gap;

        string direction = moveRight ? "Right" : "Left";
        trajectoryName = $"Wave {direction} (amp={waveAmplitude:F0}, freq={waveFrequency:F1}, beats={beatCount}, len={totalLength:F0})";
    }
#endif
}