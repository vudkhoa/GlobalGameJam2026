using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Event definitions for Correction mini-game
/// Centralized event management
/// </summary>
public static class CorrectionEvents
{
    /// <summary>
    /// Event triggered when correction is complete
    /// </summary>
    [System.Serializable]
    public class CorrectionCompleteEvent : UnityEvent { }
    
    /// <summary>
    /// Event triggered when progress changes
    /// </summary>
    [System.Serializable]
    public class ProgressChangedEvent : UnityEvent<float> { }
    
    /// <summary>
    /// Event triggered when a parameter is updated
    /// </summary>
    [System.Serializable]
    public class ParameterUpdatedEvent : UnityEvent<string, float> { }
}
