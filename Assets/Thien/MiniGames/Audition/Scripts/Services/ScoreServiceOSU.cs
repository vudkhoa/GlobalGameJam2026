using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// OSU-style scoring implementation
/// Score = BaseScore × ComboMultiplier × AccuracyBonus
/// </summary>
public class ScoreServiceOSU : IScoreService
{
    private JudgementConfig _config;

    // Score tracking
    private int _totalScore;
    private int _currentPhaseScore;
    private List<int> _phaseScores = new List<int>();

    // Combo tracking
    private int _currentCombo;
    private int _maxCombo;

    // Judgement counts
    private Dictionary<JudgementType, int> _judgementCounts = new Dictionary<JudgementType, int>
    {
        { JudgementType.Perfect, 0 },
        { JudgementType.Good, 0 },
        { JudgementType.OK, 0 },
        { JudgementType.Miss, 0 }
    };

    // OSU-specific settings
    private const float COMBO_MULTIPLIER_BASE = 1f;
    private const float COMBO_MULTIPLIER_INCREMENT = 0.01f; // +1% per combo
    private const float MAX_COMBO_MULTIPLIER = 4f; // Max 4x at 300 combo

    public ScoreServiceOSU(JudgementConfig config)
    {
        _config = config;
    }

    // ═══════════════════════════════════════════════════════════
    // INTERFACE IMPLEMENTATION
    // ═══════════════════════════════════════════════════════════

    public int RecordJudgement(JudgementType judgement)
    {
        // Update judgement count
        _judgementCounts[judgement]++;

        // Calculate base score
        int baseScore = GetBaseScore(judgement);

        // Update combo
        if (judgement == JudgementType.Miss)
        {
            _currentCombo = 0;
        }
        else
        {
            _currentCombo++;
            if (_currentCombo > _maxCombo)
            {
                _maxCombo = _currentCombo;
            }
        }

        // Calculate combo multiplier (OSU-style)
        float comboMultiplier = CalculateComboMultiplier(_currentCombo);

        // Calculate accuracy bonus
        float accuracyBonus = CalculateAccuracyBonus(judgement);

        // Final score = BaseScore × ComboMultiplier × AccuracyBonus
        int finalScore = Mathf.RoundToInt(baseScore * comboMultiplier * accuracyBonus);

        // Add to totals
        _currentPhaseScore += finalScore;
        _totalScore += finalScore;

        return finalScore;
    }

    public int GetTotalScore() => _totalScore;

    public int GetCurrentCombo() => _currentCombo;

    public int GetMaxCombo() => _maxCombo;

    public void RecordPhaseScore(int phaseIndex, int score)
    {
        // Ensure list is large enough
        while (_phaseScores.Count <= phaseIndex)
        {
            _phaseScores.Add(0);
        }

        _phaseScores[phaseIndex] = score;
    }

    public int GetPhaseScore(int phaseIndex)
    {
        if (phaseIndex >= 0 && phaseIndex < _phaseScores.Count)
        {
            return _phaseScores[phaseIndex];
        }
        return 0;
    }

    public List<int> GetAllPhaseScores()
    {
        return new List<int>(_phaseScores);
    }

    public void ResetPhaseScore()
    {
        _currentPhaseScore = 0;
    }

    public void ResetAll()
    {
        _totalScore = 0;
        _currentPhaseScore = 0;
        _currentCombo = 0;
        _maxCombo = 0;
        _phaseScores.Clear();

        foreach (var key in new List<JudgementType>(_judgementCounts.Keys))
        {
            _judgementCounts[key] = 0;
        }
    }

    public int GetJudgementCount(JudgementType type)
    {
        return _judgementCounts.ContainsKey(type) ? _judgementCounts[type] : 0;
    }

    public float GetAccuracy()
    {
        int totalHits = 0;
        float weightedHits = 0f;

        foreach (var kvp in _judgementCounts)
        {
            totalHits += kvp.Value;

            // Weight: Perfect = 1.0, Good = 0.7, OK = 0.3, Miss = 0
            float weight = kvp.Key switch
            {
                JudgementType.Perfect => 1.0f,
                JudgementType.Good => 0.7f,
                JudgementType.OK => 0.3f,
                _ => 0f
            };

            weightedHits += kvp.Value * weight;
        }

        return totalHits > 0 ? (weightedHits / totalHits) * 100f : 0f;
    }

    // ═══════════════════════════════════════════════════════════
    // OSU-SPECIFIC CALCULATIONS
    // ═══════════════════════════════════════════════════════════

    private int GetBaseScore(JudgementType judgement)
    {
        return judgement switch
        {
            JudgementType.Perfect => _config.perfectScore,
            JudgementType.Good => _config.goodScore,
            JudgementType.OK => _config.okScore,
            _ => _config.missScore
        };
    }

    private float CalculateComboMultiplier(int combo)
    {
        // OSU formula: 1 + (combo × 0.01), capped at 4x
        float multiplier = COMBO_MULTIPLIER_BASE + (combo * COMBO_MULTIPLIER_INCREMENT);
        return Mathf.Min(multiplier, MAX_COMBO_MULTIPLIER);
    }

    private float CalculateAccuracyBonus(JudgementType judgement)
    {
        // Bonus based on judgement quality
        return judgement switch
        {
            JudgementType.Perfect => 1.2f,  // +20% bonus
            JudgementType.Good => 1.0f,     // No bonus
            JudgementType.OK => 0.8f,       // -20% penalty
            _ => 0.5f                        // -50% penalty
        };
    }

    // ═══════════════════════════════════════════════════════════
    // DEBUG
    // ═══════════════════════════════════════════════════════════

    public void LogStats()
    {
        Debug.Log("═══════════════════════════════════════════════════════");
        Debug.Log($"[ScoreServiceOSU] STATISTICS");
        Debug.Log($"Total Score: {_totalScore}");
        Debug.Log($"Max Combo: {_maxCombo}");
        Debug.Log($"Accuracy: {GetAccuracy():F2}%");
        Debug.Log($"Perfect: {GetJudgementCount(JudgementType.Perfect)}");
        Debug.Log($"Good: {GetJudgementCount(JudgementType.Good)}");
        Debug.Log($"OK: {GetJudgementCount(JudgementType.OK)}");
        Debug.Log($"Miss: {GetJudgementCount(JudgementType.Miss)}");
        Debug.Log("═══════════════════════════════════════════════════════");
    }
}
