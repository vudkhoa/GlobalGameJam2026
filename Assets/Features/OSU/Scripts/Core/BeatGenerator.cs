using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SRP: Generate beat data from phase config
/// Responsibility: Convert PhaseData → List<BeatData>
/// </summary>
public static class BeatGenerator
{
    public static List<BeatData> GenerateBeats(PhaseData phase, float phaseStartTime)
    {
        List<BeatData> beats = new List<BeatData>();

        if (phase.trajectoryConfigs == null || phase.trajectoryConfigs.Length == 0)
        {
            return beats;
        }

        float currentTime = phaseStartTime;

        foreach (var trajectoryConfig in phase.trajectoryConfigs)
        {
            if (trajectoryConfig == null)
            {
                continue;
            }

            // Generate beats for this trajectory
            for (int i = 0; i < trajectoryConfig.beatCount; i++)
            {
                float t = trajectoryConfig.beatCount > 1
                    ? i / (float)(trajectoryConfig.beatCount - 1)
                    : 0.5f;

                Vector2 position = trajectoryConfig.EvaluatePosition(t, i, trajectoryConfig.beatCount);

                BeatData beat = new BeatData
                {
                    time = currentTime,
                    position = position,
                    size = trajectoryConfig.beatSize,
                    spriteSet = trajectoryConfig.beatSpriteSet // ✅ NEW: Pass sprite set
                };

                beats.Add(beat);
                currentTime += trajectoryConfig.beatInterval;
            }
        }

        return beats;
    }
}