using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// SRP: Core Game Loop for OSU-style rhythm game
/// Responsibility: Orchestrate game flow using injected services
/// </summary>
public class GameLoopOSU : MonoBehaviour
{
    [Header("Service Provider")]
    [SerializeField] private GameInstallerOSU _installer;

    [Header("Configuration")]
    [SerializeField] private BeatConfig _defaultBeatConfig;

    [Header("Components")]
    [SerializeField] private BeatSpawner _beatSpawner;

    [Header("UI References")]
    [SerializeField] private JudgementDisplay _judgementDisplay;
    [SerializeField] private ComboDisplay _comboDisplay;

    // Injected services
    private GameTimeService _timeService;
    private IScoreService _scoreService;
    private JudgementEvaluator _evaluator;
    private PhaseController _phaseController;

    // Beat tracking
    private List<BeatCircle> _activeBeats = new List<BeatCircle>();

    public event Action<int> OnGameCompleted;

    private void Awake()
    {
        // Validate installer is assigned
        if (_installer == null)
        {
            Debug.LogError("[GameLoopOSU] GameInstallerOSU is not assigned in the Inspector!");
            enabled = false;
            return;
        }

        // Validate UI references
        if (_judgementDisplay == null)
        {
            Debug.LogWarning("[GameLoopOSU] JudgementDisplay is not assigned! UI feedback will not be displayed.");
        }

        if (_comboDisplay == null)
        {
            Debug.LogWarning("[GameLoopOSU] ComboDisplay is not assigned! Combo UI will not be displayed.");
        }

        // Explicitly ensure installer has initialized its services
        // This solves script execution order issues
        _installer.EnsureInitialized();

        // Now inject services
        EnsureServicesInitialized();

        Debug.Log("[GameLoopOSU] Services injected from GameInstallerOSU");
    }

    private void EnsureServicesInitialized()
    {
        // Inject services from installer
        _timeService = _installer.TimeService;
        _scoreService = _installer.ScoreService;
        _evaluator = _installer.Evaluator;
        _phaseController = _installer.PhaseController;

        // Validate all services are available
        if (_timeService == null || _scoreService == null || _evaluator == null || _phaseController == null)
        {
            Debug.LogError("[GameLoopOSU] One or more services from GameInstallerOSU are null! " +
                          "Make sure GameInstallerOSU.Awake() has run and initialized services.");
            enabled = false;
        }
    }

    private void Start()
    {
        // ✅ FIX: Warm services BEFORE game starts
        WarmupServices();
        StartGame().Forget();
    }

    private void WarmupServices()
    {
        Debug.Log("[GameLoopOSU] Warming up services...");

        // 1. Warm Evaluator
        Vector2 dummySize = Vector2.one * 100f;
        _evaluator.Evaluate(dummySize, dummySize);
        _evaluator.GetFeedback(JudgementType.Perfect);
        _evaluator.GetFeedback(JudgementType.Good);
        _evaluator.GetFeedback(JudgementType.OK);
        _evaluator.GetFeedback(JudgementType.Miss);

        // 2. Warm ScoreService
        _scoreService.RecordJudgement(JudgementType.Perfect);
        _scoreService.GetTotalScore();
        _scoreService.GetCurrentCombo();
        _scoreService.GetAccuracy();
        _scoreService.ResetPhaseScore(); // Reset về 0

        Debug.Log("[GameLoopOSU] Services warmup complete!");
    }

    private void Update()
    {
        if (_timeService == null || !_timeService.IsRunning) return;

        // Update time service
        _timeService.Update(Time.deltaTime);

        // Update spawner with current time
        _beatSpawner.UpdateSpawning(_timeService.CurrentTime);

        // ✅ UPDATE COMBO DISPLAY mỗi frame
        if (_comboDisplay != null)
        {
            _comboDisplay.UpdateCombo(_scoreService.GetCurrentCombo());
        }
    }

    private async UniTask StartGame()
    {
        // ✅ Wait 1 frame để warmup hoàn tất
        await UniTask.Yield();

        Debug.Log("[GameLoopOSU] Initializing...");

        // Update beat config
        _beatSpawner.UpdateBeatConfig(_defaultBeatConfig);

        // Setup events
        _beatSpawner.OnBeatSpawned += OnBeatSpawned;

        _phaseController.OnPhaseStarted += OnPhaseStarted;
        _phaseController.OnPhaseEnded += OnPhaseEnded;
        _phaseController.OnAllPhasesCompleted += OnAllPhasesCompleted;

        // ✅ Wait 1 more frame
        await UniTask.Yield();

        // Start time service
        _timeService.Start();

        // Start first phase
        _phaseController.StartFirstPhase();

        Debug.Log("[GameLoopOSU] Game Started");
    }

    private void OnPhaseStarted(PhaseData phase)
    {
        Debug.Log($"[GameLoopOSU] Phase Started: {phase.phaseName}");

        // Get BeatConfig for this phase
        BeatConfig beatConfigForPhase = phase.beatConfig != null
            ? phase.beatConfig
            : _defaultBeatConfig;

        // Update spawner with phase's BeatConfig
        _beatSpawner.UpdateBeatConfig(beatConfigForPhase);

        // Set generated beats to spawner
        _beatSpawner.SetBeats(_phaseController.CurrentPhaseBeats);

        // Reset phase score
        _scoreService.ResetPhaseScore();
    }

    private async void OnPhaseEnded(PhaseData phase, int phaseIndex)
    {
        int phaseScore = _scoreService.GetTotalScore();
        _scoreService.RecordPhaseScore(phaseIndex, phaseScore);

        Debug.Log($"[GameLoopOSU] Phase {phaseIndex + 1} Ended: {phase.phaseName} | Score: {phaseScore}");

        // ✅ CHECK IF MORE PHASES EXIST
        if (_phaseController.CurrentPhaseIndex < _phaseController.TotalPhases - 1)
        {
            // More phases available - pause and start next
            if (phase.pauseDuration > 0f)
            {
                Debug.Log($"[GameLoopOSU] Pausing for {phase.pauseDuration}s before next phase...");
                await UniTask.Delay((int)(phase.pauseDuration * 1000));
            }

            // Start next phase
            _phaseController.StartNextPhase();
        }
        else
        {
            // No more phases - game will end
            Debug.Log($"[GameLoopOSU] No more phases. Waiting for completion event...");
        }
    }

    private void OnAllPhasesCompleted()
    {
        // Stop time
        _timeService.Stop();

        // Get total score
        int totalScore = _scoreService.GetTotalScore();
        List<int> phaseScores = _scoreService.GetAllPhaseScores();

        // Log final results
        Debug.Log("═══════════════════════════════════════════════════════");
        Debug.Log($"[GameLoopOSU] GAME COMPLETED!");
        Debug.Log($"[GameLoopOSU] Phase Scores: {string.Join(", ", phaseScores)}");
        Debug.Log($"[GameLoopOSU] TOTAL SCORE: {totalScore}");
        Debug.Log($"[GameLoopOSU] Accuracy: {_scoreService.GetAccuracy():F2}%");
        Debug.Log($"[GameLoopOSU] Max Combo: {_scoreService.GetMaxCombo()}");
        Debug.Log("═══════════════════════════════════════════════════════");

        // Log statistics (if ScoreServiceOSU)
        if (_scoreService is ScoreServiceOSU osuService)
        {
            osuService.LogStats();
        }

        // OUTPUT SCORE
        OnGameCompleted?.Invoke(totalScore);

        ShowEndScreen(totalScore).Forget();
    }

    private async UniTask ShowEndScreen(int finalScore)
    {
        await UniTask.Delay(1000);
    }

    private void OnBeatSpawned(BeatCircle beat)
    {
        _activeBeats.Add(beat);
        beat.OnTapped += OnBeatTapped;
        beat.OnMissed += OnBeatMissed;
    }

    // ═══════════════════════════════════════════════════════════
    // ✅ BEAT EVENT HANDLERS - FIXED DUPLICATE DISPLAY
    // ═══════════════════════════════════════════════════════════

    private void OnBeatTapped(BeatCircle beat)
    {
        _activeBeats.Remove(beat);

        // ✅ Evaluate judgement (Perfect/Good/OK/Miss based on timing)
        JudgementType judgement = _evaluator.Evaluate(beat.CurrentSize, beat.TargetSize);

        // ✅ Record to score service (updates combo + score)
        _scoreService.RecordJudgement(judgement);

        // ✅ Get feedback data
        FeedbackData feedback = _evaluator.GetFeedback(judgement);

        // ✅ Show judgement UI (ALWAYS show for tap, even if Miss)
        if (_judgementDisplay != null)
        {
            _judgementDisplay.Show(feedback);
        }

        // ✅ Play visual feedback on beat
        beat.PlayHitFeedback();

        // ✅ Notify phase controller
        _phaseController.OnBeatCompleted();
    }

    private void OnBeatMissed(BeatCircle beat)
    {
        _activeBeats.Remove(beat);

        // ✅ Record miss to score service (breaks combo)
        _scoreService.RecordJudgement(JudgementType.Miss);

        // ✅ Get miss feedback
        FeedbackData feedback = _evaluator.GetFeedback(JudgementType.Miss);

        // ✅ Show MISS UI (user didn't tap at all)
        if (_judgementDisplay != null)
        {
            _judgementDisplay.Show(feedback);
        }

        // ✅ Play miss feedback on beat
        beat.PlayMissFeedback();

        // ✅ Notify phase controller
        _phaseController.OnBeatCompleted();
    }

    // ═══════════════════════════════════════════════════════════
    // PUBLIC GETTERS
    // ═══════════════════════════════════════════════════════════

    public int GetCurrentTotalScore()
    {
        return _scoreService?.GetTotalScore() ?? 0;
    }

    public List<int> GetPhaseScores()
    {
        return _scoreService?.GetAllPhaseScores() ?? new List<int>();
    }

    public int GetCurrentCombo()
    {
        return _scoreService?.GetCurrentCombo() ?? 0;
    }

    public float GetAccuracy()
    {
        return _scoreService?.GetAccuracy() ?? 0f;
    }
}