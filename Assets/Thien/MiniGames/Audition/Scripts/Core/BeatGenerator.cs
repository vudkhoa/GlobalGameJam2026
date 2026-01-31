using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SRP: Generate beats from phase data
/// Responsibility: Create beat list with random positions
/// </summary>
public static class BeatGenerator
{
    /// <summary>
    /// Generate beats for a phase with trajectory or random positions
    /// </summary>
    public static List<BeatData> GenerateBeats(PhaseData phase, float phaseStartTime)
    {
        List<BeatData> beats = new List<BeatData>();

        // Initialize random with seed (for fallback random mode)
        Random.State oldState = Random.state;
        if (phase.randomSeed != 0)
        {
            Random.InitState(phase.randomSeed);
        }

        // Generate beats
        for (int i = 0; i < phase.beatCount; i++)
        {
            float time = phaseStartTime + (i * phase.beatInterval);

            // ✅ Use trajectory if available, fallback to random
            Vector2 position = GeneratePositionFromTrajectory(
                phase.trajectoryConfig,
                i,
                phase.beatCount,
                phase.positionRadius
            );

            beats.Add(new BeatData
            {
                time = time,
                position = position
            });
        }

        // Restore random state
        Random.state = oldState;

        return beats;
    }

    /// <summary>
    /// Generate position from trajectory config or fallback to random
    /// </summary>
    private static Vector2 GeneratePositionFromTrajectory(
        TrajectoryConfig trajectory,
        int index,
        int totalCount,
        float fallbackRadius)
    {
        // Use trajectory if available
        if (trajectory != null)
        {
            float t = totalCount > 1 ? (float)index / (totalCount - 1) : 0.5f;
            return trajectory.EvaluatePosition(t, index, totalCount);
        }

        // Fallback to random (backward compatibility)
        return GenerateRandomPosition(fallbackRadius);
    }

    // Random theo RECTANGLE thay vì CIRCLE
    private static Vector2 GenerateRandomPosition(float radius)
    {
        if (radius <= 0f)
        {
            return Vector2.zero;
        }

        // ✅ Random theo rectangular area (phủ toàn bộ container)
        float x = Random.Range(-radius, radius);
        float y = Random.Range(-radius, radius);

        return new Vector2(x, y);
    }

    //private static Vector2 GenerateRandomPositionCircle(float radius)
    //{
    //    if (radius <= 0f)
    //    {
    //        return Vector2.zero;
    //    }

    //    float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
    //    float r = Mathf.Sqrt(Random.Range(0f, 1f)) * radius;

    //    return new Vector2(
    //        Mathf.Cos(angle) * r,
    //        Mathf.Sin(angle) * r
    //    );
    //}
}