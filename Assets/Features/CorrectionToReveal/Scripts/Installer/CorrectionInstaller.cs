using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Main installer for Correction mini-game
/// Manages Multi-Level Progression and Resource Lifecycle
/// </summary>
public class CorrectionInstaller : MonoBehaviour
{
    [Header("Level Config")]
    [Tooltip("List of all levels (CorrectionSettings) to play in sequence")]
    [SerializeField] private List<CorrectionSettings> _levels = new List<CorrectionSettings>();

    [Header("UI")]
    [SerializeField] private Transform _rulerContainer;
    [SerializeField] private Image _revealImage;

    // Dependencies
    private CorrectionLogicHandler _logicHandler;
    private RulerSpawner _rulerSpawner;

    // Current State
    private int _currentLevelIndex = 0;

    // Transient Level Data (Cleaned up/Recreated per level)
    private Material _currentMaterialInstance;
    private Sprite _currentSpriteInstance;
    private CorrectionData _data;
    private CorrectionValidator _validator;
    private ShaderParameterApplier _shaderApplier;
    private List<ShaderParameter> _shaderParameters;

    private CorrectionAnimationManager _animationManager;
    public CorrectionAnimationManager AnimationManager => _animationManager;

    public CorrectionLogicHandler LogicHandler => _logicHandler;
    public RulerSpawner RulerSpawner => _rulerSpawner;

    private void OnEnable()
    {
        InstallDependencies();
    }

    private void InstallDependencies()
    {
        // 1. Initialize persistent systems (Ruler Spawner)
        // We use the first level's prefab as the base for pooling (Assuming consistent prefabs)
        GameObject rulerPrefab = (_levels.Count > 0 && _levels[0] != null) ? _levels[0].RulerPrefab : null;
        float defaultSpacing = (_levels.Count > 0 && _levels[0] != null) ? _levels[0].RulerSpacing : 150f;

        if (_rulerSpawner == null)
        {
            _rulerSpawner = new RulerSpawner(rulerPrefab, _rulerContainer, defaultSpacing);
        }

        // Setup Animation Manager
        if (_animationManager == null)
        {
            _animationManager = GetComponent<CorrectionAnimationManager>();
            if (_animationManager == null) _animationManager = gameObject.AddComponent<CorrectionAnimationManager>();
        }
        _animationManager.Initialize(_revealImage, _rulerContainer);

        // 2. Load the first level
        if (_levels.Count > 0)
        {
            LoadLevel(0);
        }
        else
        {
            // No levels configured
        }
    }

    /// <summary>
    /// Advance to the next level
    /// Returns true if successful, false if all levels are complete
    /// </summary>
    public bool AdvanceLevel()
    {
        int nextIndex = _currentLevelIndex + 1;
        if (nextIndex < _levels.Count)
        {
            LoadLevel(nextIndex);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Load a specific level by index
    /// Handles cleanup and transition
    /// </summary>
    private void LoadLevel(int index)
    {
        if (index < 0 || index >= _levels.Count) return;
        CorrectionSettings settings = _levels[index];
        if (settings == null) return;

        _currentLevelIndex = index;

        // 1. Cleanup Old Visuals (Prevent Memory Leaks)
        CleanupGeneratesAssets();

        // 2. Setup New Visuals
        SetupVisuals(settings);

        // 3. Create Shader Parameters (Data)
        _shaderParameters = ShaderParameterFactory.CreateAllParameters(settings);

        // 4. Create Logic Components
        _data = new CorrectionData();
        _data.Initialize(_shaderParameters, settings.Tolerance);

        _validator = new CorrectionValidator(_data);

        // Use the INSTANCE material for the applier so we modify the visible visual only
        _shaderApplier = new ShaderParameterApplier(_currentMaterialInstance);

        // 5. Initialize or Update Logic Handler
        if (_logicHandler == null)
        {
            _logicHandler = new CorrectionLogicHandler(_data, _validator, _shaderApplier);
            _logicHandler.OnCorrectionComplete += OnCorrectionComplete;
        }
        else
        {
            // Transfer logic handler to new data context
            _logicHandler.LoadLevel(_data, _validator, _shaderApplier);
        }

        // 6. Setup Rulers (Reuse objects)
        if (_rulerSpawner != null)
        {
            _rulerSpawner.SetSpacing(settings.RulerSpacing);
            _rulerSpawner.RepopulateRulers(_shaderParameters, OnParameterChanged);
        }
    }

    private void SetupVisuals(CorrectionSettings settings)
    {
        if (_revealImage == null) return;

        // Instantiate Material
        if (settings.CorrectionMaterial != null)
        {
            _currentMaterialInstance = new Material(settings.CorrectionMaterial);
            _revealImage.material = _currentMaterialInstance;
        }

        // Create Sprite
        if (settings.TargetTexture != null)
        {
            Texture2D tex = settings.TargetTexture;
            Rect rect = new Rect(0, 0, tex.width, tex.height);
            _currentSpriteInstance = Sprite.Create(tex, rect, Vector2.one * 0.5f);

            _revealImage.sprite = _currentSpriteInstance;
        }
    }

    private void CleanupGeneratesAssets()
    {
        // Destroy Material Instance
        if (_currentMaterialInstance != null)
        {
            SafeDestroy(_currentMaterialInstance);
            _currentMaterialInstance = null;
        }

        // Destroy Sprite Instance
        if (_currentSpriteInstance != null)
        {
            SafeDestroy(_currentSpriteInstance);
            _currentSpriteInstance = null;
        }

        if (_revealImage != null) _revealImage.sprite = null;
    }

    private void SafeDestroy(Object obj)
    {
        if (obj == null) return;
#if UNITY_EDITOR
        if (Application.isPlaying)
            Destroy(obj);
        else
            DestroyImmediate(obj);
#else
        Destroy(obj);
#endif
    }

    private void OnParameterChanged(string propertyName, float value)
    {
        if (_logicHandler != null)
        {
            _logicHandler.UpdateParameter(propertyName, value);
        }
    }

    private void OnCorrectionComplete()
    {
    }

    private void OnDestroy()
    {
        CleanupGeneratesAssets();
        if (_logicHandler != null)
        {
            _logicHandler.OnCorrectionComplete -= OnCorrectionComplete;
        }
    }
}
