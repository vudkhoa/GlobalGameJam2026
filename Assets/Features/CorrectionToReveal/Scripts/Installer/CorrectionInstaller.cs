using UnityEngine;

/// <summary>
/// Main installer for Correction mini-game
/// Dependency Injection container
/// Follows Single Responsibility Principle
/// </summary>
public class CorrectionInstaller : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private CorrectionSettings _settings;

    [Header("UI")]
    [SerializeField] private CorrectionUIController _uiController;

    [Header("Rendering")]
    [SerializeField] private Renderer _targetRenderer;

    // Dependencies
    private CorrectionData _data;
    private CorrectionValidator _validator;
    private ShaderParameterApplier _shaderApplier;
    private CorrectionLogicHandler _logicHandler;
    private RulerSpawner _rulerSpawner;

    private void Start()
    {
        InstallDependencies();
        SetupGame();
    }

    /// <summary>
    /// Install all dependencies (Dependency Injection)
    /// </summary>
    private void InstallDependencies()
    {
        // Validate settings
        if (_settings == null)
        {
            Debug.LogError("CorrectionSettings is not assigned!");
            return;
        }

        // Create data
        _data = new CorrectionData
        {
            BlurAmount = _settings.InitialBlurAmount,
            HorizontalScale = _settings.InitialHorizontalScale,
            TargetBlurAmount = _settings.TargetBlurAmount,
            TargetHorizontalScale = _settings.TargetHorizontalScale,
            Tolerance = _settings.Tolerance
        };

        // Create validator
        _validator = new CorrectionValidator(_data);

        // Get material from renderer or settings
        Material material = GetMaterial();
        if (material == null)
        {
            Debug.LogError("Material not found!");
            return;
        }

        // Apply texture to material if provided
        if (_settings.TargetTexture != null)
        {
            material.mainTexture = _settings.TargetTexture;
        }

        // Create shader applier
        _shaderApplier = new ShaderParameterApplier(
            material,
            _settings.BlurPropertyName,
            _settings.ScalePropertyName
        );

        // Create logic handler
        _logicHandler = new CorrectionLogicHandler(_data, _validator, _shaderApplier);

        // Subscribe to events
        _logicHandler.OnCorrectionComplete += OnCorrectionComplete;
        _logicHandler.OnProgressChanged += OnProgressChanged;

        // Create ruler spawner
        _rulerSpawner = new RulerSpawner(
            _settings.RulerPrefab,
            _uiController.RulerContainer,
            _settings.RulerSpacing
        );

        // Initialize UI
        _uiController.Initialize(_rulerSpawner);
    }

    /// <summary>
    /// Get material from renderer or settings
    /// </summary>
    private Material GetMaterial()
    {
        if (_targetRenderer != null)
        {
            // Create instance to avoid modifying shared material
            _targetRenderer.material = new Material(_settings.CorrectionMaterial);
            return _targetRenderer.material;
        }

        return _settings.CorrectionMaterial;
    }

    /// <summary>
    /// Setup game - spawn rulers and initialize
    /// </summary>
    private void SetupGame()
    {
        if (_logicHandler == null || _rulerSpawner == null)
        {
            Debug.LogError("Dependencies not installed!");
            return;
        }

        // Initialize logic
        _logicHandler.Initialize();

        // Spawn rulers for each parameter
        SpawnRulers();
    }

    /// <summary>
    /// Spawn ruler UI elements based on shader parameters
    /// </summary>
    private void SpawnRulers()
    {
        // Ruler 1: Blur Amount
        _rulerSpawner.SpawnRuler(
            "Blur Amount",
            0f,
            10f,
            _data.BlurAmount,
            (value) => _logicHandler.UpdateBlurAmount(value)
        );

        // Ruler 2: Horizontal Scale
        _rulerSpawner.SpawnRuler(
            "Horizontal Scale",
            0.1f,
            3f,
            _data.HorizontalScale,
            (value) => _logicHandler.UpdateHorizontalScale(value)
        );
    }

    /// <summary>
    /// Called when correction is complete
    /// </summary>
    private void OnCorrectionComplete()
    {
        Debug.Log("Correction Complete!");
        _uiController.ShowCompletion();
    }

    /// <summary>
    /// Called when progress changes
    /// </summary>
    private void OnProgressChanged(float progress)
    {
        _uiController.UpdateProgress(progress);
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (_logicHandler != null)
        {
            _logicHandler.OnCorrectionComplete -= OnCorrectionComplete;
            _logicHandler.OnProgressChanged -= OnProgressChanged;
        }

        // Clear UI
        _uiController?.Clear();
    }
}
