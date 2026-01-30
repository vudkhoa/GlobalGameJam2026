using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Pure dependency injection container
/// ONLY responsible for creating and wiring components
/// NO business logic - all logic delegated to ScratchLogicHandler
/// </summary>
public class ScratchInstaller : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform _scratchArea;
    [SerializeField] private Image _scratchLayer;
    [SerializeField] private Image _resultLayer;
    [SerializeField] private ScratchSettings _settings;

    [Header("Events")]
    public UnityEvent<float> OnProgressChanged;

    // Components (created via DI)
    private ScratchData _data;
    private MaskRenderer _maskRenderer;
    private ScratchProgressCalculator _progressCalculator;
    private CoordinateConverter _coordinateConverter;
    private ScratchLogicHandler _scratchLogic;
    private Material _scratchMaterial;

    private void Awake()
    {
        // Step 1: Create data
        CreateData();

        // Step 2: Create logic components (inject dependencies)
        CreateLogicComponents();

        // Step 3: Create main logic handler (inject all dependencies)
        CreateScratchLogic();

        // Step 4: Setup input tracker (wire events)
        WireInputTracker();

        // Step 5: Setup material
        SetupMaterial();

        // Step 6: Wire scratch logic events
        WireScratchLogicEvents();
    }

    // ========== Helper Methods ==========

    /// <summary>
    /// Get correct camera based on Canvas render mode
    /// </summary>
    private Camera GetCameraForCanvas()
    {
        Canvas canvas = _scratchArea.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("No Canvas found! Using Camera.main");
            return Camera.main;
        }

        switch (canvas.renderMode)
        {
            case RenderMode.ScreenSpaceOverlay:
                // Overlay mode doesn't use camera
                return null;

            case RenderMode.ScreenSpaceCamera:
                // Use assigned world camera
                return canvas.worldCamera;

            case RenderMode.WorldSpace:
                // Use event camera or main camera
                return canvas.worldCamera != null ? canvas.worldCamera : Camera.main;

            default:
                return Camera.main;
        }
    }

    // ========== Setup Steps ==========

    /// <summary>
    /// Step 1: Create runtime data
    /// </summary>
    private void CreateData()
    {
        _data = new ScratchData(_settings.maskResolution);
    }

    /// <summary>
    /// Step 2: Create logic components with dependencies
    /// </summary>
    private void CreateLogicComponents()
    {
        _maskRenderer = new MaskRenderer(
            _data.MaskTexture,
            _settings.brushSize,
            _settings.brushOpacity
        );

        _progressCalculator = new ScratchProgressCalculator(
            _data.MaskTexture,
            _settings.maskResolution
        );

        // Use correct camera based on Canvas render mode
        Camera cam = GetCameraForCanvas();
        _coordinateConverter = new CoordinateConverter(
            _scratchArea,
            cam
        );
    }

    /// <summary>
    /// Step 3: Create main scratch logic handler (inject all dependencies)
    /// </summary>
    private void CreateScratchLogic()
    {
        _scratchLogic = new ScratchLogicHandler(
            _maskRenderer,
            _progressCalculator,
            _coordinateConverter
        );
    }

    /// <summary>
    /// Step 4: Wire input tracker to scratch logic
    /// Input events → Scratch logic actions
    /// </summary>
    private void WireInputTracker()
    {
        PointerInputTracker tracker = PointerInputTracker.Instance;

        // Detect correct camera based on Canvas render mode
        Camera cam = GetCameraForCanvas();
        tracker.Initialize(_scratchArea, cam, _settings.minMoveDistance);

        // Wire input events → scratch logic (action-based methods)
        tracker.OnPointerDown += _scratchLogic.StartScratchAtPosition;
        tracker.OnPointerMove += _scratchLogic.ContinueScratchAtPosition;
        tracker.OnPointerUp += _scratchLogic.FinalizeScratch;
    }


    /// <summary>
    /// Step 5: Setup material for scratch layer
    /// </summary>
    private void SetupMaterial()
    {
        Shader shader = Shader.Find("Custom/ScratchReveal");
        if (shader == null)
        {
            Debug.LogError("Shader 'Custom/ScratchReveal' not found!");
            return;
        }

        _scratchMaterial = new Material(shader);
        _scratchMaterial.SetTexture("_MainTex", _settings.scratchLayerTexture);
        _scratchMaterial.SetTexture("_MaskTex", _data.MaskTexture);
        _scratchMaterial.SetColor("_Color", _settings.scratchLayerColor);

        _scratchLayer.material = _scratchMaterial;
    }

    /// <summary>
    /// Step 6: Wire scratch logic events to Unity events
    /// </summary>
    private void WireScratchLogicEvents()
    {
        _scratchLogic.OnProgressChanged += HandleProgressChanged;
    }

    /// <summary>
    /// Forward progress event to Unity event
    /// </summary>
    private void HandleProgressChanged(float percent)
    {
        OnProgressChanged?.Invoke(percent);
    }

    // ========== Public API ==========

    /// <summary>
    /// Reset scratch to initial state
    /// </summary>
    public void ResetScratch()
    {
        _scratchLogic?.Reset();
    }

    /// <summary>
    /// Get current progress (0-100)
    /// </summary>
    public float GetProgress()
    {
        return _scratchLogic?.GetProgress() ?? 0f;
    }

    // ========== Cleanup ==========

    private void OnDestroy()
    {
        // Cleanup data
        _data?.Dispose();
        _progressCalculator?.Dispose();
        _maskRenderer?.Dispose();

        if (_scratchMaterial != null)
        {
            Destroy(_scratchMaterial);
        }

        // Unwire input tracker
        var tracker = PointerInputTracker.Instance;
        if (tracker != null)
        {
            tracker.OnPointerDown -= _scratchLogic.StartScratchAtPosition;
            tracker.OnPointerMove -= _scratchLogic.ContinueScratchAtPosition;
            tracker.OnPointerUp -= _scratchLogic.FinalizeScratch;
        }

        // Unwire scratch logic
        if (_scratchLogic != null)
        {
            _scratchLogic.OnProgressChanged -= HandleProgressChanged;
        }
    }
}
