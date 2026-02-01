using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Interface: Score calculation service
/// Different implementations can have different scoring algorithms
/// </summary>
public interface IScoreService
{
    /// <summary>
    /// Record a judgement and calculate score
    /// </summary>
    int RecordJudgement(JudgementType judgement);

    /// <summary>
    /// Get current total score
    /// </summary>
    int GetTotalScore();

    /// <summary>
    /// Get current combo
    /// </summary>
    int GetCurrentCombo();

    /// <summary>
    /// Get max combo achieved
    /// </summary>
    int GetMaxCombo();

    /// <summary>
    /// Record phase score
    /// </summary>
    void RecordPhaseScore(int phaseIndex, int score);

    /// <summary>
    /// Get phase score
    /// </summary>
    int GetPhaseScore(int phaseIndex);

    /// <summary>
    /// Get all phase scores
    /// </summary>
    List<int> GetAllPhaseScores();

    /// <summary>
    /// Reset current phase score
    /// </summary>
    void ResetPhaseScore();

    /// <summary>
    /// Reset all scores
    /// </summary>
    void ResetAll();

    /// <summary>
    /// Get judgement count
    /// </summary>
    int GetJudgementCount(JudgementType type);

    /// <summary>
    /// Get accuracy percentage
    /// </summary>
    float GetAccuracy();
}
