using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SRP: Manage phase progression and beat generation
/// Responsibility: Track phase state, generate beat data, emit transition events
/// </summary>
public class PhaseController
{
    private PhaseData[] _phases;
    private int _currentPhaseIndex = -1;
    private int _beatsCompleted = 0;
    private int _totalBeatsInPhase = 0;

    private List<BeatData> _currentPhaseBeats = new List<BeatData>();

    // Events
    public event Action<PhaseData> OnPhaseStarted;
    public event Action<PhaseData, int> OnPhaseEnded;
    public event Action OnAllPhasesCompleted;
    public event Action<TrajectoryTransitionData> OnTrajectoryTransition;

    // Properties
    public int CurrentPhaseIndex => _currentPhaseIndex;
    public int TotalPhases => _phases?.Length ?? 0;
    public List<BeatData> CurrentPhaseBeats => _currentPhaseBeats;

    // ═══════════════════════════════════════════════════════════
    // INITIALIZATION
    // ═══════════════════════════════════════════════════════════

    public void Initialize(PhaseData[] phases)
    {
        _phases = phases;
        _currentPhaseIndex = -1;
        _beatsCompleted = 0;
        _totalBeatsInPhase = 0;
        _currentPhaseBeats.Clear();
    }

    // ═══════════════════════════════════════════════════════════
    // PUBLIC API
    // ═══════════════════════════════════════════════════════════

    public void StartFirstPhase()
    {
        if (_phases == null || _phases.Length == 0)
        {
            Debug.LogError("[PhaseController] No phases configured!");
            return;
        }

        _currentPhaseIndex = 0;
        StartPhase(_phases[0]);
    }

    public void StartNextPhase()
    {
        _currentPhaseIndex++;

        if (_currentPhaseIndex >= _phases.Length)
        {
            OnAllPhasesCompleted?.Invoke();
            return;
        }

        StartPhase(_phases[_currentPhaseIndex]);
    }

    public void OnBeatCompleted()
    {
        _beatsCompleted++;

        if (_beatsCompleted >= _totalBeatsInPhase)
        {
            EndCurrentPhase();
        }
    }

    // ═══════════════════════════════════════════════════════════
    // PRIVATE METHODS
    // ═══════════════════════════════════════════════════════════

    private void StartPhase(PhaseData phase)
    {
        _beatsCompleted = 0;
        _currentPhaseBeats.Clear();

        GenerateBeatsForPhase(phase);

        _totalBeatsInPhase = _currentPhaseBeats.Count;

        OnPhaseStarted?.Invoke(phase);
    }

    private void GenerateBeatsForPhase(PhaseData phase)
    {
        if (phase.trajectoryConfigs == null || phase.trajectoryConfigs.Length == 0)
        {
            Debug.LogWarning($"[PhaseController] Phase {phase.phaseName} has no trajectories!");
            return;
        }

        // ✅ Lấy connectorConfig từ Phase thay vì từ Trajectory
        BeatConnectorConfig phaseConnector = phase.connectorConfig;

        float currentTime = 0f;

        for (int trajIndex = 0; trajIndex < phase.trajectoryConfigs.Length; trajIndex++)
        {
            TrajectoryConfig trajectory = phase.trajectoryConfigs[trajIndex];

            if (trajectory == null) continue;

            Vector2 trajectoryEndPos = Vector2.zero;

            for (int beatIndex = 0; beatIndex < trajectory.beatCount; beatIndex++)
            {
                float t = trajectory.beatCount > 1
                    ? (float)beatIndex / (trajectory.beatCount - 1)
                    : 0.5f;

                Vector2 position = trajectory.EvaluatePosition(t, beatIndex, trajectory.beatCount);

                if (beatIndex == trajectory.beatCount - 1)
                {
                    trajectoryEndPos = position;
                }

                BeatData beatData = new BeatData
                {
                    time = currentTime,
                    position = position,
                    size = Vector2.one * 100f,
                    spriteSet = trajectory.beatSpriteSet
                };

                _currentPhaseBeats.Add(beatData);
                currentTime += trajectory.beatInterval;
            }

            // ✅ Emit transition event dùng phase-level connectorConfig
            if (trajIndex < phase.trajectoryConfigs.Length - 1 && phaseConnector != null)
            {
                TrajectoryConfig nextTrajectory = phase.trajectoryConfigs[trajIndex + 1];

                if (nextTrajectory != null)
                {
                    Vector2 nextTrajectoryStartPos = nextTrajectory.EvaluatePosition(0f, 0, nextTrajectory.beatCount);

                    TrajectoryTransitionData transitionData = new TrajectoryTransitionData
                    {
                        currentTrajectoryEndPos = trajectoryEndPos,
                        nextTrajectoryStartPos = nextTrajectoryStartPos,
                        connectorConfig = phaseConnector,
                        transitionTime = currentTime - trajectory.beatInterval
                    };

                    OnTrajectoryTransition?.Invoke(transitionData);
                }
            }
        }
    }

    private void EndCurrentPhase()
    {
        PhaseData currentPhase = _phases[_currentPhaseIndex];
        OnPhaseEnded?.Invoke(currentPhase, _currentPhaseIndex);
    }
}

/// <summary>
/// Data for trajectory transition (connector spawning)
/// </summary>
[System.Serializable]
public struct TrajectoryTransitionData
{
    public Vector2 currentTrajectoryEndPos;
    public Vector2 nextTrajectoryStartPos;
    public BeatConnectorConfig connectorConfig;
    public float transitionTime;
}