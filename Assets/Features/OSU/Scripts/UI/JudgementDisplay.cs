using UnityEngine;
using DG.Tweening;
using TMPro;

/// <summary>
/// SRP: Display judgement feedback using 3D TextMeshPro (MeshRenderer)
/// Responsibility: Show judgement text at beat positions (single reusable instance)
/// </summary>
public class JudgementDisplay : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private JudgementConfig _config;

    [Header("Text Reference")]
    [SerializeField] private TextMeshPro _judgementText;

    [Header("Animation Settings")]
    [SerializeField] private float _scaleInDuration = 0.15f;
    [SerializeField] private float _floatUpDistance = 1f;

    private Sequence _currentSequence;

    // ═══════════════════════════════════════════════════════════
    // INITIALIZATION
    // ═══════════════════════════════════════════════════════════

    private void Awake()
    {
        if (_judgementText == null) return;

        // Setup text properties from config
        _judgementText.fontSize = _config.textFontSize;
        _judgementText.alignment = TextAlignmentOptions.Center;

        // Setup sorting layer
        MeshRenderer renderer = _judgementText.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.sortingLayerName = _config.textSortingLayer;
            renderer.sortingOrder = _config.textSortingOrder;
        }

        // Keep active, hide via scale 0
        _judgementText.gameObject.SetActive(true);
        _judgementText.transform.localScale = Vector3.zero;
    }

    // ═══════════════════════════════════════════════════════════
    // PUBLIC API
    // ═══════════════════════════════════════════════════════════

    public void Show(FeedbackData feedback, Vector3 worldPosition)
    {
        if (feedback == null || _judgementText == null) return;

        // Kill existing animation
        _currentSequence?.Kill();

        // Setup text
        _judgementText.text = feedback.text;
        _judgementText.color = feedback.color;

        // Set position, start from startScale
        _judgementText.transform.position = worldPosition;
        _judgementText.transform.localScale = Vector3.one * _config.textStartScale;

        // Animate
        PlayAnimation(feedback, worldPosition);
    }

    public void Show(FeedbackData feedback)
    {
        Show(feedback, Vector3.zero);
    }

    // ═══════════════════════════════════════════════════════════
    // ANIMATION
    // ═══════════════════════════════════════════════════════════

    private void PlayAnimation(FeedbackData feedback, Vector3 startPos)
    {
        _currentSequence = DOTween.Sequence();

        // Scale up to show scale
        Vector3 showScale = Vector3.one * _config.textShowScale;
        _currentSequence.Append(_judgementText.transform.DOScale(showScale, _scaleInDuration).SetEase(Ease.OutBack));

        // Float up + fade out
        Vector3 endPos = startPos + Vector3.up * _floatUpDistance;
        _currentSequence.Append(_judgementText.transform.DOMove(endPos, feedback.displayDuration).SetEase(Ease.OutQuad));
        _currentSequence.Join(_judgementText.DOFade(0f, feedback.displayDuration));

        // Hide via scale 0 on complete
        _currentSequence.OnComplete(() =>
        {
            _judgementText.transform.localScale = Vector3.zero;

            // Reset alpha
            Color c = _judgementText.color;
            c.a = 1f;
            _judgementText.color = c;
        });
    }

    // ═══════════════════════════════════════════════════════════
    // CLEANUP
    // ═══════════════════════════════════════════════════════════

    private void OnDestroy()
    {
        _currentSequence?.Kill();
    }
}