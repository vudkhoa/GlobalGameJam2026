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
    [SerializeField] private float _startDuration = 0.8f;
    [SerializeField] private float _completeDuration = 1.0f;
    [SerializeField] private Ease _entranceEase = Ease.OutBack;
    [SerializeField] private Ease _exitEase = Ease.InBack;
    [SerializeField] private Ease _successEase = Ease.OutElastic;

    private Image _revealImage;
    private Transform _rulerContainer;

    /// <summary>
    /// Initialize references
    /// </summary>
    public void Initialize(Image revealImage, Transform rulerContainer)
    {
        _revealImage = revealImage;
        _rulerContainer = rulerContainer;
    }

    /// <summary>
    /// Play entrance animation for Image and Rulers
    /// </summary>
    public void PlayLevelStart()
    {
        // 1. Image Entrance (Fade + Scale Up)
        if (_revealImage != null)
        {
            _revealImage.transform.localScale = Vector3.one * 0.8f;
            Color c = _revealImage.color;
            c.a = 0f;
            _revealImage.color = c;

            _revealImage.transform.DOScale(1f, _startDuration).SetEase(_entranceEase);
            _revealImage.DOFade(1f, _startDuration);
        }

        // 2. Rulers Entrance (Staggered Slide/Scale)
        if (_rulerContainer != null)
        {
            int index = 0;
            foreach (Transform child in _rulerContainer)
            {
                child.localScale = Vector3.zero;
                // Stagger by 0.1s per ruler
                child.DOScale(1f, 0.5f).SetEase(_entranceEase).SetDelay(index * 0.1f);
                index++;
            }
        }
    }

    /// <summary>
    /// Play success animation
    /// Returns UniTask to allow GameLoop to wait
    /// </summary>
    public async UniTask PlayLevelComplete()
    {
        var tasks = new List<UniTask>();

        // 1. Reveal Image Celebration (Punch/Flash)
        if (_revealImage != null)
        {
            // Flash or Punch
            Tween t = _revealImage.transform.DOPunchScale(Vector3.one * 0.15f, _completeDuration, 5, 0.5f)
                .SetEase(_successEase);
            tasks.Add(t.AsyncWaitForCompletion().AsUniTask());
        }

        // 2. Rulers Exit (Slide Out / Fade) -> Enhance focus on Image
        if (_rulerContainer != null)
        {
            int index = 0;
            // Iterate backwards or forwards?
            foreach (Transform child in _rulerContainer)
            {
                // Quick exit
                Tween t = child.DOScale(0f, 0.3f).SetEase(_exitEase).SetDelay(index * 0.05f);
                // We don't necessarily need to wait for Rulers to disappear to finish the "Level Complete" sound/feel
                // But let's track one of them or simple delay
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
    }
}
