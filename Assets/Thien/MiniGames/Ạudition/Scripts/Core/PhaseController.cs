using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// SRP: Control phase progression (FSM)
/// Responsibility: Track phase state, handle transitions
/// </summary>
public class PhaseController
{
    private List<PhaseData> _phases;
    private int _currentPhaseIndex = -1;
    private int _beatsCompletedInPhase = 0;
    private float _currentPhaseStartTime = 0f;

    private List<BeatData> _currentPhaseBeats;

    public GameState CurrentState { get; private set; } = GameState.Idle;
    public PhaseData CurrentPhase => _currentPhaseIndex >= 0 ? _phases[_currentPhaseIndex] : null;
    public List<BeatData> CurrentPhaseBeats => _currentPhaseBeats;
    public int CurrentPhaseIndex => _currentPhaseIndex;
    public int TotalPhases => _phases.Count;

    public event Action<PhaseData> OnPhaseStarted;
    public event Action<PhaseData, int> OnPhaseEnded;
    public event Action OnAllPhasesCompleted;

    // ═══════════════════════════════════════════════════════════
    // INITIALIZATION
    // ═══════════════════════════════════════════════════════════

    public void Initialize(List<PhaseData> phases)
    {
        _phases = phases ?? new List<PhaseData>();

        if (_phases.Count == 0)
        {
            Debug.LogError("[PhaseController] No phases provided!");
            return;
        }

        _currentPhaseIndex = -1;
        _beatsCompletedInPhase = 0;
        CurrentState = GameState.Idle;

        Debug.Log($"[PhaseController] Initialized with {_phases.Count} phases");
    }

    // ═══════════════════════════════════════════════════════════
    // STATE TRANSITIONS
    // ═══════════════════════════════════════════════════════════

    public void StartFirstPhase()
    {
        if (_phases.Count == 0)
        {
            Debug.LogError("[PhaseController] No phases to start!");
            return;
        }

        _currentPhaseStartTime = 0f;
        StartPhase(0);
    }

    private void StartPhase(int phaseIndex)
    {
        if (phaseIndex >= _phases.Count)
        {
            Debug.LogError($"[PhaseController] Invalid phase index: {phaseIndex}");
            return;
        }

        _currentPhaseIndex = phaseIndex;
        _beatsCompletedInPhase = 0;
        CurrentState = GameState.PlayingPhase;

        PhaseData phase = CurrentPhase;

        // Generate beats using BeatGenerator
        _currentPhaseBeats = BeatGenerator.GenerateBeats(phase, _currentPhaseStartTime);

        Debug.Log($"[PhaseController] Starting {phase.phaseName} ({_currentPhaseBeats.Count} beats, duration: {phase.TotalDuration:F1}s)");

        OnPhaseStarted?.Invoke(phase);
    }

    public void OnBeatCompleted()
    {
        if (CurrentState != GameState.PlayingPhase) return;

        _beatsCompletedInPhase++;

        // Check if phase completed
        if (_beatsCompletedInPhase >= _currentPhaseBeats.Count)
        {
            EndCurrentPhase();
        }
    }

    private void EndCurrentPhase()
    {
        PhaseData completedPhase = CurrentPhase;
        //Debug.Log($"[PhaseController] {completedPhase.phaseName} completed ({_beatsCompletedInPhase} beats)");

        OnPhaseEnded?.Invoke(completedPhase, _currentPhaseIndex);

        // Update start time for next phase
        _currentPhaseStartTime += completedPhase.TotalDuration;

        // Check if more phases exist
        if (_currentPhaseIndex < _phases.Count - 1)
        {
            CurrentState = GameState.PausingPhase;
        }
        else
        {
            CurrentState = GameState.Ended;
            OnAllPhasesCompleted?.Invoke();
        }
    }

    public void StartNextPhase()
    {
        if (CurrentState != GameState.PausingPhase)
        {
            Debug.LogWarning("[PhaseController] Not in pausing state!");
            return;
        }

        StartPhase(_currentPhaseIndex + 1);
    }
}