using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SRP: Generate beat data from phase config
/// Responsibility: Convert PhaseData → List<BeatData>
/// ✅ Beat size comes from BeatConfig (auto-converted to world scale)
/// </summary>
public static class BeatGenerator
{
    public static List<BeatData> GenerateBeats(PhaseData phase, BeatConfig beatConfig, float phaseStartTime)
    {
        List<BeatData> beats = new List<BeatData>();

        if (phase.trajectoryConfigs == null || phase.trajectoryConfigs.Length == 0)
        {
            return beats;
        }

        float currentTime = phaseStartTime;

        // ✅ Get beat size from BeatConfig (already converted to world scale)
        Vector2 beatSizeWorld = Vector2.one * beatConfig.BeatSizeWorld;

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
                    size = beatSizeWorld, // ✅ World scale size (already divided by 100)
                    spriteSet = trajectoryConfig.beatSpriteSet
                };

                beats.Add(beat);
                currentTime += trajectoryConfig.beatInterval;
            }
        }

        return beats;
    }
}