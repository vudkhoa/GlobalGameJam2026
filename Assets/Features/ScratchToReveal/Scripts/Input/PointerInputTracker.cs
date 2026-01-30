using System;
using UnityEngine;

/// <summary>
/// General-purpose pointer input tracker
/// Tracks pointer down/move/up and emits screen position events
/// NOT scratch-specific - can be reused for any pointer tracking
/// </summary>
public class PointerInputTracker : MonoSingleton<PointerInputTracker>
{
    // Events - Emit screen positions only
    public event Action<Vector2> OnPointerDown;
    public event Action<Vector2> OnPointerMove;
    public event Action OnPointerUp;
    
    // Configuration
    private RectTransform _trackingArea;
    private Camera _camera;
    private float _minMoveDistance = 5f;
    
    // State
    private bool _isTracking;
    private Vector2 _lastScreenPosition;
    
    /// <summary>
    /// Initialize tracker with area and camera
    /// </summary>
    public void Initialize(RectTransform trackingArea, Camera camera, float minMoveDistance = 5f)
    {
        _trackingArea = trackingArea;
        _camera = camera;
        _minMoveDistance = minMoveDistance;
    }
    
    private void Update()
    {
        if (_trackingArea == null) return;
        
        HandlePointerInput();
    }
    
    private void HandlePointerInput()
    {
        Vector2 screenPos = Input.mousePosition;
        
        // Pointer down
        if (Input.GetMouseButtonDown(0))
        {
            if (IsPointerOverArea(screenPos))
            {
                _isTracking = true;
                _lastScreenPosition = screenPos;
                OnPointerDown?.Invoke(screenPos);
            }
        }
        // Pointer move
        else if (Input.GetMouseButton(0) && _isTracking)
        {
            if (Vector2.Distance(screenPos, _lastScreenPosition) >= _minMoveDistance)
            {
                OnPointerMove?.Invoke(screenPos);
                _lastScreenPosition = screenPos;
            }
        }
        // Pointer up
        else if (Input.GetMouseButtonUp(0) && _isTracking)
        {
            _isTracking = false;
            OnPointerUp?.Invoke();
        }
    }
    
    private bool IsPointerOverArea(Vector2 screenPosition)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(
            _trackingArea, 
            screenPosition, 
            _camera
        );
    }
}
