using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// UI Ruler component for adjusting shader parameters
/// custom implementation of a slider/handle mechanism
/// Supports infinite scrolling, value looping, and smooth animations using DOTween
/// </summary>
public class RulerUI : MonoBehaviour, IDragHandler, IPointerDownHandler, IBeginDragHandler, IEndDragHandler
{
    [Header("Interaction Components")]
    [Tooltip("The background container defining the slide area")]
    [SerializeField] private RectTransform _container;

    [Tooltip("The image to slide")]
    [SerializeField] private RectTransform _handle;

    private float _minValue;
    private float _maxValue;
    private float _currentValue;
    private float _virtualValue; // Linear, unbounded value for calculating PingPong

    private System.Action<float> _onValueChanged;
    private bool _isInteractable = true;

    // Cache for performance
    private float _containerWidth;
    private float _containerHeight;
    private bool _isHorizontal;

    [Header("Physics Settings")]
    [SerializeField] private float _dragPixelSteps = 40f; // Pixels required to move 1 step
    [SerializeField] private float _valueStep = 0.1f;     // Value change per step
    [SerializeField] private float _pixelsPerUnit = 100f; // Kept for reference/inertia, but logic overrides

    private float _dragAccumulator; // Accumulates drag delta
    [SerializeField] private float _inertiaDuration = 1.0f; // Duration for inertia tween
    [SerializeField] private float _snapDuration = 0.5f;    // Increased for smoother snap
    [SerializeField] private Ease _inertiaEase = Ease.OutCubic; // Smoother natural stop
    [SerializeField] private Ease _snapEase = Ease.OutQuad;

    private Vector2 _lastPosition;
    private Vector2 _velocity;
    private Vector2 _smoothedVelocity; // Smoothed velocity for better release
    private Tween _physicsTween;

    private void Awake()
    {
        if (_container == null) _container = GetComponent<RectTransform>();

        if (_handle == null && transform.childCount > 0)
        {
            foreach (RectTransform child in transform)
            {
                if (child != _container)
                {
                    _handle = child;
                    break;
                }
            }
        }
    }

    private void Start()
    {
        UpdateContainerDimensions();
    }

    public void Initialize(string label, float minValue, float maxValue, float currentValue, System.Action<float> onValueChanged)
    {
        _minValue = minValue;
        _maxValue = maxValue;
        _onValueChanged = onValueChanged;

        // Initialize Center (Midpoint)
        _currentValue = currentValue;
        _virtualValue = currentValue; // Start virtual aligned with current

        UpdateContainerDimensions();
        if (_handle != null) _handle.anchoredPosition = Vector2.zero;

        // Sync initial value
        UpdateValueDirectly(_virtualValue);
    }

    public void SetInteractable(bool state)
    {
        _isInteractable = state;
        if (!_isInteractable)
        {
            KillTween();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!_isInteractable) return;
        KillTween(); // Stop any running animation immediately
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_container, eventData.position, eventData.pressEventCamera, out _lastPosition);
        _velocity = Vector2.zero;
        _smoothedVelocity = Vector2.zero;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!_isInteractable) return;
        KillTween();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_container, eventData.position, eventData.pressEventCamera, out _lastPosition);
        _dragAccumulator = 0f; // Reset accumulator on new drag
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_isInteractable) return;
        if (_container == null || _handle == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(_container, eventData.position, eventData.pressEventCamera, out Vector2 localPoint);

        Vector2 delta = localPoint - _lastPosition;

        // Calculate velocity significantly smoother
        if (Time.deltaTime > 0)
        {
            Vector2 instantVelocity = delta / Time.deltaTime;
            // Low-pass filter for velocity to remove jitter (Lerp factor 0.1 - 0.2 works well)
            _smoothedVelocity = Vector2.Lerp(_smoothedVelocity, instantVelocity, 0.2f);
        }

        _lastPosition = localPoint;
        _velocity = _smoothedVelocity; // Update main velocity for Release

        MoveHandle(delta);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!_isInteractable) return;
        // Start Inertia
        float magnitude = _isHorizontal ? _smoothedVelocity.x : _smoothedVelocity.y;

        // Predict target value based on velocity
        // Derived sensitivity from steps: 
        float derivedPixelsPerUnit = _dragPixelSteps / _valueStep;
        float predictedChange = (magnitude * 0.25f) / derivedPixelsPerUnit;

        float startValue = _virtualValue;
        float targetValueVirtual = startValue + predictedChange;

        // Tweens the virtual value
        _physicsTween = DOVirtual.Float(startValue, targetValueVirtual, _inertiaDuration, (v) =>
        {
            // During inertia, we likely want smooth movement, OR stepped?
            // User requested "slide theo step". 
            // We will let tween update smoothly but 'UpdateValueDirectly' handles the rounding for logic.
            UpdateValueDirectly(v);
        })
        .SetEase(_inertiaEase)
        .OnComplete(() =>
        {
            SnapToNearestStep();
        });
    }

    private void SnapToNearestStep()
    {
        // Snap to nearest 0.1 step
        float currentVirtual = _virtualValue;
        float steps = Mathf.Round(currentVirtual / _valueStep);
        float targetStep = steps * _valueStep;

        _physicsTween = DOVirtual.Float(currentVirtual, targetStep, _snapDuration, (v) =>
        {
            UpdateValueDirectly(v);
        })
        .SetEase(_snapEase);
    }

    private void KillTween()
    {
        if (_physicsTween != null && _physicsTween.IsActive())
        {
            _physicsTween.Kill();
        }
    }

    private void MoveHandle(Vector2 pixelDelta)
    {
        float move = _isHorizontal ? pixelDelta.x : pixelDelta.y;
        _dragAccumulator += move;

        // Check if we crossed the step threshold
        if (Mathf.Abs(_dragAccumulator) >= _dragPixelSteps)
        {
            // How many steps?
            int steps = (int)(_dragAccumulator / _dragPixelSteps);
            if (steps != 0)
            {
                float valueChange = steps * _valueStep;
                _virtualValue += valueChange;

                // Reduce accumulator by the consumed steps
                _dragAccumulator -= steps * _dragPixelSteps;

                UpdateValueDirectly(_virtualValue);
            }
        }
    }

    private void UpdateValueDirectly(float newVirtualValue)
    {
        // Update stored virtual value (critical for Tween updates)
        _virtualValue = newVirtualValue;

        // 1. Calculate Shader Value (PingPong -> Smooth Oscillation)
        float shaderValue = CalculatePingPongValue(_virtualValue);

        // Apply Rounding to Nearest 0.1f as requested (Polish)
        // This ensures the logic always operates on clean 0.1 increments
        shaderValue = Mathf.Round(shaderValue * 10f) / 10f;
        shaderValue = Mathf.Clamp(shaderValue, _minValue, _maxValue); // Safety Clamp

        _onValueChanged?.Invoke(shaderValue);

        // 2. Calculate Visual Value (Repeat -> Continuous Slide Loop)
        // This ensures the handle visually loops from End to Start, maintaining slide direction
        _currentValue = CalculateLoopValue(_virtualValue); // Update _currentValue for correct Visual Positioning
        UpdateVisuals();
    }

    // PingPong logic for Shader (0 -> 1 -> 0)
    private float CalculatePingPongValue(float v)
    {
        float range = _maxValue - _minValue;
        if (range <= 0) return _minValue;

        return _minValue + Mathf.PingPong(v - _minValue, range);
    }

    // Repeat logic for Visuals (0 -> 1 -> 0 -> 1)
    private float CalculateLoopValue(float v)
    {
        float range = _maxValue - _minValue;
        if (range <= 0) return _minValue;

        return _minValue + Mathf.Repeat(v - _minValue, range);
    }

    private void UpdateVisuals()
    {
        if (_container == null || _handle == null) return;

        // Visualize the current wrapped value relative to the container
        float normalized = Mathf.InverseLerp(_minValue, _maxValue, _currentValue);

        // Map 0..1 to -Size/2 .. Size/2
        if (_isHorizontal)
        {
            float targetX = Mathf.Lerp(-_containerWidth / 2f, _containerWidth / 2f, normalized);
            _handle.anchoredPosition = new Vector2(targetX, 12f);
        }
        else
        {
            float targetY = Mathf.Lerp(-_containerHeight / 2f, _containerHeight / 2f, normalized);
            _handle.anchoredPosition = new Vector2(0, targetY);
        }
    }

    private void UpdateContainerDimensions()
    {
        if (_container != null)
        {
            _containerWidth = _container.rect.width;
            _containerHeight = _container.rect.height;
            _isHorizontal = _containerWidth > _containerHeight;
        }
    }
}
