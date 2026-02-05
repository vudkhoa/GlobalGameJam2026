using UnityEngine;

/// <summary>
/// SRP: Input processing service for beat circles
/// Responsibility: Convert screen input to world position and raycast to beats
/// Note: This is NOT a MonoBehaviour - it's a helper service called by GameLoopOSU
/// </summary>
public class BeatInputHandler
{
    private readonly Camera _gameCamera;
    private readonly LayerMask _beatLayerMask;
    private readonly bool _debugMode;

    public BeatInputHandler(Camera gameCamera, LayerMask beatLayerMask, bool debugMode = false)
    {
        _gameCamera = gameCamera;
        _beatLayerMask = beatLayerMask;
        _debugMode = debugMode;

        if (_gameCamera == null)
        {
            Debug.LogError("[BeatInputHandler] Camera is null! Input will not work.");
        }
    }

    // ═══════════════════════════════════════════════════════════
    // INPUT PROCESSING - Called by GameLoopOSU.Update()
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Process all current input (mouse + touch) and return the first valid beat hit
    /// Returns null if no beat was hit
    /// </summary>
    public BeatCircle ProcessInput()
    {
        if (_gameCamera == null) return null;

        // Handle mouse/touch input
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 inputPosition = _gameCamera.ScreenToWorldPoint(Input.mousePosition);
            return ProcessClick(inputPosition);
        }

        // Handle touch input for mobile (multiple touches)
        if (Input.touchCount > 0)
        {
            foreach (Touch touch in Input.touches)
            {
                if (touch.phase == TouchPhase.Began)
                {
                    Vector2 touchPosition = _gameCamera.ScreenToWorldPoint(touch.position);
                    BeatCircle beat = ProcessClick(touchPosition);

                    // Return first valid beat hit (prevents double-tap same frame)
                    if (beat != null) return beat;
                }
            }
        }

        return null;
    }

    // ═══════════════════════════════════════════════════════════
    // RAYCAST LOGIC
    // ═══════════════════════════════════════════════════════════

    private BeatCircle ProcessClick(Vector2 worldPosition)
    {
        if (_debugMode)
        {
            Debug.Log($"[BeatInputHandler] Click at world position: {worldPosition}");
        }

        // Perform raycast to find beat circles
        RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero, Mathf.Infinity, _beatLayerMask);

        if (hit.collider != null)
        {
            // Try to get BeatCircle component
            BeatCircle beatCircle = hit.collider.GetComponent<BeatCircle>();

            if (beatCircle != null && beatCircle.IsActive)
            {
                if (_debugMode)
                {
                    Debug.Log($"[BeatInputHandler] Hit beat circle at {beatCircle.transform.position}");
                }

                return beatCircle;
            }
            else if (_debugMode)
            {
                Debug.Log($"[BeatInputHandler] Hit object '{hit.collider.name}' but it's not an active beat");
            }
        }
        else if (_debugMode)
        {
            Debug.Log($"[BeatInputHandler] No hit detected");
        }

        return null;
    }

    // ═══════════════════════════════════════════════════════════
    // MULTI-TAP PROCESSING (Optional - for future features)
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Get all beats hit by current input (for multi-tap features)
    /// </summary>
    public BeatCircle[] ProcessAllInputs()
    {
        if (_gameCamera == null) return System.Array.Empty<BeatCircle>();

        var hits = new System.Collections.Generic.List<BeatCircle>();

        // Handle mouse
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 inputPosition = _gameCamera.ScreenToWorldPoint(Input.mousePosition);
            BeatCircle beat = ProcessClick(inputPosition);
            if (beat != null) hits.Add(beat);
        }

        // Handle all touches
        if (Input.touchCount > 0)
        {
            foreach (Touch touch in Input.touches)
            {
                if (touch.phase == TouchPhase.Began)
                {
                    Vector2 touchPosition = _gameCamera.ScreenToWorldPoint(touch.position);
                    BeatCircle beat = ProcessClick(touchPosition);
                    if (beat != null && !hits.Contains(beat))
                    {
                        hits.Add(beat);
                    }
                }
            }
        }

        return hits.ToArray();
    }
}