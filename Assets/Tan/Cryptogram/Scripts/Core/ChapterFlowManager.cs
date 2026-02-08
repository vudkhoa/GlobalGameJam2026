using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Cysharp.Threading.Tasks; // Quan trọng
using System.Collections.Generic;
using System;
using System.Threading;

public class ChapterFlowManager : MonoBehaviour
{
    [Header("1. INTRO CONFIG")]
    [Header("--- DAY NIGHT TRANSITION---")]
    public DayNightTransitionController dayNightController;
    public bool playDayNightCycle = true;
    [SerializeField] private CanvasGroup _dayNightCanvasGroup;
    
    [Header("--- ROOM SCENES ---")]
    [SerializeField] private RectTransform _sceneARect;      
    [SerializeField] private CanvasGroup _sceneAGroup;       
    [SerializeField] private RectTransform _sceneBRect;
    [SerializeField] private CanvasGroup _sceneBGroup;

    public RectTransform finalImageMaskRect;
    public Image finalImageContent;
    public HandWritingAnimation handImage;

    public RectTransform targetFrameLeft;
    
    public Vector2 targetSquareSize = new Vector2(500, 500);
    public float slideDuration = 3f;

    [Header("2. GAMEPLAY REFS")]
    public CanvasGroup gameplayCanvasGroup;
    public RectTransform rightPanelRect;
    public float targetRightPanelPosX = 400f;
    public PuzzleController puzzleController;

    [Header("3. OUTRO CONFIG")]
    public GameObject outroPanel;
    public Image imgDenial;
    public Image imgAcceptance;
    public CanvasGroup blackScreen;
    public TextMeshProUGUI quoteText;
    [TextArea] public string finalQuote;

    private UniTaskCompletionSource<bool> _puzzleCompletionSource;

    public event Action OnIntroCompleted;

    public async UniTask PlayIntroOnlyAsync()
    {
        var token = this.GetCancellationTokenOnDestroy();
        SetupInitialState();

        await PlayIntroSequence(token);

        OnIntroCompleted?.Invoke();
    }

    void SetupInitialState()
    {
        gameplayCanvasGroup.alpha = 0;
        gameplayCanvasGroup.blocksRaycasts = false;
        outroPanel.SetActive(false);
        blackScreen.alpha = 0;
        quoteText.DOFade(0f, 0f).Complete();

        _sceneAGroup.alpha = 0f;
        _sceneBGroup.alpha = 0f;

        if (dayNightController != null)
        {
            dayNightController.gameObject.SetActive(true);
            dayNightController.ResetToDay();
        }
    }

    private async UniTask PlayZoomTransition(CancellationToken token)
    {
        _sceneARect.DOKill(true); 
        _sceneAGroup.DOKill(true);
        _sceneBRect.DOKill(true);

        _sceneBGroup.alpha = 0f;
        _sceneBGroup.gameObject.SetActive(true);
        _sceneBRect.localScale = new Vector3(1.3f, 1.3f, 1f);

        await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: token);
    
        _sceneAGroup.alpha = 1f;
        _sceneAGroup.gameObject.SetActive(true);

        Sequence seq = DOTween.Sequence();

        float duration = 1.5f;
        float switchTime = duration * 0.85f;
        float fadeDuration = duration - switchTime;

        seq.PrependInterval(0.05f);

        seq.Append(_sceneARect.DOScale(30f, duration).SetEase(Ease.InExpo));

        seq.Insert(switchTime, _sceneBGroup.DOFade(1f, fadeDuration));
        seq.Insert(switchTime, _sceneBRect.DOScale(1f, 1.5f).SetEase(Ease.OutCubic));

        await seq.ToUniTask(cancellationToken: token);

        _sceneAGroup.gameObject.SetActive(false);
        _sceneBGroup.gameObject.SetActive(false);
    }

    private async UniTask TransitionFromDayNightToSceneA(CancellationToken token)
    {
        _dayNightCanvasGroup.alpha = 1f;
        _dayNightCanvasGroup.gameObject.SetActive(true);
        _dayNightCanvasGroup.transform.localPosition = Vector3.zero;

        _sceneAGroup.alpha = 0f;
        _sceneAGroup.gameObject.SetActive(true);
        _sceneARect.localScale = Vector3.one; 
        _sceneARect.anchoredPosition = new Vector2(0, -150f);
        Sequence seq = DOTween.Sequence();
        float duration = 2.5f;

        seq.Append(_dayNightCanvasGroup.transform.DOLocalMoveY(200f, duration)
            .SetEase(Ease.InSine)); 
        
        seq.Join(_dayNightCanvasGroup.DOFade(0f, duration));

        float overlapTime = duration * 0.3f; 

        seq.Insert(overlapTime, _sceneAGroup.DOFade(1f, duration * 0.8f));

        seq.Insert(overlapTime, _sceneARect.DOAnchorPosY(0f, duration)
            .SetEase(Ease.OutCubic));

        await seq.ToUniTask(cancellationToken: token);

        _dayNightCanvasGroup.gameObject.SetActive(false);
        _dayNightCanvasGroup.transform.localPosition = Vector3.zero;
    }

    async UniTask PlayIntroSequence(CancellationToken token)
    {
        await UniTask.Delay(1500, cancellationToken: token);

        if (playDayNightCycle && dayNightController != null)
        {

            await dayNightController.PlayDayNightCycleAsync(token);

            await UniTask.Delay(1000, cancellationToken: token);
        }

        await TransitionFromDayNightToSceneA(token);

        await UniTask.Delay(1500, cancellationToken: token);

        await PlayZoomTransition(token);

        var rightPanelCG = rightPanelRect.GetComponent<CanvasGroup>();
        if (rightPanelCG == null) rightPanelCG = rightPanelRect.gameObject.AddComponent<CanvasGroup>();
        rightPanelCG.alpha = 0f;

        gameplayCanvasGroup.alpha = 1; 
        rightPanelRect.anchoredPosition = Vector2.zero; 

        finalImageMaskRect.gameObject.SetActive(true);
        // finalImageContent.DOFade(1f, 1f).From(0f).WithCancellation(token);
        await UniTask.Delay(1500, cancellationToken: token);

        Vector2 startSize = finalImageMaskRect.rect.size;
        finalImageMaskRect.anchorMin = new Vector2(0.5f, 0.5f);
        finalImageMaskRect.anchorMax = new Vector2(0.5f, 0.5f);
        finalImageMaskRect.pivot = new Vector2(0.5f, 0.5f);
        finalImageMaskRect.sizeDelta = startSize;

        var seqPhase1 = DOTween.Sequence();
        seqPhase1.Append(finalImageContent.transform.DOScale(0.78f, 1.0f).SetEase(Ease.InOutBack).SetLink(finalImageContent.gameObject));
        seqPhase1.Join(finalImageMaskRect.DOSizeDelta(targetSquareSize, 1.5f).SetEase(Ease.InOutExpo).SetLink(finalImageMaskRect.gameObject));
        seqPhase1.Join(finalImageMaskRect.DOAnchorPos(Vector2.zero, 1.5f).SetEase(Ease.InOutExpo).SetLink(finalImageMaskRect.gameObject));

        await seqPhase1.ToUniTask(cancellationToken: token);
        
        await UniTask.Delay(500, cancellationToken: token);

        rightPanelRect.gameObject.SetActive(true);

        var seqPhase2 = DOTween.Sequence();
        
        
        seqPhase2.Append(finalImageMaskRect.DOMove(targetFrameLeft.position, 1.5f).SetEase(Ease.InOutBack));

        await seqPhase2.ToUniTask(cancellationToken: token);

        finalImageMaskRect.SetParent(targetFrameLeft);
        finalImageMaskRect.anchoredPosition = Vector2.zero;

        gameplayCanvasGroup.blocksRaycasts = true;

        handImage.StartWriting();
    }

    public async UniTask TriggerOutro(PlayerDecision decision)
    {
        var token = this.GetCancellationTokenOnDestroy();
        handImage.StopWriting();
        await PlayOutroSequence(decision, token);
    }

    async UniTask PlayOutroSequence(PlayerDecision decision, System.Threading.CancellationToken token)
    {
        await gameplayCanvasGroup.DOFade(0f, 1f).WithCancellation(token);
        gameplayCanvasGroup.blocksRaycasts = false;

        outroPanel.SetActive(true);
        imgDenial.gameObject.SetActive(false);
        imgAcceptance.gameObject.SetActive(false);

        Image chosenImg = (decision == PlayerDecision.Acceptance) ? imgAcceptance : imgDenial;
        chosenImg.gameObject.SetActive(true);

        await chosenImg.DOFade(1f, 2f).From(0f).WithCancellation(token);
        await UniTask.Delay(3000, cancellationToken: token);

        blackScreen.gameObject.SetActive(true);
        await blackScreen.DOFade(1f, 2f).WithCancellation(token);
        
        chosenImg.gameObject.SetActive(false);

        quoteText.text = finalQuote;
        await quoteText.DOFade(1f, 2f).From(0f).WithCancellation(token);
        
        await UniTask.Delay(4000, cancellationToken: token);
    }
}