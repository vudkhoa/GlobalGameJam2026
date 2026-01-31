using UnityEngine;
using UnityEngine.Events; // Dùng để bắn sự kiện cho UI nghe

public class TapMechanic : MonoBehaviour
{
    [Header("Settings")]
    [Range(0f, 1f)] public float decayRate = 0.3f;  
    [Range(0f, 1f)] public float tapStrength = 0.1f;
    
    [Header("Debug Info")]
    [SerializeField] private float _currentValue = 0f;

    public UnityEvent<float> OnValueChanged;
    public UnityEvent OnTapSuccess;

    public float CurrentValue => _currentValue; 

    private void Update()
    {
        if (_currentValue > 0)
        {
            _currentValue -= decayRate * Time.deltaTime;
            _currentValue = Mathf.Clamp01(_currentValue); 
            
            OnValueChanged?.Invoke(_currentValue);
        }
    }

    public void AddPressure()
    {
        _currentValue += tapStrength;
        _currentValue = Mathf.Clamp01(_currentValue);
        
        OnValueChanged?.Invoke(_currentValue);
        OnTapSuccess?.Invoke();
    }
}