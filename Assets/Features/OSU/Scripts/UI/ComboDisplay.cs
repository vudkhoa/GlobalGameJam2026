using UnityEngine;
using DG.Tweening;
using TMPro;

/// <summary>
/// SRP: Display combo counter using 3D TextMeshPro (MeshRenderer)
/// Responsibility: Show combo when threshold reached, animate
/// Uses near-zero scale instead of SetActive for performance
/// </summary>
public class ComboDisplay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshPro _comboText; // 3D TextMeshPro

    [Header("Animation Settings")]
    [SerializeField] private int _comboThreshold = 5;
    [SerializeField] private float _showHideDuration = 0.2f;
    [SerializeField] private float _pulseDuration = 0.1f;
    [SerializeField] private float _pulseScaleMultiplier = 1.2f;

    private static readonly Vector3 HiddenScale = Vector3.one * 0.001f;

    private Sequence _pulseSequence;
    private bool _isVisible;
    private Vector3 _initialScale;

    // ═══════════════════════════════════════════════════════════
    // INITIALIZATION
    // ═══════════════════════════════════════════════════════════

    private void Awake()
    {
        if (_comboText == null) return;

        // Remember initial scale from Inspector (preserve all Inspector settings)
        _initialScale = _comboText.transform.localScale;

        // Keep active, hide via near-zero scale (avoid Vector3.zero to prevent NaN AABB)
        _comboText.gameObject.SetActive(true);
        _comboText.transform.localScale = HiddenScale;
        _isVisible = false;
    }

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
        // Scale up if not visible
        if (!_isVisible)
        {
            DOTween.Kill(_comboText.transform);
            _comboText.transform.DOScale(_initialScale, _showHideDuration).SetEase(Ease.OutBack);
            _isVisible = true;
        }

        // Update text only
        _comboText.text = $"COMBO x{combo}";

        // Pulse animation
        PlayPulseAnimation();
    }

    private void HideCombo()
    {
        if (!_isVisible) return;

        _pulseSequence?.Kill();
        DOTween.Kill(_comboText.transform);
        _comboText.transform.DOScale(HiddenScale, _showHideDuration).SetEase(Ease.InBack);
        _isVisible = false;
    }

    private void PlayPulseAnimation()
    {
        _pulseSequence?.Kill();

        // Pulse based on initial scale from Inspector
        Vector3 pulseScale = _initialScale * _pulseScaleMultiplier;

        _pulseSequence = DOTween.Sequence();
        _pulseSequence.Append(_comboText.transform.DOScale(pulseScale, _pulseDuration));
        _pulseSequence.Append(_comboText.transform.DOScale(_initialScale, _pulseDuration));
    }

    // ═══════════════════════════════════════════════════════════
    // CLEANUP
    // ═══════════════════════════════════════════════════════════

    private void OnDestroy()
    {
        _pulseSequence?.Kill();
        DOTween.Kill(_comboText.transform);
    }
}