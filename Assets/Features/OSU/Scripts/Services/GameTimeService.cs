using UnityEngine;

/// <summary>
/// Service: Manage game time (not MonoSingleton - injected via DI)
/// </summary>
public class GameTimeService
{
    private float _gameTime;
    private bool _isRunning;
    private float _timeScale = 1f;

    // ═══════════════════════════════════════════════════════════
    // PUBLIC API
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Start time tracking
    /// </summary>
    public void Start()
    {
        _isRunning = true;
    }

    /// <summary>
    /// Stop time tracking
    /// </summary>
    public void Stop()
    {
        _isRunning = false;
    }

    /// <summary>
    /// Reset time to zero
    /// </summary>
    public void Reset()
    {
        _gameTime = 0f;
    }

    /// <summary>
    /// Pause time
    /// </summary>
    public void Pause()
    {
        _isRunning = false;
    }

    /// <summary>
    /// Resume time
    /// </summary>
    public void Resume()
    {
        _isRunning = true;
    }

    /// <summary>
    /// Update time (call from MonoBehaviour Update)
    /// </summary>
    public void Update(float deltaTime)
    {
        if (_isRunning)
        {
            _gameTime += deltaTime * _timeScale;
        }
    }

    /// <summary>
    /// Set time scale (slow-mo, fast-forward)
    /// </summary>
    public void SetTimeScale(float scale)
    {
        _timeScale = Mathf.Max(0f, scale);
    }

    /// <summary>
    /// Set time directly
    /// </summary>
    public void SetTime(float time)
    {
        _gameTime = Mathf.Max(0f, time);
    }

    // ═══════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════

    public float CurrentTime => _gameTime;
    public bool IsRunning => _isRunning;
    public float TimeScale => _timeScale;
}
