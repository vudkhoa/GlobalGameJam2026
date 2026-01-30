using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Main UI controller for correction mini-game
/// Manages UI state and feedback
/// </summary>
public class CorrectionUIController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Transform _rulerContainer;
    [SerializeField] private TextMeshProUGUI _progressText;
    [SerializeField] private TextMeshProUGUI _feedbackText;
    [SerializeField] private Image _progressBar;
    
    [Header("Feedback Messages")]
    [SerializeField] private string _startMessage = "Adjust the rulers to correct the image!";
    [SerializeField] private string _completeMessage = "Perfect! Image corrected!";
    
    private RulerSpawner _rulerSpawner;
    
    public Transform RulerContainer => _rulerContainer;
    
    /// <summary>
    /// Initialize UI controller with ruler spawner
    /// </summary>
    public void Initialize(RulerSpawner rulerSpawner)
    {
        _rulerSpawner = rulerSpawner;
        UpdateFeedback(_startMessage);
        UpdateProgress(0f);
    }
    
    /// <summary>
    /// Update progress display
    /// </summary>
    public void UpdateProgress(float progress)
    {
        if (_progressBar != null)
        {
            _progressBar.fillAmount = progress;
        }
        
        if (_progressText != null)
        {
            _progressText.text = $"Progress: {(progress * 100f):F0}%";
        }
    }
    
    /// <summary>
    /// Update feedback message
    /// </summary>
    public void UpdateFeedback(string message)
    {
        if (_feedbackText != null)
        {
            _feedbackText.text = message;
        }
    }
    
    /// <summary>
    /// Show completion state
    /// </summary>
    public void ShowCompletion()
    {
        UpdateFeedback(_completeMessage);
        UpdateProgress(1f);
    }
    
    /// <summary>
    /// Clear all UI elements
    /// </summary>
    public void Clear()
    {
        _rulerSpawner?.ClearRulers();
    }
}
