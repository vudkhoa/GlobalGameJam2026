using UnityEngine;

/// <summary>
/// SRP: Service Provider (DI Container)
/// Responsibility: Create and provide services for game loop
/// </summary>
public class GameInstallerOSU : MonoBehaviour
{
    [Header("Configurations")]
    [SerializeField] private JudgementConfig _judgementConfig;
    [SerializeField] private BeatConfig _defaultBeatConfig; // ✅ NEW: Default beat config
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
        // Initialize services as early as possible (OnEnable runs before Awake)
        EnsureInitialized();
    }

    private void Awake()
    {
        // Ensure initialization (in case OnEnable didn't run)
        EnsureInitialized();
    }

    /// <summary>
    /// Public method to ensure services are initialized
    /// Can be called by dependent scripts to guarantee initialization
    /// </summary>
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
        // Create services
        _timeService = new GameTimeService();
        _scoreService = new ScoreServiceOSU(_judgementConfig);
        _evaluator = new JudgementEvaluator(_judgementConfig);
        _phaseController = new PhaseController();

        // ✅ Initialize phase controller with default beat config
        _phaseController.Initialize(_sessionData.phases, _defaultBeatConfig);
    }

    private void OnDestroy()
    {
    }
}