using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Cysharp.Threading.Tasks; // Quan trọng
using System.Collections.Generic;

public class ChapterFlowManager : MonoBehaviour
{
    [Header("DEBUG")]
    public bool skipIntro = false;
    [Header("1. INTRO CONFIG")]
    public List<Image> introSlides;
    [Tooltip("Kéo cái Gameobject 'FinalImageMask' vừa tạo vào đây")]
    public RectTransform finalImageMaskRect; 
    [Tooltip("Kéo cái ảnh con bên trong Mask vào đây để Fade")]
    public Image finalImageContent; 

    [Tooltip("Kéo cái khung tranh ở LeftPanel vào đây (Đích đến bên trái)")]
    public RectTransform targetFrameLeft;
    
    [Tooltip("Kích thước hình vuông mong muốn (ví dụ 500, 500)")]
    public Vector2 targetSquareSize = new Vector2(500, 500);
    public float slideDuration = 3f;

    [Header("2. GAMEPLAY REFS")]
    public CanvasGroup gameplayCanvasGroup;
    [Tooltip("Kéo cái RightPanel (Puzzle) vào đây")]
    public RectTransform rightPanelRect;
    [Tooltip("Vị trí X cuối cùng bên phải của Puzzle (ví dụ 400)")]
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

    public async UniTask PlayIntroOnlyAsync()
    {
        var token = this.GetCancellationTokenOnDestroy();
        SetupInitialState();

        await PlayIntroSequence(token);
    }

    public async UniTask RunPuzzleAndWaitAsync()
    {
        _puzzleCompletionSource = new UniTaskCompletionSource<bool>();
        puzzleController.StartGameManually();
        await _puzzleCompletionSource.Task;
    }

    // public async UniTask RunChapterSequence()
    // {
    //     var token = this.GetCancellationTokenOnDestroy();
        
    //     _chapterCompletionSource = new UniTaskCompletionSource<bool>();

    //     SetupInitialState();

    //     if (skipIntro)
    //     {
    //         SkipIntroSequence();
    //     }
    //     else
    //     {
    //         await PlayIntroSequence(token);
    //     }

    //     puzzleController.StartGameManually();

    //     await _chapterCompletionSource.Task;
    // }

    void SkipIntroSequence()
    {
        Debug.Log("--- SKIPPING INTRO ---");

        foreach (var img in introSlides) img.gameObject.SetActive(false);

        gameplayCanvasGroup.alpha = 1;
        gameplayCanvasGroup.blocksRaycasts = true;

        finalImageMaskRect.gameObject.SetActive(true);
        finalImageContent.color = Color.white; // Alpha = 1

        finalImageMaskRect.anchorMin = new Vector2(0.5f, 0.5f);
        finalImageMaskRect.anchorMax = new Vector2(0.5f, 0.5f);
        finalImageMaskRect.pivot = new Vector2(0.5f, 0.5f);
        finalImageMaskRect.sizeDelta = targetSquareSize;

        finalImageMaskRect.SetParent(targetFrameLeft);
        finalImageMaskRect.anchoredPosition = Vector2.zero;
        finalImageMaskRect.localScale = Vector3.one; 

        rightPanelRect.anchoredPosition = new Vector2(targetRightPanelPosX, 0);
    }

    void SetupInitialState()
    {
        gameplayCanvasGroup.alpha = 0;
        gameplayCanvasGroup.blocksRaycasts = false;
        outroPanel.SetActive(false);
        blackScreen.alpha = 0;
        quoteText.DOFade(0f, 0f).Complete();
        foreach(var img in introSlides) img.gameObject.SetActive(false);
        // finalIntroImage.gameObject.SetActive(false);
    }

    // --- LOGIC INTRO ---
    async UniTask PlayIntroSequence(System.Threading.CancellationToken token)
    {
        foreach (var img in introSlides)
        {
            img.gameObject.SetActive(true);
            
            // Fade In & Wait
            await img.DOFade(1f, 1f).From(0f).ToUniTask(cancellationToken: token);
            
            // Chờ người xem (Delay)
            await UniTask.Delay((int)(slideDuration * 1000), cancellationToken: token);
            
            // Fade Out & Wait
            await img.DOFade(0f, 1f).WithCancellation(token);
            img.gameObject.SetActive(false);
        }

        gameplayCanvasGroup.alpha = 1; 
        rightPanelRect.anchoredPosition = Vector2.zero; // Nằm giữa

        finalImageMaskRect.gameObject.SetActive(true);
        finalImageContent.DOFade(1f, 1f).From(0f).WithCancellation(token);
        await UniTask.Delay(1500, cancellationToken: token);

        Vector2 startSize = finalImageMaskRect.rect.size;
        finalImageMaskRect.anchorMin = new Vector2(0.5f, 0.5f);
        finalImageMaskRect.anchorMax = new Vector2(0.5f, 0.5f);
        finalImageMaskRect.pivot = new Vector2(0.5f, 0.5f);
        finalImageMaskRect.sizeDelta = startSize;

        var seqPhase1 = DOTween.Sequence();
        seqPhase1.Join(finalImageMaskRect.DOSizeDelta(targetSquareSize, 1.5f).SetEase(Ease.InOutExpo));
        // Đảm bảo nó nằm đúng giữa (phòng hờ)
        seqPhase1.Join(finalImageMaskRect.DOAnchorPos(Vector2.zero, 1.5f).SetEase(Ease.InOutExpo));

        await seqPhase1.ToUniTask(cancellationToken: token);
        
        await UniTask.Delay(500, cancellationToken: token);

        var rightPanelCG = rightPanelRect.GetComponent<CanvasGroup>();
        if (rightPanelCG == null) rightPanelCG = rightPanelRect.gameObject.AddComponent<CanvasGroup>();

        rightPanelCG.alpha = 0f;
        rightPanelRect.gameObject.SetActive(true);

        var seqPhase2 = DOTween.Sequence();
        
        seqPhase2.Append(finalImageMaskRect.DOMove(targetFrameLeft.position, 1.5f).SetEase(Ease.InOutBack));

        seqPhase2.Append(rightPanelCG.DOFade(1f, 1.0f).SetEase(Ease.Linear));

        await seqPhase2.ToUniTask(cancellationToken: token);

        finalImageMaskRect.SetParent(targetFrameLeft);
        finalImageMaskRect.anchoredPosition = Vector2.zero;
        
        gameplayCanvasGroup.blocksRaycasts = true;
    }

    // --- LOGIC OUTRO ---
    public async UniTask TriggerOutro(PlayerDecision decision)
    {
        var token = this.GetCancellationTokenOnDestroy();
        // Chuyển việc gọi hàm nội bộ thành await trực tiếp
        await PlayOutroSequence(decision, token);
    }

    async UniTask PlayOutroSequence(PlayerDecision decision, System.Threading.CancellationToken token)
    {
        // 1. Tắt Gameplay
        await gameplayCanvasGroup.DOFade(0f, 1f).WithCancellation(token);
        gameplayCanvasGroup.blocksRaycasts = false;

        // 2. Hiện Outro Panel
        outroPanel.SetActive(true);
        imgDenial.gameObject.SetActive(false);
        imgAcceptance.gameObject.SetActive(false);

        Image chosenImg = (decision == PlayerDecision.Acceptance) ? imgAcceptance : imgDenial;
        chosenImg.gameObject.SetActive(true);

        // 3. Fade ảnh kết lên
        await chosenImg.DOFade(1f, 2f).From(0f).WithCancellation(token);
        await UniTask.Delay(3000, cancellationToken: token);

        // 4. Màn hình đen & Quote
        blackScreen.gameObject.SetActive(true);
        await blackScreen.DOFade(1f, 2f).WithCancellation(token);
        
        quoteText.text = finalQuote;
        await quoteText.DOFade(1f, 2f).From(0f).WithCancellation(token);
        
        await UniTask.Delay(4000, cancellationToken: token);

        Debug.Log("--- THE END ---");

        // _puzzleCompletionSource?.TrySetResult(true);
        // SceneManager.LoadScene("MainMenu");
    }
}