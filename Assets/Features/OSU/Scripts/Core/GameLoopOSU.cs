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
    [SerializeField] private BeatConnector _connectorManager; 

    [Header("Input Configuration")]
    [SerializeField] private Camera _gameCamera;
    [SerializeField] private LayerMask _beatLayerMask = -1;
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

    // ✅ Connector tracking
    private Queue<PendingConnector> _pendingConnectors = new Queue<PendingConnector>();

    public event Action<int> OnGameCompleted;

    private void Awake()
    {
        if (_installer == null)
        {
            enabled = false;
            return;
        }

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

        _inputHandler = new BeatInputHandler(_gameCamera, _beatLayerMask, _debugInput);
        _installer.EnsureInitialized();
        EnsureServicesInitialized();
    }

    private void EnsureServicesInitialized()
    {
        _timeService = _installer.TimeService;
        _scoreService = _installer.ScoreService;
        _evaluator = _installer.Evaluator;
        _phaseController = _installer.PhaseController;

        if (_timeService == null || _scoreService == null || _evaluator == null || _phaseController == null)
        {
            enabled = false;
        }
    }

    private void Start()
    {
        WarmupServices();
        StartGame().Forget();
    }

    private void WarmupServices()
    {
        Vector2 dummySize = Vector2.one * 100f;
        _evaluator.Evaluate(dummySize, dummySize);
        _evaluator.GetFeedback(JudgementType.Perfect);
        _evaluator.GetFeedback(JudgementType.Good);
        _evaluator.GetFeedback(JudgementType.OK);
        _evaluator.GetFeedback(JudgementType.Miss);

        _scoreService.RecordJudgement(JudgementType.Perfect);
        _scoreService.GetTotalScore();
        _scoreService.GetCurrentCombo();
        _scoreService.GetAccuracy();
        _scoreService.ResetPhaseScore();
    }

    private void Update()
    {
        if (_timeService == null || !_timeService.IsRunning) return;

        HandleInput();
        _timeService.Update(Time.deltaTime);
        _beatSpawner.UpdateSpawning(_timeService.CurrentTime);

        // ✅ Update connector spawning
        UpdateConnectorSpawning(_timeService.CurrentTime);

        if (_comboDisplay != null)
        {
            _comboDisplay.UpdateCombo(_scoreService.GetCurrentCombo());
        }
    }

    // ═══════════════════════════════════════════════════════════
    // ✅ CONNECTOR SPAWNING LOGIC
    // ═══════════════════════════════════════════════════════════

    private void UpdateConnectorSpawning(float currentTime)
    {
        while (_pendingConnectors.Count > 0)
        {
            PendingConnector pending = _pendingConnectors.Peek();

            if (currentTime >= pending.spawnTime)
            {
                _pendingConnectors.Dequeue();
                SpawnConnector(pending.data);
            }
            else
            {
                break;
            }
        }
    }

    private void SpawnConnector(TrajectoryTransitionData data)
    {
        if (_connectorManager == null)
        {
            Debug.LogWarning("[GameLoopOSU] BeatConnectorManager not assigned!");
            return;
        }

        _connectorManager.SpawnConnector(
            data.currentTrajectoryEndPos,
            data.nextTrajectoryStartPos,
            data.connectorConfig
        );
    }

    // ═══════════════════════════════════════════════════════════
    // INPUT HANDLING
    // ═══════════════════════════════════════════════════════════

    private void HandleInput()
    {
        List<BeatCircle> hitBeats = _inputHandler.ProcessInput();

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
        _beatSpawner.UpdateBeatConfig(_defaultBeatConfig);

        // ✅ Subscribe to events
        _beatSpawner.OnBeatSpawned += OnBeatSpawned;
        _phaseController.OnPhaseStarted += OnPhaseStarted;
        _phaseController.OnPhaseEnded += OnPhaseEnded;
        _phaseController.OnAllPhasesCompleted += OnAllPhasesCompleted;
        _phaseController.OnTrajectoryTransition += OnTrajectoryTransition; // ✅ NEW

        await UniTask.Yield();

        _phaseController.StartFirstPhase();

        await UniTask.Yield();

        _timeService.Start();
    }

    // ═══════════════════════════════════════════════════════════
    // ✅ EVENT HANDLERS
    // ═══════════════════════════════════════════════════════════

    private void OnTrajectoryTransition(TrajectoryTransitionData transitionData)
    {
        // Queue connector để spawn đúng timing
        _pendingConnectors.Enqueue(new PendingConnector
        {
            spawnTime = transitionData.transitionTime,
            data = transitionData
        });
    }

    private void OnPhaseStarted(PhaseData phase)
    {
        if (_blurEffect != null)
        {
            _blurEffect.ShowPhaseBackground(_phaseController.CurrentPhaseIndex);
        }

        BeatConfig beatConfigForPhase = phase.beatConfig != null
            ? phase.beatConfig
            : _defaultBeatConfig;

        _beatSpawner.UpdateBeatConfig(beatConfigForPhase);
        _beatSpawner.SetBeats(_phaseController.CurrentPhaseBeats);

        if (_connectorManager != null)
        {
            _connectorManager.SetBeatConfig(beatConfigForPhase);
        }

        _scoreService.ResetPhaseScore();
    }

    private async void OnPhaseEnded(PhaseData phase, int phaseIndex)
    {
        int phaseScore = _scoreService.GetTotalScore();
        _scoreService.RecordPhaseScore(phaseIndex, phaseScore);

        // ✅ Clear pending connectors khi kết thúc phase
        _pendingConnectors.Clear();

        if (_phaseController.CurrentPhaseIndex < _phaseController.TotalPhases - 1)
        {
            if (_blurEffect != null)
            {
                _blurEffect.UnBlurBg();
            }

            await PlayCutsceneForPhase(phaseIndex);

            if (phase.pauseDuration > 0f)
            {
                await UniTask.Delay((int)(phase.pauseDuration * 1000));
            }

            _phaseController.StartNextPhase();
        }
    }

    private async UniTask PlayCutsceneForPhase(int completedPhaseIndex)
    {
        switch (completedPhaseIndex)
        {
            case 0:
                await cutscene_1.PlayAsync();
                break;
            case 1:
                await cutscene_2.PlayAsync();
                break;
            case 2:
                break;
        }
    }

    private void OnAllPhasesCompleted()
    {
        _timeService.Stop();

        int totalScore = _scoreService.GetTotalScore();
        List<int> phaseScores = _scoreService.GetAllPhaseScores();

        if (_scoreService is ScoreServiceOSU osuService)
        {
            osuService.LogStats();
        }

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

    private void OnBeatTapped(BeatCircle beat)
    {
        _activeBeats.Remove(beat);

        JudgementType judgement = _evaluator.Evaluate(beat.CurrentSize, beat.TargetSize);
        _scoreService.RecordJudgement(judgement);
        FeedbackData feedback = _evaluator.GetFeedback(judgement);

        if (_judgementDisplay != null)
        {
            _judgementDisplay.Show(feedback, beat.transform.position);
        }

        beat.PlayHitFeedback();
        _phaseController.OnBeatCompleted();
    }

    private void OnBeatMissed(BeatCircle beat)
    {
        _activeBeats.Remove(beat);

        _scoreService.RecordJudgement(JudgementType.Miss);
        FeedbackData feedback = _evaluator.GetFeedback(JudgementType.Miss);

        if (_judgementDisplay != null)
        {
            _judgementDisplay.Show(feedback, beat.transform.position);
        }

        beat.PlayMissFeedback();
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

/// <summary>
/// Helper struct để queue connectors
/// </summary>
[System.Serializable]
public struct PendingConnector
{
    public float spawnTime;
    public TrajectoryTransitionData data;
}