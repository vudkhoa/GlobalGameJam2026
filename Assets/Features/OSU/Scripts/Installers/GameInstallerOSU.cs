using UnityEngine;

/// <summary>
/// SRP: Service Provider (DI Container)
/// Responsibility: Create and provide services for game loop
/// </summary>
public class GameInstallerOSU : MonoBehaviour
{
    [Header("Configurations")]
    [SerializeField] private JudgementConfig _judgementConfig;
    [SerializeField] private BeatConfig _defaultBeatConfig;
    [SerializeField] private AuditionSessionData _sessionData;

    // Services (created by installer)
    private GameTimeService _timeService;
    private IScoreService _scoreService;
    private JudgementEvaluator _evaluator;
    private PhaseController _phaseController;

    private bool _isInitialized = false;

    // ═══════════════════════════════════════════════════════════
    // PUBLIC PROPERTIES (Dependency Injection)
    // ═══════════════════════════════════════════════════════════

    public GameTimeService TimeService => _timeService;
    public IScoreService ScoreService => _scoreService;
    public JudgementEvaluator Evaluator => _evaluator;
    public PhaseController PhaseController => _phaseController;
    public AuditionSessionData SessionData => _sessionData;

    // ═══════════════════════════════════════════════════════════
    // LIFECYCLE
    // ═══════════════════════════════════════════════════════════

    private void OnEnable()
    {
        EnsureInitialized();
    }

    private void Awake()
    {
        EnsureInitialized();
    }

    public void EnsureInitialized()
    {
        if (_isInitialized) return;

        InstallServices();
        _isInitialized = true;
    }

    // ═══════════════════════════════════════════════════════════
    // INSTALLATION
    // ═══════════════════════════════════════════════════════════

    private void InstallServices()
    {
        _timeService = new GameTimeService();
        _scoreService = new ScoreServiceOSU(_judgementConfig);
        _evaluator = new JudgementEvaluator(_judgementConfig);
        _phaseController = new PhaseController();

        // ✅ FIX: Convert List<PhaseData> → PhaseData[]
        _phaseController.Initialize(_sessionData.phases.ToArray());
    }

    private void OnDestroy()
    {
    }
}