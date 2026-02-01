using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FrameController : MonoBehaviour
{
    [SerializeField] private List<FrameShowOptions> rects = new List<FrameShowOptions>();

    private int _currentIndex = -1;

    private void Start()
    {
        CacheFrames();

        _currentIndex = -1;
    }

    private void CacheFrames()
    {
        foreach (FrameShowOptions child in rects)
        {
            child.rect.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ShowNextFrame();
        }
    }

    public void ShowNextFrame()
    {
        if (rects.Count == 0) return;
        //// Hide current frame
        //if (_currentIndex >= 0)
        //{
        //    _rects[_currentIndex].gameObject.SetActive(false);
        //}
        // Show next frame
        _currentIndex++;
        if (_currentIndex >= rects.Count)
        {
            return;
        }
        Show(rects[_currentIndex].rect, rects[_currentIndex].showType, rects[_currentIndex].duration);
    }

    public void Show(RectTransform rect, FrameShowType type, float duration)
    {
        rect.gameObject.SetActive(true);

        if (!rect.TryGetComponent<CanvasGroup>(out var canvasGroup))
        {
            canvasGroup = rect.gameObject.AddComponent<CanvasGroup>();
        }
        Vector2 originPos = rect.anchoredPosition;
        if (!rect.TryGetComponent<RectMask2D>(out var mask))
        {
            mask = rect.gameObject.AddComponent<RectMask2D>();
        }

        switch (type)
        {
            case FrameShowType.Fade:
                canvasGroup.alpha = 0;
                canvasGroup.DOFade(1, duration);
                break;

            case FrameShowType.SlideLeft:
                rect.anchoredPosition = originPos + new Vector2(-Screen.width, 0);
                rect.DOAnchorPos(originPos, duration).SetEase(Ease.OutCubic).SetLink(rect.gameObject);
                break;

            case FrameShowType.SlideRight:
                rect.anchoredPosition = originPos + new Vector2(Screen.width, 0);
                rect.DOAnchorPos(originPos, duration).SetEase(Ease.OutCubic).SetLink(rect.gameObject);
                break;

            case FrameShowType.ZoomIn:
                rect.localScale = Vector3.zero;
                rect.DOScale(1f, duration).SetEase(Ease.OutBack);
                break;

            case FrameShowType.MaskReveal:
                if (mask != null)
                {
                    mask.enabled = true;
                    rect.localScale = new Vector3(0, 1, 1);
                    rect.DOScaleX(1, duration).SetEase(Ease.OutCubic);
                }
                break;
        }
    }
}

[SerializeField]
public enum FrameShowType
{
    None = 0,
    Fade,
    SlideLeft,
    SlideRight,
    ZoomIn,
    MaskReveal // d�ng RectMask2D
}

[Serializable]
public class FrameShowOptions
{
    public RectTransform rect;
    public FrameShowType showType;
    public float duration = 0.5f;
}
