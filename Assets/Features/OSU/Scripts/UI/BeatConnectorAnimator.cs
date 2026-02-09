using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Threading;
using UnityEngine;

/// <summary>
/// SRP: Handle beat connector animations
/// Responsibility: Animate connector line từ beat này sang beat khác
/// </summary>
public class BeatConnectorAnimator : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    private BeatConnectorConfig _config;

    private Vector2 _startPos;
    private Vector2 _endPos;
    private float _lineLength;

    private CancellationTokenSource _cts;

    // ✅ NEW: Thêm beat timing
    private float _beatShrinkDuration;
    private float _timingOffset = 0.1f; // Fade out sớm hơn beat miss 0.1s

    // ═══════════════════════════════════════════════════════════
    // LIFECYCLE
    // ═══════════════════════════════════════════════════════════

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// Initialize connector từ startPos → endPos
    /// </summary>
    public void Initialize(Vector2 startPos, Vector2 endPos, BeatConnectorConfig config, float beatShrinkDuration, Action onComplete = null)
    {
        _config = config;
        _startPos = startPos;
        _endPos = endPos;
        _beatShrinkDuration = beatShrinkDuration;

        // Cancel previous animation if exists
        Cleanup();

        _cts = new CancellationTokenSource();

        // Setup visual
        SetupVisuals();

        // Start fade animation
        PlayFadeAnimationAsync(onComplete).Forget();
    }

    // ═══════════════════════════════════════════════════════════
    // VISUAL SETUP
    // ═══════════════════════════════════════════════════════════

    private void SetupVisuals()
    {
        // Calculate line properties
        Vector2 direction = _endPos - _startPos;
        _lineLength = direction.magnitude;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Position ở điểm START (A), không phải tâm
        transform.position = _startPos;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // Setup sprite renderer
        _spriteRenderer.sprite = _config.connectorSprite;
        _spriteRenderer.color = _config.connectorColor;
        _spriteRenderer.drawMode = SpriteDrawMode.Tiled;
        _spriteRenderer.sortingLayerName = _config.sortingLayerName;
        _spriteRenderer.sortingOrder = _config.sortingOrder;

        // ✅ Giảm mật độ tile bằng cách tăng size của sprite
        // Hoặc giảm tilesPerUnit (nếu dùng)
        _spriteRenderer.tileMode = SpriteTileMode.Continuous;

        // Initially size = 0 (không hiển thị)
        _spriteRenderer.size = new Vector2(0, _config.lineWidth);

        // Initially invisible
        Color col = _spriteRenderer.color;
        col.a = 0f;
        _spriteRenderer.color = col;

        Debug.Log($"[BeatConnector] Setup - Start: {_startPos}, End: {_endPos}, Length: {_lineLength}, BeatDuration: {_beatShrinkDuration}s");
    }

    // ═══════════════════════════════════════════════════════════
    // FADE ANIMATION (A → B fade in, then A → B fade out)
    // ═══════════════════════════════════════════════════════════

    private async UniTaskVoid PlayFadeAnimationAsync(Action onComplete)
    {
        try
        {
            // Phase 1: Fade in from A → B (line vẽ từ A đến B)
            await FadeInAsync(_cts.Token);

            // Phase 2: Wait until near beat miss time
            // ✅ Tính toán thời gian chờ dựa trên beat shrink duration
            float fadeInDuration = _lineLength / _config.fadeInSpeed;
            float totalVisibleTime = _beatShrinkDuration - _timingOffset; // Fade out sớm hơn beat miss 0.1s
            float delayBeforeFadeOut = Mathf.Max(0, totalVisibleTime - fadeInDuration);

            if (delayBeforeFadeOut > 0)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(delayBeforeFadeOut), cancellationToken: _cts.Token);
            }

            // Phase 3: Fade out from A → B (line biến mất từ A về B)
            await FadeOutAsync(_cts.Token);

            // Complete callback
            onComplete?.Invoke();

            // Cleanup after animation complete
            Cleanup();
            Destroy(gameObject);
        }
        catch (OperationCanceledException)
        {
            // Animation was cancelled - cleanup already handled
        }
    }

    /// <summary>
    /// Fade in: Connector xuất hiện dần từ A → B (line vẽ từ trái sang phải)
    /// </summary>
    private async UniTask FadeInAsync(CancellationToken ct)
    {
        float duration = _lineLength / _config.fadeInSpeed;
        float elapsed = 0f;

        // Fade in entire line alpha first (instant)
        Color col = _spriteRenderer.color;
        col.a = _config.connectorColor.a;
        _spriteRenderer.color = col;

        // Animate size.x from 0 → full length (vẽ từ A → B)
        while (elapsed < duration)
        {
            ct.ThrowIfCancellationRequested();

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Size.x tăng dần = line vẽ từ A → B
            float currentLength = _lineLength * t;
            _spriteRenderer.size = new Vector2(currentLength, _config.lineWidth);

            await UniTask.Yield(PlayerLoopTiming.Update, ct);
        }

        // Ensure final state
        _spriteRenderer.size = new Vector2(_lineLength, _config.lineWidth);
    }

    /// <summary>
    /// Fade out: Connector biến mất dần từ A → B (đuôi line rút dần từ A về B)
    /// </summary>
    private async UniTask FadeOutAsync(CancellationToken ct)
    {
        float duration = _lineLength / _config.fadeOutSpeed;
        float elapsed = 0f;

        Vector2 direction = (_endPos - _startPos).normalized;

        while (elapsed < duration)
        {
            ct.ThrowIfCancellationRequested();

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Shrink from start (đuôi line rút dần)
            float remainingLength = _lineLength * (1f - t);
            _spriteRenderer.size = new Vector2(remainingLength, _config.lineWidth);

            // Move position forward để đầu line luôn ở vị trí đúng
            float movedDistance = _lineLength * t;
            transform.position = _startPos + direction * movedDistance;

            await UniTask.Yield(PlayerLoopTiming.Update, ct);
        }

        // Ensure final state (fully faded)
        _spriteRenderer.size = Vector2.zero;
        transform.position = _endPos;
    }

    // ═══════════════════════════════════════════════════════════
    // CLEANUP
    // ═══════════════════════════════════════════════════════════

    private void Cleanup()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }

    private void OnDestroy()
    {
        Cleanup();
    }
}