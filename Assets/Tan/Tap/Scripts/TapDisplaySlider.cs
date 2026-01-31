using UnityEngine;
using UnityEngine.UI;

public class TapDisplaySlider : MonoBehaviour
{
    [SerializeField] private Slider targetSlider;
    
    public void UpdateDisplay(float newValue)
    {
        if (targetSlider != null)
        {
            targetSlider.value = newValue;
        }
    }
}