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

    private List<BeatData> _currentPhaseBeats;
    private BeatConfig _defaultBeatConfig;

    // ✅ NEW: Reference to GameTimeService để đồng bộ timing
    private GameTimeService _timeService;

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

    public void Initialize(List<PhaseData> phases, BeatConfig defaultBeatConfig, GameTimeService timeService)
    {
        _phases = phases ?? new List<PhaseData>();
        _defaultBeatConfig = defaultBeatConfig;
        _timeService = timeService;

        if (_phases.Count == 0)
        {
            return;
        }

        _currentPhaseIndex = -1;
        _beatsCompletedInPhase = 0;
        CurrentState = GameState.Idle;
    }

    // ═══════════════════════════════════════════════════════════
    // STATE TRANSITIONS
    // ═══════════════════════════════════════════════════════════

    public void StartFirstPhase()
    {
        if (_phases.Count == 0)
        {
            return;
        }

        StartPhase(0);
    }

    private void StartPhase(int phaseIndex)
    {
        if (phaseIndex >= _phases.Count)
        {
            return;
        }

        _currentPhaseIndex = phaseIndex;
        _beatsCompletedInPhase = 0;
        CurrentState = GameState.PlayingPhase;

        PhaseData phase = CurrentPhase;

        // ✅ Get BeatConfig for this phase (or use default)
        BeatConfig beatConfig = phase.beatConfig != null ? phase.beatConfig : _defaultBeatConfig;

        // ✅ FIX: Sử dụng GameTimeService.CurrentTime thay vì tính toán thủ công
        // Đây là nguồn sự thật duy nhất về thời gian trong game
        float currentGameTime = _timeService != null ? _timeService.CurrentTime : 0f;

        // Generate beats using BeatGenerator with ACTUAL game time
        _currentPhaseBeats = BeatGenerator.GenerateBeats(phase, beatConfig, currentGameTime);

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

        OnPhaseEnded?.Invoke(completedPhase, _currentPhaseIndex);

        // ✅ REMOVED: Không cần tính toán _currentPhaseStartTime nữa
        // Thay vào đó, mỗi phase sẽ sử dụng GameTimeService.CurrentTime khi start

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
            return;
        }

        StartPhase(_currentPhaseIndex + 1);
    }
}