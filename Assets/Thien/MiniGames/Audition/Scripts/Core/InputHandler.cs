using UnityEngine;
using System;

/// <summary>
/// SRP: Detect player input
/// Responsibility: Listen for touch/mouse input
/// </summary>
public class InputHandler : MonoBehaviour
{
    public event Action OnTapInput;

    private void Update()
    {
        bool inputDetected = false;

        // Touch (primary for mobile)
        if (GameConstants.ALLOW_TOUCH && Input.touchCount > 0)
        {
            if (Input.GetTouch(0).phase == TouchPhase.Began)
            {
                inputDetected = true;
            }
        }

        // Mouse (debug)
        if (GameConstants.ALLOW_MOUSE && Input.GetMouseButtonDown(0))
        {
            inputDetected = true;
        }

        if (inputDetected)
        {
            OnTapInput?.Invoke();
        }
    }
}