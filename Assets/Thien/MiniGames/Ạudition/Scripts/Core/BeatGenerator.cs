using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SRP: Generate beats from phase data
/// Responsibility: Create beat list with random positions
/// </summary>
public static class BeatGenerator
{
    /// <summary>
    /// Generate beats for a phase with random positions
    /// </summary>
    public static List<BeatData> GenerateBeats(PhaseData phase, float phaseStartTime)
    {
        List<BeatData> beats = new List<BeatData>();

        // Initialize random with seed
        Random.State oldState = Random.state;
        if (phase.randomSeed != 0)
        {
            Random.InitState(phase.randomSeed);
        }

        // Generate beats
        for (int i = 0; i < phase.beatCount; i++)
        {
            float time = phaseStartTime + (i * phase.beatInterval);
            Vector2 position = GenerateRandomPosition(phase.positionRadius);

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

    private static Vector2 GenerateRandomPosition(float radius)
    {
        if (radius <= 0f)
        {
            return Vector2.zero;
        }

        // Random angle
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;

        // Random radius (uniform distribution)
        float r = Mathf.Sqrt(Random.Range(0f, 1f)) * radius;

        return new Vector2(
            Mathf.Cos(angle) * r,
            Mathf.Sin(angle) * r
        );
    }
}