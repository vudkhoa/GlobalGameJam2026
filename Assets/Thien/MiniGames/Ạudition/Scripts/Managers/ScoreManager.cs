using System;

/// <summary>
/// SRP: Track score, combo, statistics
/// Responsibility: Manage score/combo state
/// </summary>
public class ScoreManager
{
    private JudgementConfig _config;

    private int _totalScore = 0;
    private int _currentPhaseScore = 0;
    private int _currentCombo = 0;
    private int _maxCombo = 0;

    private int _perfectCount = 0;
    private int _goodCount = 0;
    private int _okCount = 0;
    private int _missCount = 0;

    public int TotalScore => _totalScore;
    public int CurrentCombo => _currentCombo;
    public int MaxCombo => _maxCombo;

    public event Action<int> OnScoreChanged;
    public event Action<int> OnComboChanged;

    // ═══════════════════════════════════════════════════════════
    // INITIALIZATION
    // ═══════════════════════════════════════════════════════════

    public ScoreManager(JudgementConfig config)
    {
        _config = config;
    }

    // ═══════════════════════════════════════════════════════════
    // SCORE TRACKING
    // ═══════════════════════════════════════════════════════════

    public void RecordJudgement(JudgementType judgement, int score)
    {
        // Update stats
        switch (judgement)
        {
            case JudgementType.Perfect:
                _perfectCount++;
                _currentCombo++;
                break;
            case JudgementType.Good:
                _goodCount++;
                _currentCombo++;
                break;
            case JudgementType.OK:
                _okCount++;
                _currentCombo = 0;
                break;
            case JudgementType.Miss:
                _missCount++;
                _currentCombo = 0;
                break;
        }

        // Update score
        _totalScore += score;
        _currentPhaseScore += score;

        // Update max combo
        if (_currentCombo > _maxCombo)
            _maxCombo = _currentCombo;

        // Trigger events
        OnScoreChanged?.Invoke(_totalScore);
        OnComboChanged?.Invoke(_currentCombo);
    }

    public bool IsComboActive()
    {
        return _currentCombo >= _config.comboThreshold;
    }

    // ═══════════════════════════════════════════════════════════
    // PHASE MANAGEMENT
    // ═══════════════════════════════════════════════════════════

    public int GetCurrentScore()
    {
        return _currentPhaseScore;
    }

    public void ResetPhaseScore()
    {
        _currentPhaseScore = 0;
    }

    // ═══════════════════════════════════════════════════════════
    // STATS
    // ═══════════════════════════════════════════════════════════

    public void LogStats()
    {
        UnityEngine.Debug.Log("═══════════════════════════════════════════════════════");
        UnityEngine.Debug.Log($"[ScoreManager] FINAL STATISTICS:");
        UnityEngine.Debug.Log($"  Total Score: {_totalScore}");
        UnityEngine.Debug.Log($"  Max Combo: {_maxCombo}");
        UnityEngine.Debug.Log($"  Perfect: {_perfectCount} | Good: {_goodCount} | OK: {_okCount} | Miss: {_missCount}");
        UnityEngine.Debug.Log("═══════════════════════════════════════════════════════");
    }
}