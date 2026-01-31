using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Cysharp.Threading.Tasks;

/// <summary>
/// SRP: Handle background blur effect
/// Responsibility: Fade in/out blur overlay
/// </summary>
public class BlurEffect : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image _blurImage;

    [Header("Settings")]
    [SerializeField] private Color _blurColor = new Color(0, 0, 0, 0.7f);

    private Tweener _fadeTweener;

    // ═══════════════════════════════════════════════════════════
    // INITIALIZATION
    // ═══════════════════════════════════════════════════════════

    private void Awake()
    {
        if (_blurImage == null)
        {
            return;
        }

        // Initialize as invisible
        _blurImage.color = new Color(_blurColor.r, _blurColor.g, _blurColor.b, 0);
        _blurImage.gameObject.SetActive(false);
    }

    // ═══════════════════════════════════════════════════════════
    // PUBLIC API
    // ═══════════════════════════════════════════════════════════

    public async UniTask FadeIn(float targetAlpha, float duration)
    {
        if (_blurImage == null) return;

        // Kill existing tween
        _fadeTweener?.Kill();

        // Enable image
        _blurImage.gameObject.SetActive(true);

        // Set start color (transparent)
        Color startColor = _blurColor;
        startColor.a = 0;
        _blurImage.color = startColor;

        // Set target color
        Color targetColor = _blurColor;
        targetColor.a = targetAlpha;

        // Fade in
        _fadeTweener = _blurImage.DOColor(targetColor, duration).SetEase(Ease.OutQuad);

        await _fadeTweener.AsyncWaitForCompletion();
    }

    public async UniTask FadeOut(float duration)
    {
        if (_blurImage == null) return;

        // Kill existing tween
        _fadeTweener?.Kill();

        // Get current color
        Color currentColor = _blurImage.color;

        // Set target color (transparent)
        Color targetColor = currentColor;
        targetColor.a = 0;

        // Fade out
        _fadeTweener = _blurImage.DOColor(targetColor, duration).SetEase(Ease.InQuad);

        await _fadeTweener.AsyncWaitForCompletion();

        // Disable image
        _blurImage.gameObject.SetActive(false);
    }

    // ═══════════════════════════════════════════════════════════
    // CLEANUP
    // ═══════════════════════════════════════════════════════════

    private void OnDestroy()
    {
        _fadeTweener?.Kill();
    }
}