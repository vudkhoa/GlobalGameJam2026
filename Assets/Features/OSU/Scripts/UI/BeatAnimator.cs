using System;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// SRP: Handle beat visual animations for SpriteRenderer
/// Responsibility: ONLY animate sprites using DOTween - NO data manipulation
/// </summary>
public class BeatAnimator : MonoBehaviour
{
    private Tweener _shrinkTweener;

    // ═══════════════════════════════════════════════════════════
    // SHRINK ANIMATION
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Start shrinking animation on a sprite renderer
    /// Assumes the ring already has its start scale set
    /// </summary>
    public void StartShrink(SpriteRenderer ring, float targetScale, float duration, Action onComplete)
    {
        // Kill existing tween if any
        _shrinkTweener?.Kill();

        // Animate from current scale to target scale
        _shrinkTweener = ring.transform
            .DOScale(targetScale, duration)
            .SetEase(Ease.Linear)
            .OnComplete(() => onComplete?.Invoke());
    }

    /// <summary>
    /// Stop the shrink animation immediately
    /// </summary>
    public void StopShrink()
    {
        _shrinkTweener?.Kill();
    }

    // ═══════════════════════════════════════════════════════════
    // FEEDBACK ANIMATIONS
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Play hit animation - scale punch + fade out both rings
    /// </summary>
    public void PlayHitAnimation(SpriteRenderer outerRing, SpriteRenderer innerRing, Action onComplete)
    {
        Sequence seq = DOTween.Sequence();

        // Get current scales
        Vector3 outerScale = outerRing.transform.localScale;
        Vector3 innerScale = innerRing.transform.localScale;

        // Scale punch on outer ring (the one that was shrinking)
        seq.Append(outerRing.transform.DOScale(outerScale * 1.2f, 0.1f));
        seq.Append(outerRing.transform.DOScale(outerScale, 0.1f));

        // Fade out both rings
        seq.Join(outerRing.DOFade(0f, 0.3f));
        seq.Join(innerRing.DOFade(0f, 0.3f));

        seq.OnComplete(() => onComplete?.Invoke());
    }

    /// <summary>
    /// Play miss animation - fade out slower (no punch effect)
    /// </summary>
    public void PlayMissAnimation(SpriteRenderer outerRing, SpriteRenderer innerRing, Action onComplete)
    {
        Sequence seq = DOTween.Sequence();

        // Fade out slower for miss
        seq.Append(outerRing.DOFade(0f, 0.4f));
        seq.Join(innerRing.DOFade(0f, 0.4f));

        seq.OnComplete(() => onComplete?.Invoke());
    }

    // ═══════════════════════════════════════════════════════════
    // CLEANUP
    // ═══════════════════════════════════════════════════════════

    private void OnDestroy()
    {
        _shrinkTweener?.Kill();
        DOTween.Kill(transform); // Kill all tweens on this transform
    }
}