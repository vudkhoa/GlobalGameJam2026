using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

public class UIScreenManager : MonoBehaviour
{
    [SerializeField] private float transitionDuration = 0.5f;
    [SerializeField] private List<UIScreenTransition> uiScreens = new();
    [SerializeField] private int startingScreenIndex = 0;

    private RectTransform _currentScreen;
    private Dictionary<string, RectTransform> _uiScreens = new();
    private int curIndex;

    private void OnEnable()
    {
        CacheScreens();

        curIndex = startingScreenIndex;
        _currentScreen = uiScreens[startingScreenIndex].rect;
        _currentScreen.gameObject.SetActive(true);
        ExcuteTask();
    }

    private async void ExcuteTask()
    {
        int count = 0;
        foreach (var uiScreen in uiScreens)
        {
            count++;
            await uiScreen.chapterScreenManager.Execute();
            if (count < uiScreens.Count)
            {
                LoadNextUIScene();
            }
        }
    }

    private void CacheScreens()
    {
        foreach (UIScreenTransition child in uiScreens)
        {
            child.rect.gameObject.SetActive(false);
        }
    }

    public void LoadNextUIScene()
    {
        curIndex++;

        LoadUIScreen(uiScreens[curIndex]);
    }

    public void LoadUIScreen(UIScreenTransition uIScreen)
    {
        Load(uIScreen.rect, uIScreen.loadType);
    }

    public void Load(RectTransform next, LoadType loadType)
    {
        switch (loadType)
        {
            case LoadType.MoveLeft:
                Move(next, Vector2.left);
                break;
            case LoadType.MoveRight:
                Move(next, Vector2.right);
                break;
            case LoadType.MoveUp:
                Move(next, Vector2.up);
                break;
            case LoadType.MoveDown:
                Move(next, Vector2.down);
                break;

            case LoadType.Fade:
                Fade(next);
                break;

            case LoadType.ScaleIn:
                ScaleIn(next);
                break;
            case LoadType.ScaleOut:
                ScaleOut(next);
                break;

            case LoadType.MoveAndFade:
                MoveAndFade(next);
                break;

            case LoadType.FadeAndScale:
                FadeAndScale(next);
                break;

            case LoadType.ZoomIn:
                ZoomIn(next);
                break;
            case LoadType.ZoomOut:
                ZoomOut(next);
                break;
        }
    }

    private void Move(RectTransform next, Vector2 dir)
    {
        next.gameObject.SetActive(true);

        Vector2 offset = dir * 1920;
        next.anchoredPosition = offset;

        Sequence seq = DOTween.Sequence();

        if (_currentScreen != null)
        {
            seq.Join(_currentScreen
                .DOAnchorPos(-offset, transitionDuration)
                .SetEase(Ease.OutCubic));
        }

        seq.Join(next
            .DOAnchorPos(Vector2.zero, transitionDuration)
            .SetEase(Ease.OutCubic));

        Complete(seq, next);
    }


    private void Fade(RectTransform next)
    {
        next.gameObject.SetActive(true);

        var nextCg = next.GetComponent<CanvasGroup>();
        nextCg.alpha = 0;

        Sequence seq = DOTween.Sequence();

        if (_currentScreen != null)
        {
            seq.Append(_currentScreen
                .GetComponent<CanvasGroup>()
                .DOFade(0, transitionDuration));
        }

        seq.Append(nextCg.DOFade(1, transitionDuration));

        Complete(seq, next);
    }

    private void ScaleIn(RectTransform next)
    {
        next.gameObject.SetActive(true);

        next.localScale = Vector3.zero;

        Sequence seq = DOTween.Sequence();

        if (_currentScreen != null)
            seq.Append(_currentScreen.DOScale(0.8f, transitionDuration * 0.5f));

        seq.Append(next.DOScale(1f, transitionDuration).SetEase(Ease.OutBack));

        Complete(seq, next);
    }

    private void ScaleOut(RectTransform next)
    {
        next.gameObject.SetActive(true);

        next.localScale = Vector3.one * 1.2f;

        Sequence seq = DOTween.Sequence();

        if (_currentScreen != null)
            seq.Append(_currentScreen.DOScale(0f, transitionDuration));

        seq.Append(next.DOScale(1f, transitionDuration + 0.1f));

        Complete(seq, next);
    }

    private void MoveAndFade(RectTransform next)
    {
        next.gameObject.SetActive(true);

        var cg = next.GetComponent<CanvasGroup>();
        cg.alpha = 0;

        next.anchoredPosition = new Vector2(300, 0);

        Sequence seq = DOTween.Sequence();

        if (_currentScreen != null)
        {
            seq.Join(_currentScreen
                .DOAnchorPos(new Vector2(-300, 0), transitionDuration));
            seq.Join(_currentScreen
                .GetComponent<CanvasGroup>()
                .DOFade(0, transitionDuration));
        }

        seq.Join(next.DOAnchorPos(Vector2.zero, transitionDuration));
        seq.Join(cg.DOFade(1, transitionDuration));

        Complete(seq, next);
    }

    private void FadeAndScale(RectTransform next)
    {
        next.gameObject.SetActive(true);

        var cg = next.GetComponent<CanvasGroup>();
        cg.alpha = 0;
        next.localScale = Vector3.one * 0.9f;

        Sequence seq = DOTween.Sequence();

        if (_currentScreen != null)
            seq.Append(_currentScreen.GetComponent<CanvasGroup>().DOFade(0, transitionDuration));

        seq.Join(cg.DOFade(1, transitionDuration));
        seq.Join(next.DOScale(1f, transitionDuration));

        Complete(seq, next);
    }

    private void ZoomIn(RectTransform next)
    {
        next.gameObject.SetActive(true);
        next.localScale = Vector3.one * 1.5f;

        Sequence seq = DOTween.Sequence();
        seq.Append(next.DOScale(1f, transitionDuration).SetEase(Ease.OutExpo));

        Complete(seq, next);
    }

    private void ZoomOut(RectTransform next)
    {
        next.gameObject.SetActive(true);
        next.localScale = Vector3.one * 0.6f;

        Sequence seq = DOTween.Sequence();
        seq.Append(next.DOScale(1f, transitionDuration).SetEase(Ease.OutExpo));

        Complete(seq, next);
    }

    private void Complete(Sequence seq, RectTransform next)
    {
        seq.OnComplete(() =>
        {
            if (_currentScreen != null)
                _currentScreen.gameObject.SetActive(false);

            _currentScreen = next;
        });
    }

}

[SerializeField]
public enum LoadType
{
    None = 0,

    MoveLeft,
    MoveRight,
    MoveUp,
    MoveDown,

    Fade,

    ScaleIn,
    ScaleOut,

    MoveAndFade,
    FadeAndScale,

    ZoomIn,
    ZoomOut
}

[Serializable]
public class UIScreenTransition
{
    public RectTransform rect;
    public BaseTask chapterScreenManager;
    public LoadType loadType;
}