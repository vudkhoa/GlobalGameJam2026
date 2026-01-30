using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// SRP: Display combo counter
/// Responsibility: Show combo when threshold reached, animate
/// </summary>
public class ComboDisplay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Text _comboText;

    [Header("Settings")]
    [SerializeField] private int _comboThreshold = 5;
    [SerializeField] private float _pulseDuration = 0.1f;
    [SerializeField] private float _pulseScale = 1.2f;

    private Sequence _pulseSequence;

    // ═══════════════════════════════════════════════════════════
    // PUBLIC API
    // ═══════════════════════════════════════════════════════════

    public void UpdateCombo(int combo)
    {
        if (combo >= _comboThreshold)
        {
            ShowCombo(combo);
        }
        else
        {
            HideCombo();
        }
    }

    // ═══════════════════════════════════════════════════════════
    // PRIVATE
    // ═══════════════════════════════════════════════════════════

    private void ShowCombo(int combo)
    {
        // Enable text
        if (!_comboText.gameObject.activeSelf)
        {
            _comboText.gameObject.SetActive(true);
        }

        // Update text
        _comboText.text = $"COMBO x{combo}";

        // Pulse animation
        PlayPulseAnimation();
    }

    private void HideCombo()
    {
        _pulseSequence?.Kill();
        _comboText.gameObject.SetActive(false);
    }

    private void PlayPulseAnimation()
    {
        // Kill existing animation
        _pulseSequence?.Kill();

        // Create pulse sequence
        _pulseSequence = DOTween.Sequence();
        _pulseSequence.Append(_comboText.transform.DOScale(_pulseScale, _pulseDuration));
        _pulseSequence.Append(_comboText.transform.DOScale(1f, _pulseDuration));
    }

    // ═══════════════════════════════════════════════════════════
    // CLEANUP
    // ═══════════════════════════════════════════════════════════

    private void OnDestroy()
    {
        _pulseSequence?.Kill();
    }
}