using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

/// <summary>
/// SRP: Represent a single beat circle using SpriteRenderer (Poolable)
/// Responsibility: Hold beat state, emit events, support pooling
/// </summary>
[RequireComponent(typeof(BeatAnimator))]
[RequireComponent(typeof(CircleCollider2D))]
public class BeatCircle : MonoBehaviour
{
    // Static counter for dynamic sorting order
    private static int _spawnCounter = 0;
    private const int SORTING_ORDER_OFFSET = 10; // Offset between each beat

    [Header("References")]
    [SerializeField] private SpriteRenderer _outerRing; // Vòng thu vào (shrinking)
    [SerializeField] private SpriteRenderer _innerRing; // Vòng cố định (target)
    [SerializeField] private SpriteRenderer _hitEffectSprite;
    private CircleCollider2D _collider;

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
    private int _sortingOrderBase; // Base sorting order for this beat
    private CancellationTokenSource _cts; // For cancelling async operations

    public bool IsActive { get; private set; }
    public Vector2 CurrentSize => _outerRing.transform.localScale;
    public Vector2 TargetSize => _innerRing.transform.localScale;

    public event Action<BeatCircle> OnTapped;
    public event Action<BeatCircle> OnMissed;

    private Action<BeatCircle> _onReturnToPool;

    // ═══════════════════════════════════════════════════════════
    // LIFECYCLE
    // ═══════════════════════════════════════════════════════════

    private void Awake()
    {
        _animator = GetComponent<BeatAnimator>();
        _collider = GetComponent<CircleCollider2D>();

        // Setup collider
        if (_collider == null)
        {
            _collider = gameObject.AddComponent<CircleCollider2D>();
        }
        _collider.isTrigger = false; // We want physics raycasts to hit this

        // Set proper layer order
        SetupLayerOrder();

        // Hide hit effect initially
        if (_hitEffectSprite != null)
        {
            _hitEffectSprite.gameObject.SetActive(false);
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

        // Create new cancellation token source
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        // Enable collider for input
        _collider.enabled = true;

        // Ensure GameObject is active
        gameObject.SetActive(true);

        // Set position
        transform.localPosition = data.position;

        // Setup visuals (this sets initial scales and sprites)
        SetupVisuals();

        // Start shrink animation on outer ring
        StartShrinkAnimation();

        if (_debugMode)
        {
            Debug.Log($"[BeatCircle] Initialized at {data.position}");
            Debug.Log($"[BeatCircle] BeatData.size (world): {data.size}");
            Debug.Log($"[BeatCircle] BeatConfig - beatSizePixels: {_config.beatSizePixels}, innerScale: {_config.innerRingScale}, outerScale: {_config.outerRingStartScale}");
            Debug.Log($"[BeatCircle] Computed - Inner world scale: {_config.InnerRingWorldScale}, Outer world scale: {_config.OuterRingStartWorldScale}");
            Debug.Log($"[BeatCircle] Transform - Inner scale: {_innerRing.transform.localScale}, Outer scale: {_outerRing.transform.localScale}");
        }
    }

    // ═══════════════════════════════════════════════════════════
    // LAYER ORDER SETUP
    // ═══════════════════════════════════════════════════════════

    private void SetupLayerOrder()
    {
        // Tính toán sorting order dựa trên spawn counter
        _sortingOrderBase = _spawnCounter * SORTING_ORDER_OFFSET;
        _spawnCounter++;

        if (_spawnCounter > 1000)
        {
            _spawnCounter = 0;
        }

        // Tên Sorting Layer bạn đã tạo trong Unity (Ví dụ: "Gameplay")
        string targetLayer = "Gameplay";

        // Outer ring ở dưới (shrinking), Inner ring ở trên (target, visible)
        SetSpriteSorting(_outerRing, targetLayer, _sortingOrderBase + 1);
        SetSpriteSorting(_innerRing, targetLayer, _sortingOrderBase + 2);
        SetSpriteSorting(_hitEffectSprite, targetLayer, _sortingOrderBase + 3);

        if (_debugMode)
        {
            Debug.Log($"[BeatCircle] Sorting Setup - Layer: {targetLayer}, Base: {_sortingOrderBase}");
        }
    }

    /// <summary>
    /// Hàm phụ trợ để gán Sorting Layer và Order một cách an toàn
    /// </summary>
    private void SetSpriteSorting(SpriteRenderer sr, string layerName, int order)
    {
        if (sr != null)
        {
            sr.sortingLayerName = layerName;
            sr.sortingOrder = order;
        }
    }

    // ═══════════════════════════════════════════════════════════
    // VISUAL SETUP - ALL DATA MANIPULATION HAPPENS HERE
    // ═══════════════════════════════════════════════════════════

    private void SetupVisuals()
    {
        BeatSpriteSet spriteSet = _data.spriteSet;

        SetupInnerRing(spriteSet);
        SetupOuterRing(spriteSet);
    }

    private void SetupInnerRing(BeatSpriteSet spriteSet)
    {
        // ✅ Use auto-converted world scale from config
        float worldScale = _config.InnerRingWorldScale;
        _innerRing.transform.localScale = Vector3.one * worldScale;

        // Set sprite
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

        // Update collider radius based on inner ring size (target area)
        _collider.radius = worldScale * 1.5f;

        if (_debugMode)
        {
            Debug.Log($"[BeatCircle] Inner Ring - World Scale: {worldScale}");
        }
    }

    private void SetupOuterRing(BeatSpriteSet spriteSet)
    {
        // ✅ Use auto-converted world scale from config
        float worldScale = _config.OuterRingStartWorldScale;
        _outerRing.transform.localScale = Vector3.one * worldScale;

        // Set sprite
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

        if (_debugMode)
        {
            Debug.Log($"[BeatCircle] Outer Ring - World Scale: {worldScale}");
        }
    }

    // ═══════════════════════════════════════════════════════════
    // ANIMATION CONTROL
    // ═══════════════════════════════════════════════════════════

    private void StartShrinkAnimation()
    {
        // ✅ BeatCircle controls the data, BeatAnimator just animates
        float targetScale = _innerRing.transform.localScale.x;
        _animator.StartShrink(_outerRing, targetScale, _config.shrinkDuration, OnAnimationComplete);
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

        // Disable collider to prevent further clicks
        _collider.enabled = false;

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

        ShowHitEffectSpriteAsync().Forget();
        _animator.PlayHitAnimation(_outerRing, _innerRing, ReturnToPool);
    }

    public void PlayMissFeedback()
    {
        if (_debugMode)
        {
            Debug.Log($"[BeatCircle] PlayMissFeedback");
        }

        _animator.PlayMissAnimation(_outerRing, _innerRing, ReturnToPool);
    }

    // ═══════════════════════════════════════════════════════════
    // HIT EFFECT SPRITE - UNITASK VERSION
    // ═══════════════════════════════════════════════════════════

    private async UniTaskVoid ShowHitEffectSpriteAsync()
    {
        if (_hitEffectSprite == null) return;

        BeatSpriteSet spriteSet = _data.spriteSet;

        if (spriteSet != null && spriteSet.hitEffectSprite != null)
        {
            _hitEffectSprite.sprite = spriteSet.hitEffectSprite;
            // ✅ Hit effect is 120% of inner ring world scale
            float scale = _config.InnerRingWorldScale * 1.2f;
            _hitEffectSprite.transform.localScale = Vector3.one * scale;
            _hitEffectSprite.color = Color.white;
            _hitEffectSprite.gameObject.SetActive(true);

            try
            {
                // Use cancellation token to allow cleanup
                await UniTask.WaitForSeconds(0.2f, cancellationToken: _cts.Token);

                if (_hitEffectSprite != null)
                {
                    _hitEffectSprite.gameObject.SetActive(false);
                }
            }
            catch (OperationCanceledException)
            {
                // Task was cancelled during cleanup - this is expected behavior
                if (_debugMode)
                {
                    Debug.Log($"[BeatCircle] Hit effect fade cancelled");
                }
            }
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

        // Cancel all async operations
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;

        // Stop animations
        _animator.StopShrink();

        // Reset states
        IsActive = false;
        _hasBeenHit = false;

        // Hide hit effect
        if (_hitEffectSprite != null)
        {
            _hitEffectSprite.gameObject.SetActive(false);
        }

        // Disable collider
        _collider.enabled = false;

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

        // Disable collider
        _collider.enabled = false;

        // Invoke miss event
        OnMissed?.Invoke(this);
    }

    private void OnDestroy()
    {
        // Cleanup cancellation token on destroy
        _cts?.Cancel();
        _cts?.Dispose();
    }
}