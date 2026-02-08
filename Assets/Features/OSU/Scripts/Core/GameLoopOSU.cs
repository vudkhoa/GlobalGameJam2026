using System;
using System.Collections.Generic;
using BrunoMikoski.AnimationSequencer;
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

    [Header("Input Configuration")]
    [SerializeField] private Camera _gameCamera;
    [SerializeField] private LayerMask _beatLayerMask = -1; // Default: everything
    [SerializeField] private bool _debugInput = false;

    [Header("Animation Sequences")]
    [SerializeField] private AnimationSequencerController intro;
    [SerializeField] private AnimationSequencerController intro2;
    [SerializeField] private AnimationSequencerController intro3;
    [SerializeField] private AnimationSequencerController cutscene_1;
    [SerializeField] private AnimationSequencerController cutscene_2;
    [SerializeField] private AnimationSequencerController cutscene_3;

    [Header("UI References")]
    [SerializeField] private JudgementDisplay _judgementDisplay;
    [SerializeField] private ComboDisplay _comboDisplay;
    [SerializeField] private BlurEffect _blurEffect;

    // Injected services
    private GameTimeService _timeService;
    private IScoreService _scoreService;
    private JudgementEvaluator _evaluator;
    private PhaseController _phaseController;
    private BeatInputHandler _inputHandler;

    // Beat tracking
    private List<BeatCircle> _activeBeats = new List<BeatCircle>();

    public event Action<int> OnGameCompleted;

    private void Awake()
    {
        // Validate installer is assigned
        if (_installer == null)
        {
            enabled = false;
            return;
        }

        // Setup camera
        if (_gameCamera == null)
        {
            _gameCamera = Camera.main;
        }

        if (_gameCamera == null)
        {
            Debug.LogError("[GameLoopOSU] No camera found! Input will not work.");
            enabled = false;
            return;
        }

        // Initialize input handler
        _inputHandler = new BeatInputHandler(_gameCamera, _beatLayerMask, _debugInput);

        // Explicitly ensure installer has initialized its services
        // This solves script execution order issues
        _installer.EnsureInitialized();

        // Now inject services
        EnsureServicesInitialized();
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

    }

    private void Update()
    {
        if (_timeService == null || !_timeService.IsRunning) return;

        //  PROCESS INPUT FIRST (highest priority to avoid missing input)
        HandleInput();

        // Update time service
        _timeService.Update(Time.deltaTime);

        // Update spawner with current time
        _beatSpawner.UpdateSpawning(_timeService.CurrentTime);

        //  UPDATE COMBO DISPLAY mỗi frame
        if (_comboDisplay != null)
        {
            _comboDisplay.UpdateCombo(_scoreService.GetCurrentCombo());
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  INPUT HANDLING - Processed every frame in Update()
    // ═══════════════════════════════════════════════════════════

    private void HandleInput()
    {
        // Lấy danh sách các nốt bị bấm trúng (hỗ trợ đa điểm)
        List<BeatCircle> hitBeats = _inputHandler.ProcessInput();

        // Duyệt qua từng nốt và kích hoạt
        foreach (var beat in hitBeats)
        {
            if (beat != null)
            {
                beat.OnTap();
            }
        }
    }

    private async UniTask StartGame()
    {
        // Update beat config
        _beatSpawner.UpdateBeatConfig(_defaultBeatConfig);

        // Setup events
        _beatSpawner.OnBeatSpawned += OnBeatSpawned;

        _phaseController.OnPhaseStarted += OnPhaseStarted;
        _phaseController.OnPhaseEnded += OnPhaseEnded;
        _phaseController.OnAllPhasesCompleted += OnAllPhasesCompleted;

        await UniTask.Yield();

        // Đảm bảo beats được setup TRƯỚC KHI time bắt đầu chạy
        _phaseController.StartFirstPhase();

        await UniTask.Yield();

        // Start time service (SAU KHI phase đã ready)
        _timeService.Start();
    }

    private void OnPhaseStarted(PhaseData phase)
    {

        // ✅ Hiển thị phase background + blur khi bắt đầu phase
        if (_blurEffect != null)
        {
            _blurEffect.ShowPhaseBackground(_phaseController.CurrentPhaseIndex);
        }

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

        // ✅ CHECK IF MORE PHASES EXIST
        if (_phaseController.CurrentPhaseIndex < _phaseController.TotalPhases - 1)
        {
            // ✅ UnBlur và ẩn phase background khi kết thúc phase
            if (_blurEffect != null)
            {
                _blurEffect.UnBlurBg();
            }

            // Play cutscene tương ứng với phase vừa kết thúc
            await PlayCutsceneForPhase(phaseIndex);

            // Pause if needed
            if (phase.pauseDuration > 0f)
            {
                await UniTask.Delay((int)(phase.pauseDuration * 1000));
            }

            // Start next phase (sẽ hiện phase background mới trong OnPhaseStarted)
            _phaseController.StartNextPhase();
        }
        else
        {
            // No more phases - game will end
        }
    }

    /// <summary>
    /// Play cutscene dựa trên phase index vừa hoàn thành
    /// Phase 0 (Phase 1) → cutscene_1
    /// Phase 1 (Phase 2) → cutscene_2
    /// Phase 2 (Phase 3) → cutscene_3
    /// </summary>
    private async UniTask PlayCutsceneForPhase(int completedPhaseIndex)
    {
        AnimationSequencerController cutscene = null;

        switch (completedPhaseIndex)
        {
            case 0: // Sau phase 1
                cutscene = cutscene_1;
                await cutscene_1.PlayAsync();
                break;
            case 1: // Sau phase 2
                cutscene = cutscene_2;
                await cutscene_2.PlayAsync();
                break;
            case 2: // Sau phase 3
                cutscene = cutscene_3;
                break;
        }


    }

    private void OnAllPhasesCompleted()
    {
        // Stop time
        _timeService.Stop();

        // Get total score
        int totalScore = _scoreService.GetTotalScore();
        List<int> phaseScores = _scoreService.GetAllPhaseScores();

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
        await cutscene_3.PlayAsync();
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

        //  Evaluate judgement (Perfect/Good/OK/Miss based on timing)
        JudgementType judgement = _evaluator.Evaluate(beat.CurrentSize, beat.TargetSize);

        //  Record to score service (updates combo + score)
        _scoreService.RecordJudgement(judgement);

        //  Get feedback data
        FeedbackData feedback = _evaluator.GetFeedback(judgement);

        //  Show judgement UI (ALWAYS show for tap, even if Miss)
        if (_judgementDisplay != null)
        {
            _judgementDisplay.Show(feedback);
        }

        //  Play visual feedback on beat
        beat.PlayHitFeedback();

        //  Notify phase controller
        _phaseController.OnBeatCompleted();
    }

    private void OnBeatMissed(BeatCircle beat)
    {
        _activeBeats.Remove(beat);

        //  Record miss to score service (breaks combo)
        _scoreService.RecordJudgement(JudgementType.Miss);

        //  Get miss feedback
        FeedbackData feedback = _evaluator.GetFeedback(JudgementType.Miss);

        //  Show MISS UI (user didn't tap at all)
        if (_judgementDisplay != null)
        {
            _judgementDisplay.Show(feedback);
        }

        //  Play miss feedback on beat
        beat.PlayMissFeedback();

        //  Notify phase controller
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