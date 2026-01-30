using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI Ruler component for adjusting shader parameters
/// Represents a single slider with label
/// </summary>
[RequireComponent(typeof(Slider))]
public class RulerUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Slider _slider;
    [SerializeField] private TextMeshProUGUI _labelText;
    [SerializeField] private TextMeshProUGUI _valueText;
    
    private System.Action<float> _onValueChanged;
    
    private void Awake()
    {
        if (_slider == null)
        {
            _slider = GetComponent<Slider>();
        }
    }
    
    /// <summary>
    /// Initialize ruler with parameters
    /// </summary>
    public void Initialize(string label, float minValue, float maxValue, float currentValue, System.Action<float> onValueChanged)
    {
        _onValueChanged = onValueChanged;
        
        // Setup slider
        _slider.minValue = minValue;
        _slider.maxValue = maxValue;
        _slider.value = currentValue;
        
        // Setup label
        if (_labelText != null)
        {
            _labelText.text = label;
        }
        
        // Setup value display
        UpdateValueDisplay(currentValue);
        
        // Add listener
        _slider.onValueChanged.AddListener(OnSliderValueChanged);
    }
    
    private void OnSliderValueChanged(float value)
    {
        UpdateValueDisplay(value);
        _onValueChanged?.Invoke(value);
    }
    
    private void UpdateValueDisplay(float value)
    {
        if (_valueText != null)
        {
            _valueText.text = value.ToString("F2");
        }
    }
    
    /// <summary>
    /// Set slider value without triggering callback
    /// </summary>
    public void SetValueWithoutNotify(float value)
    {
        _slider.SetValueWithoutNotify(value);
        UpdateValueDisplay(value);
    }
    
    private void OnDestroy()
    {
        _slider.onValueChanged.RemoveListener(OnSliderValueChanged);
    }
}
