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

    private System.Action<float> _onValueChanged;

    // Cache for performance
    private float _containerWidth;
    private float _containerHeight;
    private bool _isHorizontal;

    [Header("Physics Settings")]
    [SerializeField] private float _pixelsPerUnit = 100f;
    [SerializeField] private float _inertiaDuration = 1.0f; // Duration for inertia tween
    [SerializeField] private float _snapDuration = 0.3f;    // Duration for snap tween
    [SerializeField] private Ease _inertiaEase = Ease.OutExpo;
    [SerializeField] private Ease _snapEase = Ease.OutBack;

    private Vector2 _lastPosition;
    private Vector2 _velocity;
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
        _currentValue = (_minValue + _maxValue) / 2f;

        UpdateContainerDimensions();
        if (_handle != null) _handle.anchoredPosition = Vector2.zero;

        // Sync initial value - Wrap it immediately to be safe
        _currentValue = WrapValue(_currentValue);
        _onValueChanged?.Invoke(_currentValue);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        KillTween(); // Stop any running animation immediately
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_container, eventData.position, eventData.pressEventCamera, out _lastPosition);
        _velocity = Vector2.zero; // Reset velocity
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

        // Calculate velocity (pixels per second) for inertia
        if (Time.deltaTime > 0)
            _velocity = delta / Time.deltaTime;

        _lastPosition = localPoint;

        MoveHandle(delta);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Start Inertia using DOTween
        float magnitude = _isHorizontal ? _velocity.x : _velocity.y;

        // Predict target value based on velocity
        float predictedChange = (magnitude * 0.2f) / _pixelsPerUnit;
        float startValue = _currentValue;
        float targetValueUnwrapped = startValue + predictedChange;

        // Tweens the *absolute* usage value, UpdateValueDirectly handles wrapping
        _physicsTween = DOVirtual.Float(startValue, targetValueUnwrapped, _inertiaDuration, (v) =>
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
        float currentDisplayValue = _currentValue;

        // Find nearest INT
        float targetInt = Mathf.Round(currentDisplayValue);

        // Tween to snap
        _physicsTween = DOVirtual.Float(currentDisplayValue, targetInt, _snapDuration, (v) =>
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

        float newValue = _currentValue + valueChange;
        UpdateValueDirectly(newValue);
    }

    private void UpdateValueDirectly(float newValue)
    {
        // LOOP VALUE Logic:
        // Ensure the value wraps around Min/Max
        _currentValue = WrapValue(newValue);

        // Notify Shader
        _onValueChanged?.Invoke(_currentValue);

        // Update Visuals based on (Wrapped) Value
        UpdateVisuals();
    }

    // Wraps logic between Min and Max
    private float WrapValue(float v)
    {
        float range = _maxValue - _minValue;
        if (range <= 0) return _minValue;

        // Use Mathf.Repeat to loop 
        // Note: Repeat(t, length) loops t between 0 and length
        // We shift by minValue to loop between minValue and maxValue
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
