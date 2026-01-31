using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Main installer for Correction mini-game
/// Dependency Injection container
/// Automatically spawns rulers based on shader parameters
/// </summary>
public class CorrectionInstaller : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private CorrectionSettings _settings;

    [Header("UI")]
    [SerializeField] private Transform _rulerContainer;

    // Dependencies
    private CorrectionData _data;
    private CorrectionValidator _validator;
    private ShaderParameterApplier _shaderApplier;
    private CorrectionLogicHandler _logicHandler;
    private RulerSpawner _rulerSpawner;

    public CorrectionLogicHandler LogicHandler => _logicHandler;

    // Shader parameters (dynamically created)
    private List<ShaderParameter> _shaderParameters;

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

        // Create shader parameters from factory (SINGLE SOURCE OF TRUTH)
        _shaderParameters = ShaderParameterFactory.CreateAllParameters(_settings);

        Debug.Log($"[CorrectionInstaller] Created {_shaderParameters.Count} shader parameters");

        // Create data and initialize with parameters
        _data = new CorrectionData();
        _data.Initialize(_shaderParameters, _settings.Tolerance);

        // Create validator
        _validator = new CorrectionValidator(_data);

        // Create shader applier (simplified constructor)
        _shaderApplier = new ShaderParameterApplier(_settings.CorrectionMaterial);

        // Create logic handler
        _logicHandler = new CorrectionLogicHandler(_data, _validator, _shaderApplier);

        // Subscribe to events
        _logicHandler.OnCorrectionComplete += OnCorrectionComplete;

        // Create ruler spawner
        _rulerSpawner = new RulerSpawner(
            _settings.RulerPrefab,
            _rulerContainer,
            _settings.RulerSpacing
        );
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

        // Spawn rulers dynamically based on parameters
        SpawnRulersDynamically();
    }

    /// <summary>
    /// ⭐ DYNAMIC RULER SPAWNING ⭐
    /// Automatically spawns rulers based on shader parameters
    /// This is the KEY feature - no hard-coding needed!
    /// </summary>
    private void SpawnRulersDynamically()
    {
        Debug.Log($"[CorrectionInstaller] Spawning {_shaderParameters.Count} rulers...");

        foreach (var param in _shaderParameters)
        {
            // Spawn a ruler for each parameter
            _rulerSpawner.SpawnRuler(
                label: param.DisplayName,
                minValue: param.MinValue,
                maxValue: param.MaxValue,
                currentValue: param.CurrentValue,
                onValueChanged: (value) => OnParameterChanged(param.PropertyName, value)
            );

            Debug.Log($"[CorrectionInstaller] Spawned ruler for: {param.DisplayName} ({param.PropertyName})");
        }

        Debug.Log($"[CorrectionInstaller] ✅ Successfully spawned {_shaderParameters.Count} rulers!");
    }

    /// <summary>
    /// Called when any parameter value changes from UI
    /// </summary>
    private void OnParameterChanged(string propertyName, float value)
    {
        _logicHandler.UpdateParameter(propertyName, value);
    }

    /// <summary>
    /// Called when correction is complete
    /// </summary>
    private void OnCorrectionComplete()
    {
        Debug.Log("Correction Complete!");
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (_logicHandler != null)
        {
            _logicHandler.OnCorrectionComplete -= OnCorrectionComplete;
        }
    }
}
