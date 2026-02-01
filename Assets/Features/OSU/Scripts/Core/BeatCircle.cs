using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// SRP: Represent a single beat circle (Poolable)
/// Responsibility: Hold beat state, emit events, support pooling
/// ✅ FIXED: Outer ring raycast ALWAYS enabled
/// </summary>
[RequireComponent(typeof(BeatAnimator))]
[RequireComponent(typeof(CanvasGroup))]
public class BeatCircle : MonoBehaviour, IPointerClickHandler, IPointerDownHandler
{
    [Header("References")]
    [SerializeField] private Image _outerRing;
    [SerializeField] private Image _innerRing;
    [SerializeField] private Image _hitEffectImage;
    [SerializeField] private CanvasGroup _canvasGroup;

    [Header("Default Sprites (Fallback)")]
    [Tooltip("Fallback sprite nếu không có BeatSpriteSet")]
    [SerializeField] private Sprite _defaultOuterSprite;
    [SerializeField] private Sprite _defaultInnerSprite;

    [Header("Debug")]
    [SerializeField] private bool _debugMode = false;

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

        // Get or add CanvasGroup
        if (_canvasGroup == null)
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        // ✅ FIX: Setup raycast targets ONCE - outer ring ALWAYS enabled
        SetupRaycastTargetsOnce();

        // Set proper layer order
        SetupLayerOrder();

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

        // Reset ALL states
        IsActive = true;
        _hasBeenHit = false;

        // ✅ FIX: Control interaction via CanvasGroup ONLY
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;

        // Ensure GameObject is active
        gameObject.SetActive(true);

        // Setup visuals
        SetupVisuals();

        transform.localPosition = data.position;

        // Calculate start and target sizes
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

        if (_debugMode)
        {
            Debug.Log($"[BeatCircle] Initialized at {data.position}, size: {beatSize}");
        }
    }

    // ═══════════════════════════════════════════════════════════
    // ✅ FIX: SETUP RAYCAST TARGETS ONCE (PERMANENT)
    // ═══════════════════════════════════════════════════════════

    private void SetupRaycastTargetsOnce()
    {
        // ✅ CRITICAL: Outer ring ALWAYS enabled for raycasting
        if (_outerRing != null)
        {
            _outerRing.raycastTarget = true; // NEVER change this!
        }

        // ✅ CRITICAL: Inner ring ALWAYS disabled (prevent blocking outer ring)
        if (_innerRing != null)
        {
            _innerRing.raycastTarget = false; // NEVER change this!
        }

        // ✅ Hit effect ALWAYS disabled
        if (_hitEffectImage != null)
        {
            _hitEffectImage.raycastTarget = false; // NEVER change this!
        }
    }

    // ═══════════════════════════════════════════════════════════
    // LAYER ORDER SETUP
    // ═══════════════════════════════════════════════════════════

    private void SetupLayerOrder()
    {
        // Outer ring phải ở TRÊN inner ring
        if (_outerRing != null)
        {
            _outerRing.transform.SetAsLastSibling();
        }

        if (_innerRing != null)
        {
            _innerRing.transform.SetAsFirstSibling();
        }

        if (_hitEffectImage != null)
        {
            _hitEffectImage.transform.SetAsLastSibling();
        }
    }

    // ═══════════════════════════════════════════════════════════
    // VISUAL SETUP
    // ═══════════════════════════════════════════════════════════

    private void SetupVisuals()
    {
        Vector2 beatSize = _data.size;
        BeatSpriteSet spriteSet = _data.spriteSet;

        SetupOuterRing(beatSize, spriteSet);
        SetupInnerRing(beatSize, spriteSet);
    }

    private void SetupOuterRing(Vector2 beatSize, BeatSpriteSet spriteSet)
    {
        _outerRing.rectTransform.sizeDelta = beatSize;

        if (spriteSet != null && spriteSet.outerRingSprite != null)
        {
            _outerRing.sprite = spriteSet.outerRingSprite;
            _outerRing.color = Color.white;
        }
        else
        {
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
        float sizeRatio = _config.innerRingStartSize / _config.outerRingSize;
        _innerRing.rectTransform.sizeDelta = beatSize * sizeRatio;

        if (spriteSet != null && spriteSet.innerRingSprite != null)
        {
            _innerRing.sprite = spriteSet.innerRingSprite;
            _innerRing.color = Color.white;
        }
        else
        {
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
    // ✅ INPUT HANDLING
    // ═══════════════════════════════════════════════════════════

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_debugMode)
        {
            Debug.Log($"[BeatCircle] OnPointerClick - IsActive={IsActive}, HasBeenHit={_hasBeenHit}, CanvasGroup.interactable={_canvasGroup.interactable}");
        }

        if (!IsActive || _hasBeenHit)
        {
            return;
        }

        OnTap();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_debugMode)
        {
            Debug.Log($"[BeatCircle] OnPointerDown - IsActive={IsActive}, HasBeenHit={_hasBeenHit}, CanvasGroup.interactable={_canvasGroup.interactable}");
        }

        if (!IsActive || _hasBeenHit)
        {
            return;
        }

        OnTap();
    }

    // ═══════════════════════════════════════════════════════════
    // PUBLIC API
    // ═══════════════════════════════════════════════════════════

    public void OnTap()
    {
        if (!IsActive || _hasBeenHit) return;

        if (_debugMode)
        {
            Debug.Log($"[BeatCircle] OnTap SUCCESS at {transform.localPosition}");
        }

        // Set flags FIRST
        _hasBeenHit = true;
        IsActive = false;

        // Stop animation
        _animator.StopShrink();

        // ✅ FIX: Disable interaction via CanvasGroup (NOT raycastTarget!)
        if (_canvasGroup != null)
        {
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false; // Prevent further clicks
        }

        // Invoke event (GameLoop will call PlayHitFeedback)
        OnTapped?.Invoke(this);
    }

    public void PlayHitFeedback()
    {
        if (!_hasBeenHit) return;

        if (_debugMode)
        {
            Debug.Log($"[BeatCircle] PlayHitFeedback");
        }

        ShowHitEffectSprite();
        _animator.PlayHitAnimation(_innerRing, _outerRing, ReturnToPool);
    }

    public void PlayMissFeedback()
    {
        if (_debugMode)
        {
            Debug.Log($"[BeatCircle] PlayMissFeedback");
        }

        _animator.PlayMissAnimation(_innerRing, _outerRing, ReturnToPool);
    }

    // ═══════════════════════════════════════════════════════════
    // HIT EFFECT SPRITE
    // ═══════════════════════════════════════════════════════════

    private void ShowHitEffectSprite()
    {
        if (_hitEffectImage == null) return;

        BeatSpriteSet spriteSet = _data.spriteSet;

        if (spriteSet != null && spriteSet.hitEffectSprite != null)
        {
            _hitEffectImage.sprite = spriteSet.hitEffectSprite;
            _hitEffectImage.rectTransform.sizeDelta = _data.size * 1.2f;
            _hitEffectImage.color = Color.white;
            _hitEffectImage.gameObject.SetActive(true);

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
    // ✅ COMPLETE CLEANUP
    // ═══════════════════════════════════════════════════════════

    private void ReturnToPool()
    {
        if (_debugMode)
        {
            Debug.Log($"[BeatCircle] Returning to pool");
        }

        // Stop all coroutines first
        StopAllCoroutines();

        // Stop animations
        _animator.StopShrink();

        // Reset states
        IsActive = false;
        _hasBeenHit = false;

        // Hide hit effect
        if (_hitEffectImage != null)
        {
            _hitEffectImage.gameObject.SetActive(false);
        }

        // ✅ FIX: Disable interaction via CanvasGroup (NOT raycastTarget!)
        if (_canvasGroup != null)
        {
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        // Invoke callback to return to pool
        var callback = _onReturnToPool;
        if (callback != null)
        {
            callback.Invoke(this);
        }

        // Clear callback after invoke
        _onReturnToPool = null;

        // Clear events after pool release
        OnTapped = null;
        OnMissed = null;
    }

    private void OnAnimationComplete()
    {
        if (_hasBeenHit) return;

        if (_debugMode)
        {
            Debug.Log($"[BeatCircle] Animation complete - MISS");
        }

        // Beat missed - disable immediately
        IsActive = false;

        // ✅ FIX: Disable interaction via CanvasGroup
        if (_canvasGroup != null)
        {
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        // Invoke miss event
        OnMissed?.Invoke(this);
    }
}