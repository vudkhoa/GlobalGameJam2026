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

    // Cache for performance
    private float _containerWidth;
    private float _containerHeight;
    private bool _isHorizontal;

    [Header("Physics Settings")]
    [SerializeField] private float _pixelsPerUnit = 100f;
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

    public void OnPointerDown(PointerEventData eventData)
    {
        KillTween(); // Stop any running animation immediately
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_container, eventData.position, eventData.pressEventCamera, out _lastPosition);
        _velocity = Vector2.zero;
        _smoothedVelocity = Vector2.zero;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        KillTween();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_container, eventData.position, eventData.pressEventCamera, out _lastPosition);
    }

    public void OnDrag(PointerEventData eventData)
    {
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
        // Start Inertia using DOTween
        float magnitude = _isHorizontal ? _smoothedVelocity.x : _smoothedVelocity.y;

        // Predict target value based on velocity
        float predictedChange = (magnitude * 0.25f) / _pixelsPerUnit; // Tuned factor

        // Use Virtual Value for inertia calculation to maintain direction/momentum even across boundaries
        float startValue = _virtualValue;
        float targetValueVirtual = startValue + predictedChange;

        // Tweens the virtual value
        _physicsTween = DOVirtual.Float(startValue, targetValueVirtual, _inertiaDuration, (v) =>
        {
            UpdateValueDirectly(v);
        })
        .SetEase(_inertiaEase)
        .OnComplete(() =>
        {
            SnapToNearestInteger();
        });
    }

    private void SnapToNearestInteger()
    {
        // Snap the VIRTUAL value to nearest integer to keep alignment consistent
        float currentVirtual = _virtualValue;
        float targetInt = Mathf.Round(currentVirtual);

        _physicsTween = DOVirtual.Float(currentVirtual, targetInt, _snapDuration, (v) =>
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
        float valueChange = 0f;

        if (_isHorizontal)
        {
            float moveX = pixelDelta.x;
            valueChange = moveX / _pixelsPerUnit;
        }
        else
        {
            float moveY = pixelDelta.y;
            valueChange = moveY / _pixelsPerUnit;
        }

        // Apply change to linear virtual value
        _virtualValue += valueChange;

        UpdateValueDirectly(_virtualValue);
    }

    private void UpdateValueDirectly(float newVirtualValue)
    {
        // Update stored virtual value (critical for Tween updates)
        _virtualValue = newVirtualValue;

        // 1. Calculate Shader Value (PingPong -> Smooth Oscillation)
        float shaderValue = CalculatePingPongValue(_virtualValue);
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
            _handle.anchoredPosition = new Vector2(targetX, 0);
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
