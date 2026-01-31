using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

/// <summary>
/// SRP: Display judgement feedback (PERFECT, GOOD, OK, MISS)
/// Responsibility: Show text with color, fade out after duration
/// </summary>
public class JudgementDisplay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI _judgementText;

    [Header("Settings")]
    [SerializeField] private Vector3 _startScale = Vector3.one * 1.5f;
    [SerializeField] private float _scaleInDuration = 0.1f;

    private Sequence _displaySequence;

    // ═══════════════════════════════════════════════════════════
    // PUBLIC API
    // ═══════════════════════════════════════════════════════════

    public void Show(FeedbackData feedback)
    {
        // ✅ DEBUG: Check if method is called
        Debug.Log($"[JudgementDisplay] Show() called!");

        if (feedback == null)
        {
            Debug.LogError("[JudgementDisplay] FeedbackData is NULL!");
            return;
        }

        if (_judgementText == null)
        {
            Debug.LogError("[JudgementDisplay] _judgementText is NULL! Assign TextMeshProUGUI in Inspector!");
            return;
        }

        // ✅ DEBUG: Log feedback data
        Debug.Log($"[JudgementDisplay] Text: {feedback.text}, Color: {feedback.color}, Duration: {feedback.displayDuration}");

        // Kill existing animation
        _displaySequence?.Kill();

        // Set text and color
        _judgementText.text = feedback.text;
        _judgementText.color = feedback.color;
        _judgementText.gameObject.SetActive(true);

        // ✅ DEBUG: Confirm text is active
        Debug.Log($"[JudgementDisplay] Text active: {_judgementText.gameObject.activeSelf}");

        // Reset transform
        _judgementText.transform.localScale = _startScale;

        // Create animation sequence
        _displaySequence = DOTween.Sequence();

        // Scale in
        _displaySequence.Append(_judgementText.transform.DOScale(Vector3.one, _scaleInDuration).SetEase(Ease.OutBack));

        // Fade out
        _displaySequence.Append(_judgementText.DOFade(0f, feedback.displayDuration).SetDelay(0.2f));

        // On complete
        _displaySequence.OnComplete(() =>
        {
            _judgementText.gameObject.SetActive(false);

            // Reset alpha
            Color c = feedback.color;
            c.a = 1f;
            _judgementText.color = c;

            Debug.Log("[JudgementDisplay] Animation complete");
        });
    }

    // ═══════════════════════════════════════════════════════════
    // CLEANUP
    // ═══════════════════════════════════════════════════════════

    private void OnDestroy()
    {
        _displaySequence?.Kill();
    }
}