using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SRP: Generate beats from phase data
/// Responsibility: Create beat list with random positions
/// </summary>
public static class BeatGenerator
{
    /// <summary>
    /// Generate beats for a phase using multiple trajectory configurations
    /// </summary>
    public static List<BeatData> GenerateBeats(PhaseData phase, float phaseStartTime)
    {
        if (phase.trajectoryConfigs == null || phase.trajectoryConfigs.Length == 0)
        {
            Debug.LogError($"[BeatGenerator] Phase '{phase.phaseName}' has no trajectory configs!");
            return new List<BeatData>();
        }

        List<BeatData> beats = new List<BeatData>();
        float currentTime = phaseStartTime;

        // Loop through all trajectory configs in this phase
        foreach (var trajectory in phase.trajectoryConfigs)
        {
            if (trajectory == null)
            {
                Debug.LogWarning($"[BeatGenerator] Null trajectory config in phase '{phase.phaseName}', skipping...");
                continue;
            }

            // Generate beats for this trajectory
            for (int i = 0; i < trajectory.beatCount; i++)
            {
                float time = currentTime + (i * trajectory.beatInterval);

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

            // Move time forward for next trajectory
            currentTime += trajectory.beatCount * trajectory.beatInterval;
        }

        return beats;
    }
}