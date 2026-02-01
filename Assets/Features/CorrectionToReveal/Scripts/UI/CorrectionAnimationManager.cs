using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages visual feedback animations for the Correction Game Loop.
/// Handles Level Start (Entrance) and Level Complete (Success) sequences.
/// Adheres to SRP: Only handles visuals/animations.
/// </summary>
public class CorrectionAnimationManager : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float _startDuration = 1.5f;
    [SerializeField] private float _completeDuration = 1.2f;
    [SerializeField] private Ease _entranceEase = Ease.OutCubic;
    [SerializeField] private Ease _exitEase = Ease.InOutCubic;
    [SerializeField] private Ease _successEase = Ease.OutElastic;

    [Header("Shader Animation")]
    [SerializeField] private float _shaderAnimationDuration = 2.0f;
    [SerializeField] private Ease _shaderEase = Ease.InOutSine;

    private Image _revealImage;
    private Transform _rulerContainer;
    private List<ShaderParameter> _currentParameters;
    private Material _currentMaterial;

    /// <summary>
    /// Initialize references
    /// </summary>
    public void Initialize(Image revealImage, Transform rulerContainer)
    {
        _revealImage = revealImage;
        _rulerContainer = rulerContainer;
    }

    /// <summary>
    /// Play entrance animation for Image and Rulers with shader reveal
    /// </summary>
    public async UniTask PlayLevelStart(List<ShaderParameter> parameters, Material material)
    {
        _currentParameters = parameters;
        _currentMaterial = material;

        // Set shader to target values initially (fully corrected state)
        if (_currentMaterial != null && _currentParameters != null)
        {
            foreach (var param in _currentParameters)
            {
                if (_currentMaterial.HasProperty(param.PropertyName))
                {
                    _currentMaterial.SetFloat(param.PropertyName, param.TargetValue);
                }
            }
        }

        // 1. Image Entrance (Smooth Fade + Gentle Scale)
        if (_revealImage != null)
        {
            _revealImage.transform.localScale = Vector3.one * 0.95f;
            Color c = _revealImage.color;
            c.a = 0f;
            _revealImage.color = c;

            // Smooth scale and fade
            _revealImage.transform.DOScale(1f, _startDuration).SetEase(_entranceEase);
            _revealImage.DOFade(1f, _startDuration).SetEase(_entranceEase);
        }

        // 2. Rulers Entrance (Smooth Staggered Fade + Slide)
        if (_rulerContainer != null)
        {
            int index = 0;
            foreach (Transform child in _rulerContainer)
            {
                // Start from slightly below and transparent
                CanvasGroup canvasGroup = child.GetComponent<CanvasGroup>();
                if (canvasGroup == null) canvasGroup = child.gameObject.AddComponent<CanvasGroup>();

                Vector3 originalPos = child.localPosition;
                child.localPosition = originalPos + Vector3.down * 30f;
                canvasGroup.alpha = 0f;

                // Smooth slide up and fade in
                float delay = index * 0.15f;
                child.DOLocalMove(originalPos, _startDuration * 0.8f)
                    .SetEase(_entranceEase)
                    .SetDelay(delay);
                canvasGroup.DOFade(1f, _startDuration * 0.8f)
                    .SetEase(_entranceEase)
                    .SetDelay(delay);

                index++;
            }
        }

        // Wait for entrance animations to complete
        await UniTask.Delay((int)(_startDuration * 1000));

        // 3. Animate shader from target values to initial values
        await AnimateShaderParameters();
    }

    /// <summary>
    /// Animate shader parameters from target to initial values
    /// </summary>
    private async UniTask AnimateShaderParameters()
    {
        if (_currentMaterial == null || _currentParameters == null) return;

        var tweens = new List<Tween>();

        foreach (var param in _currentParameters)
        {
            if (_currentMaterial.HasProperty(param.PropertyName))
            {
                float startValue = param.TargetValue;
                float endValue = param.CurrentValue;

                // Create tween for each parameter
                Tween tween = DOTween.To(
                    () => startValue,
                    x => _currentMaterial.SetFloat(param.PropertyName, x),
                    endValue,
                    _shaderAnimationDuration
                ).SetEase(_shaderEase);

                tweens.Add(tween);
            }
        }

        // Wait for all shader animations to complete
        if (tweens.Count > 0)
        {
            await UniTask.WhenAll(tweens.Select(t => t.AsyncWaitForCompletion().AsUniTask()));
        }
    }

    /// <summary>
    /// Play success animation - smooth and chill celebration
    /// Returns UniTask to allow GameLoop to wait
    /// </summary>
    public async UniTask PlayLevelComplete()
    {
        var tasks = new List<UniTask>();

        // 1. Reveal Image Celebration (Gentle Pulse)
        if (_revealImage != null)
        {
            // Gentle breathing pulse effect
            Sequence pulseSequence = DOTween.Sequence();
            pulseSequence.Append(_revealImage.transform.DOScale(1.08f, _completeDuration * 0.4f).SetEase(Ease.OutCubic));
            pulseSequence.Append(_revealImage.transform.DOScale(1.0f, _completeDuration * 0.6f).SetEase(Ease.InOutCubic));

            tasks.Add(pulseSequence.AsyncWaitForCompletion().AsUniTask());
        }

        // 2. Rulers Exit (Smooth Fade + Slide Down)
        if (_rulerContainer != null)
        {
            int index = 0;
            foreach (Transform child in _rulerContainer)
            {
                CanvasGroup canvasGroup = child.GetComponent<CanvasGroup>();
                if (canvasGroup == null) canvasGroup = child.gameObject.AddComponent<CanvasGroup>();

                float delay = index * 0.08f;

                // Smooth slide down and fade out
                child.DOLocalMoveY(child.localPosition.y - 30f, 0.6f)
                    .SetEase(_exitEase)
                    .SetDelay(delay);

                canvasGroup.DOFade(0f, 0.6f)
                    .SetEase(_exitEase)
                    .SetDelay(delay);

                index++;
            }
        }

        // Wait for the main celebration to finish
        if (tasks.Count > 0)
        {
            await UniTask.WhenAll(tasks);
        }
        else
        {
            await UniTask.Delay((int)(_completeDuration * 1000));
        }

        // 3. Image gentle fade out
        if (_revealImage != null)
        {
            await _revealImage.DOFade(0f, 0.5f).SetEase(Ease.InOutCubic).AsyncWaitForCompletion().AsUniTask();
        }
    }
}
