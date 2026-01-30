using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

/// <summary>
/// SRP: Handle beat visual animations
/// Responsibility: Animate rings using DOTween
/// </summary>
public class BeatAnimator : MonoBehaviour
{
    private Tweener _shrinkTweener;

    // ═══════════════════════════════════════════════════════════
    // PUBLIC API
    // ═══════════════════════════════════════════════════════════

    public void StartShrink(Image ring, float startSize, float targetSize, float duration, Action onComplete)
    {
        ring.rectTransform.sizeDelta = Vector2.one * startSize;

        _shrinkTweener = ring.rectTransform
            .DOSizeDelta(Vector2.one * targetSize, duration)
            .SetEase(Ease.InOutCubic)
            .OnComplete(() => onComplete?.Invoke());
    }

    public void StopShrink()
    {
        _shrinkTweener?.Kill();
    }

    public void PlayHitAnimation(Image innerRing, Image outerRing, Action onComplete)
    {
        Sequence seq = DOTween.Sequence();

        // Scale punch
        seq.Append(innerRing.transform.DOScale(1.2f, 0.1f));
        seq.Append(innerRing.transform.DOScale(1f, 0.1f));

        // Fade out
        seq.Join(innerRing.DOFade(0f, 0.3f));
        seq.Join(outerRing.DOFade(0f, 0.3f));

        seq.OnComplete(() => onComplete?.Invoke());
    }

    public void PlayMissAnimation(Image innerRing, Image outerRing, Action onComplete)
    {
        Sequence seq = DOTween.Sequence();

        // Fade out slower
        seq.Append(innerRing.DOFade(0f, 0.4f));
        seq.Join(outerRing.DOFade(0f, 0.4f));

        seq.OnComplete(() => onComplete?.Invoke());
    }

    private void OnDestroy()
    {
        _shrinkTweener?.Kill();
    }
}