using DG.Tweening;
using TMPro;
using UnityEngine;

public class CorrectionUIManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI _progressText;

    [Header("Value Animation")]
    [SerializeField] private float _valueAnimationDuration = 0.5f;

    [Header("Completion Animation")]
    [SerializeField] private float _punchScale = 0.5f;
    [SerializeField] private float _punchDuration = 0.5f;
    [SerializeField] private float _hideDuration = 0.3f;
    [SerializeField] private float _hideDelay = 0.5f;

    [Header("Entrance Animation")]
    [SerializeField] private float _entranceDuration = 0.5f;
    [SerializeField] private Ease _entranceEase = Ease.OutBack;

    private CorrectionLogicHandler _handler;
    private float _currentDisplayValue;
    private Tween _valueTween;
    private Sequence _animationSequence;

    /// <summary>
    /// Bind UI to the Logic Handler events
    /// </summary>
    public void Bind(CorrectionLogicHandler handler)
    {
        // Unsubscribe from old handler if exists
        if (_handler != null)
        {
            _handler.OnProgressChanged -= UpdateProgress;
            _handler.OnCorrectionComplete -= HandleComplete;
        }

        _handler = handler;

        if (_handler != null)
        {
            _handler.OnProgressChanged += UpdateProgress;
            _handler.OnCorrectionComplete += HandleComplete;

            // Reset state
            ResetUI();

            // Animate Entrance
            AnimateEntrance();
        }
    }

    private void ResetUI()
    {
        if (_animationSequence != null && _animationSequence.IsActive())
            _animationSequence.Kill();

        if (_valueTween != null && _valueTween.IsActive())
            _valueTween.Kill();

        // Reset Text
        if (_progressText != null)
        {
            // Initial value
            float initialProgress = _handler != null ? _handler.GetProgress() * 100f : 0f;
            _currentDisplayValue = initialProgress;
            UpdateText(_currentDisplayValue);

            // Create fresh state for entrance
            _progressText.transform.localScale = Vector3.zero;
            _progressText.alpha = 1f;
            _progressText.gameObject.SetActive(true);
        }
    }

    private void AnimateEntrance()
    {
        if (_progressText == null) return;

        _progressText.transform.DOScale(1f, _entranceDuration).SetEase(_entranceEase);
    }

    private void UpdateProgress(float progress)
    {
        float targetValue = progress * 100f;

        // Kill previous value tween
        if (_valueTween != null && _valueTween.IsActive())
        {
            _valueTween.Kill();
        }

        // DOTween to animate the value
        _valueTween = DOTween.To(() => _currentDisplayValue, x =>
        {
            _currentDisplayValue = x;
            UpdateText(_currentDisplayValue);
        }, targetValue, _valueAnimationDuration)
        .SetEase(Ease.OutQuad);
    }

    private void HandleComplete()
    {
        // Cancel value tween
        if (_valueTween != null && _valueTween.IsActive()) _valueTween.Kill();

        // 1. Force 100%
        _currentDisplayValue = 100f;
        UpdateText(100f);

        // 2. Play Celebration Sequence
        if (_progressText != null)
        {
            _animationSequence = DOTween.Sequence();

            // Pulse/Bounce
            _animationSequence.Append(_progressText.transform.DOPunchScale(Vector3.one * _punchScale, _punchDuration, 10, 1f));

            // Wait a bit
            _animationSequence.AppendInterval(_hideDelay);

            // Hide/Scale Down
            _animationSequence.Append(_progressText.transform.DOScale(0f, _hideDuration).SetEase(Ease.InBack));
        }
    }

    private void UpdateText(float value)
    {
        if (_progressText != null)
        {
            _progressText.text = $"{value:F0}%";
        }
    }

    private void OnDestroy()
    {
        if (_handler != null)
        {
            _handler.OnProgressChanged -= UpdateProgress;
            _handler.OnCorrectionComplete -= HandleComplete;
        }

        if (_valueTween != null && _valueTween.IsActive()) _valueTween.Kill();
        if (_animationSequence != null && _animationSequence.IsActive()) _animationSequence.Kill();
    }
}
