using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// SRP: Represent a single beat circle (Poolable)
/// Responsibility: Hold beat state, emit events, support pooling
/// ✅ NOW: Supports swapping sprites based on BeatSpriteSet
/// </summary>
[RequireComponent(typeof(BeatAnimator))]
public class BeatCircle : MonoBehaviour, IPointerClickHandler
{
    [Header("References")]
    [SerializeField] private Image _outerRing;
    [SerializeField] private Image _innerRing;
    [SerializeField] private Image _hitEffectImage;

    [Header("Default Sprites (Fallback)")]
    [Tooltip("Fallback sprite nếu không có BeatSpriteSet")]
    [SerializeField] private Sprite _defaultOuterSprite;
    [SerializeField] private Sprite _defaultInnerSprite;

    private BeatConfig _config;
    private BeatData _data;
    private BeatAnimator _animator;
    private bool _hasBeenHit = false;

    public bool IsActive { get; private set; }
    public Vector2 CurrentSize => _innerRing.rectTransform.sizeDelta;
    public Vector2 TargetSize => _data.size;

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

        // Hide hit effect image initially
        if (_hitEffectImage != null)
        {
            _hitEffectImage.gameObject.SetActive(false);
        }
    }

    public void Initialize(BeatConfig config, BeatData data, Action<BeatCircle> onReturnToPool)
    {
        _config = config;
        _data = data;
        _onReturnToPool = onReturnToPool;

        // ✅ Setup visuals (swap sprites based on BeatSpriteSet)
        SetupVisuals();

        transform.localPosition = data.position;
        IsActive = true;
        _hasBeenHit = false;

        // Calculate start and target sizes based on beat size
        Vector2 beatSize = _data.size;
        float sizeRatio = config.innerRingStartSize / config.outerRingSize;
        Vector2 innerStartSize = beatSize * sizeRatio;
        Vector2 targetSize = beatSize;

        _animator.StartShrink(
            _innerRing,
            innerStartSize,
            targetSize,
            config.shrinkDuration,
            OnAnimationComplete
        );
    }

    // ═══════════════════════════════════════════════════════════
    // ✅ VISUAL SETUP - "THAY ÁO" CHO BEAT
    // ═══════════════════════════════════════════════════════════

    private void SetupVisuals()
    {
        Vector2 beatSize = _data.size;
        BeatSpriteSet spriteSet = _data.spriteSet;

        // ✅ Setup Outer Ring
        SetupOuterRing(beatSize, spriteSet);

        // ✅ Setup Inner Ring
        SetupInnerRing(beatSize, spriteSet);
    }

    private void SetupOuterRing(Vector2 beatSize, BeatSpriteSet spriteSet)
    {
        // Set size
        _outerRing.rectTransform.sizeDelta = beatSize;

        // ✅ Swap sprite nếu có custom sprite set
        if (spriteSet != null && spriteSet.outerRingSprite != null)
        {
            _outerRing.sprite = spriteSet.outerRingSprite;
            _outerRing.color = Color.white; // Keep original sprite color
        }
        else
        {
            // Fallback to default sprite + color
            if (_defaultOuterSprite != null)
            {
                _outerRing.sprite = _defaultOuterSprite;
            }

            Color outerColor = _config.outerRingColor;
            outerColor.a = _config.ringAlpha;
            _outerRing.color = outerColor;
        }
    }

    private void SetupInnerRing(Vector2 beatSize, BeatSpriteSet spriteSet)
    {
        // Set size (will shrink during animation)
        float sizeRatio = _config.innerRingStartSize / _config.outerRingSize;
        _innerRing.rectTransform.sizeDelta = beatSize * sizeRatio;

        // ✅ Swap sprite nếu có custom sprite set
        if (spriteSet != null && spriteSet.innerRingSprite != null)
        {
            _innerRing.sprite = spriteSet.innerRingSprite;
            _innerRing.color = Color.white; // Keep original sprite color
        }
        else
        {
            // Fallback to default sprite + color
            if (_defaultInnerSprite != null)
            {
                _innerRing.sprite = _defaultInnerSprite;
            }

            Color innerColor = _config.innerRingColor;
            innerColor.a = _config.ringAlpha;
            _innerRing.color = innerColor;
        }
    }

    // ═══════════════════════════════════════════════════════════
    // POINTER CLICK HANDLER
    // ═══════════════════════════════════════════════════════════

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!IsActive || _hasBeenHit) return;
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
        // ✅ Show hit effect sprite if available
        ShowHitEffectSprite();

        _animator.PlayHitAnimation(_innerRing, _outerRing, ReturnToPool);
    }

    public void PlayMissFeedback()
    {
        _animator.PlayMissAnimation(_innerRing, _outerRing, ReturnToPool);
    }

    // ═══════════════════════════════════════════════════════════
    // ✅ HIT EFFECT SPRITE
    // ═══════════════════════════════════════════════════════════

    private void ShowHitEffectSprite()
    {
        if (_hitEffectImage == null) return;

        BeatSpriteSet spriteSet = _data.spriteSet;

        // Check if custom hit effect sprite exists
        if (spriteSet != null && spriteSet.hitEffectSprite != null)
        {
            _hitEffectImage.sprite = spriteSet.hitEffectSprite;
            _hitEffectImage.rectTransform.sizeDelta = _data.size * 1.2f; // Slightly larger
            _hitEffectImage.color = Color.white;
            _hitEffectImage.gameObject.SetActive(true);

            // Fade out after short delay
            StartCoroutine(FadeOutHitEffect());
        }
    }

    private System.Collections.IEnumerator FadeOutHitEffect()
    {
        yield return new WaitForSeconds(0.2f);

        if (_hitEffectImage != null)
        {
            _hitEffectImage.gameObject.SetActive(false);
        }
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

        // Hide hit effect
        if (_hitEffectImage != null)
        {
            _hitEffectImage.gameObject.SetActive(false);
        }

        _onReturnToPool?.Invoke(this);
    }

    private void OnAnimationComplete()
    {
        if (_hasBeenHit) return;

        IsActive = false;
        OnMissed?.Invoke(this);
    }
}