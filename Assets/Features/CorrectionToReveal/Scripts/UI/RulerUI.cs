using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// UI Ruler component for adjusting shader parameters
/// custom implementation of a slider/handle mechanism
/// </summary>
public class RulerUI : MonoBehaviour, IDragHandler, IPointerDownHandler
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

    private void Awake()
    {
        // Auto-detect container if not assigned
        if (_container == null)
            _container = GetComponent<RectTransform>();

        // Auto-search for a handle if not assigned (first child usually)
        if (_handle == null && transform.childCount > 0)
        {
            // Try to find a child that looks like a handle
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

    /// <summary>
    /// Initialize ruler with parameters
    /// </summary>
    public void Initialize(string label, float minValue, float maxValue, float currentValue, System.Action<float> onValueChanged)
    {
        _minValue = minValue;
        _maxValue = maxValue;
        _currentValue = currentValue;
        _onValueChanged = onValueChanged;

        // Update visual position
        UpdateHandlePosition();
    }

    private void UpdateContainerDimensions()
    {
        if (_container != null)
        {
            _containerWidth = _container.rect.width;
            _containerHeight = _container.rect.height;

            // Determine orientation based on aspect ratio
            // If Width > Height -> Horizontal (X axis)
            // If Height > Width -> Vertical (Y axis)
            _isHorizontal = _containerWidth > _containerHeight;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        UpdateDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        UpdateDrag(eventData);
    }

    private void UpdateDrag(PointerEventData eventData)
    {
        if (_container == null || _handle == null) return;

        // Convert screen point to local point in container
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _container,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint
        );

        // Update dimensions in case of layout changes
        UpdateContainerDimensions();

        float normalizedValue = 0f;

        if (_isHorizontal)
        {
            // Rule: x tăng -> value tăng (Left to Right)
            // Calculate normalized position (-0.5 to 0.5 range usually for centered pivot)
            // Shift to 0 to 1 range
            float clampedX = Mathf.Clamp(localPoint.x, -_containerWidth / 2f, _containerWidth / 2f);

            // Map [-Width/2, Width/2] to [0, 1]
            normalizedValue = Mathf.InverseLerp(-_containerWidth / 2f, _containerWidth / 2f, clampedX);

            // Update handle position (keep Y centered)
            _handle.anchoredPosition = new Vector2(clampedX, 0);
        }
        else
        {
            // Rule: y giảm -> value giảm (Down -> Decrease, Up -> Increase)
            // This means we map Bottom (-Height/2) to 0, Top (Height/2) to 1
            float clampedY = Mathf.Clamp(localPoint.y, -_containerHeight / 2f, _containerHeight / 2f);

            normalizedValue = Mathf.InverseLerp(-_containerHeight / 2f, _containerHeight / 2f, clampedY);

            // Update handle position (keep X centered)
            _handle.anchoredPosition = new Vector2(0, clampedY);
        }

        // Calculate actual value
        float newValue = Mathf.Lerp(_minValue, _maxValue, normalizedValue);

        // Notify change if different
        if (!Mathf.Approximately(_currentValue, newValue))
        {
            _currentValue = newValue;
            _onValueChanged?.Invoke(_currentValue);
        }
    }

    /// <summary>
    /// Update handle position based on current value
    /// </summary>
    private void UpdateHandlePosition()
    {
        if (_container == null || _handle == null) return;

        UpdateContainerDimensions();

        float normalized = Mathf.InverseLerp(_minValue, _maxValue, _currentValue);

        if (_isHorizontal)
        {
            float x = Mathf.Lerp(-_containerWidth / 2f, _containerWidth / 2f, normalized);
            _handle.anchoredPosition = new Vector2(x, 0);
        }
        else
        {
            float y = Mathf.Lerp(-_containerHeight / 2f, _containerHeight / 2f, normalized);
            _handle.anchoredPosition = new Vector2(0, y);
        }
    }
}
