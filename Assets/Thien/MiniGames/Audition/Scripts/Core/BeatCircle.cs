using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // ✅ ADD THIS
using System;

/// <summary>
/// SRP: Represent a single beat circle (Poolable)
/// Responsibility: Hold beat state, emit events, support pooling
/// </summary>
[RequireComponent(typeof(BeatAnimator))]
public class BeatCircle : MonoBehaviour, IPointerClickHandler // ✅ Implement interface
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

    private Action<BeatCircle> _onReturnToPool;

    // ═══════════════════════════════════════════════════════════
    // LIFECYCLE
    // ═══════════════════════════════════════════════════════════

    private void Awake()
    {
        _animator = GetComponent<BeatAnimator>();

        // ✅ Ensure outer ring can receive raycast
        if (_outerRing != null)
        {
            _outerRing.raycastTarget = true;
        }
    }

    public void Initialize(BeatConfig config, BeatData data, Action<BeatCircle> onReturnToPool)
    {
        _config = config;
        _data = data;
        _onReturnToPool = onReturnToPool;

        SetupVisuals();

        transform.localPosition = data.position;
        IsActive = true;
        _hasBeenHit = false;

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
        _outerRing.rectTransform.sizeDelta = Vector2.one * _config.outerRingSize;
        Color outerColor = _config.outerRingColor;
        outerColor.a = _config.ringAlpha;
        _outerRing.color = outerColor;

        _innerRing.rectTransform.sizeDelta = Vector2.one * _config.innerRingStartSize;
        Color innerColor = _config.innerRingColor;
        innerColor.a = _config.ringAlpha;
        _innerRing.color = innerColor;
    }

    // ═══════════════════════════════════════════════════════════
    // ✅ NEW: POINTER CLICK HANDLER (Unity Event System)
    // ═══════════════════════════════════════════════════════════

    public void OnPointerClick(PointerEventData eventData)
    {
        // Chỉ xử lý khi beat active
        if (!IsActive || _hasBeenHit) return;

        // ✅ Tap trúng vào beat này!
        OnTap();
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
        _animator.PlayHitAnimation(_innerRing, _outerRing, ReturnToPool);
    }

    public void PlayMissFeedback()
    {
        _animator.PlayMissAnimation(_innerRing, _outerRing, ReturnToPool);
    }

    // ═══════════════════════════════════════════════════════════
    // POOLING
    // ═══════════════════════════════════════════════════════════

    private void ReturnToPool()
    {
        OnTapped = null;
        OnMissed = null;

        IsActive = false;
        _hasBeenHit = false;

        _animator.StopShrink();

        _onReturnToPool?.Invoke(this);
    }

    private void OnAnimationComplete()
    {
        if (_hasBeenHit) return;

        IsActive = false;
        OnMissed?.Invoke(this);
    }
}