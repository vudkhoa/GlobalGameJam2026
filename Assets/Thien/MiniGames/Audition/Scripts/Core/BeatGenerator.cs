using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SRP: Generate beats from phase data
/// Responsibility: Create beat list with random positions
/// </summary>
public static class BeatGenerator
{
    /// <summary>
    /// Generate beats for a phase using trajectory configuration
    /// </summary>
    public static List<BeatData> GenerateBeats(PhaseData phase, float phaseStartTime)
    {
        if (phase.trajectoryConfig == null)
        {
            Debug.LogError($"[BeatGenerator] Phase '{phase.phaseName}' has no trajectory config!");
            return new List<BeatData>();
        }

        List<BeatData> beats = new List<BeatData>();
        TrajectoryConfig trajectory = phase.trajectoryConfig;

        // Generate beats using trajectory settings
        for (int i = 0; i < trajectory.beatCount; i++)
        {
            float time = phaseStartTime + (i * trajectory.beatInterval);

            // Get position from trajectory
            float t = trajectory.beatCount > 1 ? (float)i / (trajectory.beatCount - 1) : 0.5f;
            Vector2 position = trajectory.EvaluatePosition(t, i, trajectory.beatCount);

            beats.Add(new BeatData
            {
                time = time,
                position = position,
                size = trajectory.beatSize
            });
        }

        return beats;
    }
}