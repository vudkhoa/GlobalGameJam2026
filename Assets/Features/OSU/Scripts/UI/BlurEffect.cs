using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// SRP: Handle background blur effect with phase-specific sprites
/// Responsibility: Fade in/out blur overlay and manage phase backgrounds
/// </summary>
public class BlurEffect : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image _blurImage;
    [SerializeField] private Image _phaseBackgroundImage;

    [Header("Phase Sprites")]
    [Tooltip("Background sprite cho phase 1")]
    [SerializeField] private Sprite _phase1Sprite;

    [Tooltip("Background sprite cho phase 2")]
    [SerializeField] private Sprite _phase2Sprite;

    [Tooltip("Background sprite cho phase 3")]
    [SerializeField] private Sprite _phase3Sprite;

    private void Awake()
    {
        // Ensure phase background starts hidden
        if (_phaseBackgroundImage != null)
        {
            _phaseBackgroundImage.enabled = false;
        }
    }

    /// <summary>
    /// Blur background without changing phase sprite (dùng cho intro2)
    /// </summary>
    public void BlurBg()
    {
        if (_blurImage == null)
        {
            return;
        }

        // Set blur visible
        var color = _blurImage.color;
        color.a = 0.8f;
        _blurImage.color = color;
    }

    /// <summary>
    /// Remove blur and hide phase background
    /// </summary>
    public void UnBlurBg()
    {
        if (_blurImage == null)
        {
            return;
        }

        // Set blur transparent
        var color = _blurImage.color;
        color.a = 0;
        _blurImage.color = color;

        // Hide phase background when unblurring
        if (_phaseBackgroundImage != null)
        {
            _phaseBackgroundImage.enabled = false;
        }
    }

    /// <summary>
    /// Show blur with specific phase background sprite
    /// </summary>
    /// <param name="phaseIndex">0 = phase 1, 1 = phase 2, 2 = phase 3</param>
    public void ShowPhaseBackground(int phaseIndex)
    {
        if (_phaseBackgroundImage == null)
        {
            return;
        }

        // Select sprite based on phase index
        Sprite selectedSprite = null;
        switch (phaseIndex)
        {
            case 0: // Phase 1
                selectedSprite = _phase1Sprite;
                Debug.Log("[BlurEffect] Showing Phase 1 background");
                break;
            case 1: // Phase 2
                selectedSprite = _phase2Sprite;
                Debug.Log("[BlurEffect] Showing Phase 2 background");
                break;
            case 2: // Phase 3
                selectedSprite = _phase3Sprite;
                Debug.Log("[BlurEffect] Showing Phase 3 background");
                break;
            default:
                Debug.LogWarning($"[BlurEffect] Invalid phase index: {phaseIndex}");
                break;
        }

        // Apply sprite and show background
        if (selectedSprite != null)
        {
            _phaseBackgroundImage.sprite = selectedSprite;
            _phaseBackgroundImage.enabled = true;
        }

        // Show blur on top
        BlurBg();
    }
}