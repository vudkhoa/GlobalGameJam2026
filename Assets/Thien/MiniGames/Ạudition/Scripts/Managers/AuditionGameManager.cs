using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// SRP: Orchestrate audition game flow
/// Responsibility: Coordinate all managers, output final score
/// </summary>
public class AuditionGameManager : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private BeatConfig _beatConfig;
    [SerializeField] private JudgementConfig _judgementConfig;
    [SerializeField] private AuditionSessionData _sessionData;

    [Header("Components")]
    [SerializeField] private BeatSpawner _beatSpawner;
    [SerializeField] private InputHandler _inputHandler;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private Animator _animator;

    [Header("UI")]
    [SerializeField] private ComboDisplay _comboDisplay;
    [SerializeField] private JudgementDisplay _judgementDisplay;
    [SerializeField] private BlurEffect _blurEffect;

    private ScoreManager _scoreManager;
    private JudgementEvaluator _evaluator;
    private PhaseController _phaseController;
    private BeatCirclePool _beatPool;
    private List<BeatCircle> _activeBeats = new List<BeatCircle>();

    private float _gameTime = 0f;
    private bool _isPlaying = false;

    private List<int> _phaseScores = new List<int>();

    // ✅ Output score event
    public event Action<int> OnGameCompleted;

    // ═══════════════════════════════════════════════════════════
    // LIFECYCLE
    // ═══════════════════════════════════════════════════════════

    private void Start()
    {
        StartGame().Forget();
    }

    private void Update()
    {
        if (!_isPlaying) return;

        _gameTime += Time.deltaTime;

        _beatSpawner.UpdateSpawning(_gameTime);
    }

    // ═══════════════════════════════════════════════════════════
    // GAME FLOW
    // ═══════════════════════════════════════════════════════════

    private async UniTask StartGame()
    {
        // Initialize managers
        _scoreManager = new ScoreManager(_judgementConfig);
        _evaluator = new JudgementEvaluator(_judgementConfig);
        _phaseController = new PhaseController();

        // Get pool reference
        _beatPool = FindObjectOfType<BeatCirclePool>();
        if (_beatPool == null)
        {
            Debug.LogError("[AuditionGameManager] BeatCirclePool not found in scene!");
            return;
        }

        // Initialize components
        _beatSpawner.Initialize(_beatConfig);

        // Initialize phase controller
        _phaseController.Initialize(_sessionData.phases);

        // Setup events
        _beatSpawner.OnBeatSpawned += OnBeatSpawned;
        _inputHandler.OnTapInput += OnPlayerTap;
        _scoreManager.OnComboChanged += _comboDisplay.UpdateCombo;

        _phaseController.OnPhaseStarted += OnPhaseStarted;
        _phaseController.OnPhaseEnded += OnPhaseEnded;
        _phaseController.OnAllPhasesCompleted += OnAllPhasesCompleted;

        // Visual setup
        await _blurEffect.FadeIn(GameConstants.BACKGROUND_BLUR_AMOUNT, GameConstants.BLUR_TRANSITION_DURATION);

        // Play audio
        _audioSource.clip = _sessionData.defaultAudioClip;
        _audioSource.volume = GameConstants.MUSIC_VOLUME;
        _audioSource.Play();

        _isPlaying = true;
        _gameTime = 0f;

        // Start first phase
        _phaseController.StartFirstPhase();

        Debug.Log("[AuditionGameManager] Game Started");
    }

    // ═══════════════════════════════════════════════════════════
    // PHASE CALLBACKS
    // ═══════════════════════════════════════════════════════════

    private void OnPhaseStarted(PhaseData phase)
    {
        Debug.Log($"[AuditionGameManager] Phase Started: {phase.phaseName}");

        // Set generated beats to spawner
        _beatSpawner.SetBeats(_phaseController.CurrentPhaseBeats);

        // Reset phase score
        _scoreManager.ResetPhaseScore();
    }

    private async void OnPhaseEnded(PhaseData phase, int phaseIndex)
    {
        int phaseScore = _scoreManager.GetCurrentScore();
        _phaseScores.Add(phaseScore);

        Debug.Log($"[AuditionGameManager] Phase {phaseIndex + 1} Ended: {phase.phaseName} | Score: {phaseScore}");

        // Trigger animation
        if (!string.IsNullOrEmpty(phase.animationTrigger) && _animator != null)
        {
            Debug.Log($"[AuditionGameManager] Triggering animation: {phase.animationTrigger}");
            _animator.SetTrigger(phase.animationTrigger);
        }

        // Pause
        if (phase.pauseDuration > 0f)
        {
            Debug.Log($"[AuditionGameManager] Pausing for {phase.pauseDuration}s...");
            await UniTask.Delay((int)(phase.pauseDuration * 1000));
        }

        // Start next phase
        _phaseController.StartNextPhase();
    }

    private void OnAllPhasesCompleted()
    {
        _isPlaying = false;

        // Calculate total score
        int totalScore = 0;
        foreach (int score in _phaseScores)
        {
            totalScore += score;
        }

        // Log final results
        Debug.Log("═══════════════════════════════════════════════════════");
        Debug.Log($"[AuditionGameManager] GAME COMPLETED!");
        Debug.Log($"[AuditionGameManager] Phase Scores: {string.Join(", ", _phaseScores)}");
        Debug.Log($"[AuditionGameManager] TOTAL SCORE: {totalScore}");
        Debug.Log("═══════════════════════════════════════════════════════");

        // Log statistics
        _scoreManager.LogStats();

        // ✅ OUTPUT SCORE
        OnGameCompleted?.Invoke(totalScore);

        ShowEndScreen(totalScore).Forget();
    }

    private async UniTask ShowEndScreen(int finalScore)
    {
        await UniTask.Delay(1000);
        await _blurEffect.FadeOut(GameConstants.BLUR_TRANSITION_DURATION);
    }

    // ═══════════════════════════════════════════════════════════
    // EVENT HANDLERS
    // ═══════════════════════════════════════════════════════════

    private void OnBeatSpawned(BeatCircle beat)
    {
        _activeBeats.Add(beat);
        beat.OnTapped += OnBeatTapped;
        beat.OnMissed += OnBeatMissed;
    }

    private void OnPlayerTap()
    {
        BeatCircle closestBeat = GetClosestBeat();
        if (closestBeat != null)
        {
            closestBeat.OnTap();
        }
    }

    private void OnBeatTapped(BeatCircle beat)
    {
        _activeBeats.Remove(beat);

        JudgementType judgement = _evaluator.Evaluate(beat.CurrentSize, beat.TargetSize);
        int score = _evaluator.GetScore(judgement);

        _scoreManager.RecordJudgement(judgement, score);

        FeedbackData feedback = _evaluator.GetFeedback(judgement);
        _judgementDisplay.Show(feedback);

        beat.PlayHitFeedback();

        if (feedback.soundEffect != null)
        {
            _audioSource.PlayOneShot(feedback.soundEffect, GameConstants.SFX_VOLUME);
        }

        _phaseController.OnBeatCompleted();
    }

    private void OnBeatMissed(BeatCircle beat)
    {
        _activeBeats.Remove(beat);

        _scoreManager.RecordJudgement(JudgementType.Miss, 0);

        FeedbackData feedback = _evaluator.GetFeedback(JudgementType.Miss);
        _judgementDisplay.Show(feedback);

        beat.PlayMissFeedback();

        _phaseController.OnBeatCompleted();
    }

    private BeatCircle GetClosestBeat()
    {
        BeatCircle closest = null;
        float minDifference = float.MaxValue;

        foreach (var beat in _activeBeats)
        {
            if (beat == null || !beat.IsActive) continue;

            float difference = Mathf.Abs(beat.CurrentSize - beat.TargetSize);

            if (difference < minDifference)
            {
                minDifference = difference;
                closest = beat;
            }
        }

        return closest;
    }

    // ═══════════════════════════════════════════════════════════
    // PUBLIC API
    // ═══════════════════════════════════════════════════════════

    public int GetCurrentTotalScore()
    {
        return _scoreManager?.TotalScore ?? 0;
    }

    public List<int> GetPhaseScores()
    {
        return new List<int>(_phaseScores);
    }
}