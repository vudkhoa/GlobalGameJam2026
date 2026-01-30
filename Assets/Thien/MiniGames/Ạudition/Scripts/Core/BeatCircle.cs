using System;
using UnityEngine;

using UnityEngine.UI;

/// <summary>
/// SRP: Represent a single beat circle (Poolable)
/// Responsibility: Hold beat state, emit events, support pooling
/// </summary>
[RequireComponent(typeof(BeatAnimator))]
public class BeatCircle : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image _outerRing;
    [SerializeField] private Image _innerRing;

    private BeatConfig _config;
    private BeatData _data;
    private BeatAnimator _animator;
    private bool _hasBeenHit = false;

    public bool IsActive { get; private set; }
    public float CurrentSize => _innerRing.rectTransform.sizeDelta.x;
    public float TargetSize => _config.outerRingSize;

    public event Action<BeatCircle> OnTapped;
    public event Action<BeatCircle> OnMissed;

    // ✅ NEW: Pool callback
    private Action<BeatCircle> _onReturnToPool;

    // ═══════════════════════════════════════════════════════════
    // LIFECYCLE
    // ═══════════════════════════════════════════════════════════

    private void Awake()
    {
        _animator = GetComponent<BeatAnimator>();
    }

    // ✅ NEW: Initialize with pool callback
    public void Initialize(BeatConfig config, BeatData data, Action<BeatCircle> onReturnToPool)
    {
        _config = config;
        _data = data;
        _onReturnToPool = onReturnToPool;

        SetupVisuals();

        transform.localPosition = data.position;
        IsActive = true;
        _hasBeenHit = false;

        // Start shrink animation
        _animator.StartShrink(
            _innerRing,
            config.innerRingStartSize,
            config.outerRingSize,
            config.shrinkDuration,
            OnAnimationComplete
        );
    }

    private void SetupVisuals()
    {
        // Outer ring (target, static)
        _outerRing.rectTransform.sizeDelta = Vector2.one * _config.outerRingSize;
        Color outerColor = _config.outerRingColor;
        outerColor.a = _config.ringAlpha;
        _outerRing.color = outerColor;

        // Inner ring (shrinking)
        _innerRing.rectTransform.sizeDelta = Vector2.one * _config.innerRingStartSize;
        Color innerColor = _config.innerRingColor;
        innerColor.a = _config.ringAlpha;
        _innerRing.color = innerColor;
    }

    // ═══════════════════════════════════════════════════════════
    // PUBLIC API
    // ═══════════════════════════════════════════════════════════

    public void OnTap()
    {
        if (!IsActive || _hasBeenHit) return;

        _hasBeenHit = true;
        IsActive = false;

        _animator.StopShrink();
        OnTapped?.Invoke(this);
    }

    public void PlayHitFeedback()
    {
        // ✅ CHANGED: Return to pool instead of Destroy
        _animator.PlayHitAnimation(_innerRing, _outerRing, ReturnToPool);
    }

    public void PlayMissFeedback()
    {
        // ✅ CHANGED: Return to pool instead of Destroy
        _animator.PlayMissAnimation(_innerRing, _outerRing, ReturnToPool);
    }

    // ═══════════════════════════════════════════════════════════
    // ✅ NEW: POOLING
    // ═══════════════════════════════════════════════════════════

    private void ReturnToPool()
    {
        // Cleanup events
        OnTapped = null;
        OnMissed = null;

        // Reset state
        IsActive = false;
        _hasBeenHit = false;

        // Stop any remaining animations
        _animator.StopShrink();

        // Return to pool
        _onReturnToPool?.Invoke(this);
    }

    // ═══════════════════════════════════════════════════════════
    // PRIVATE
    // ═══════════════════════════════════════════════════════════

    private void OnAnimationComplete()
    {
        if (_hasBeenHit) return;

        IsActive = false;
        OnMissed?.Invoke(this);
    }
}