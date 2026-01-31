using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Cysharp.Threading.Tasks; // Quan trọng
using System.Collections.Generic;

public class ChapterFlowManager : MonoBehaviour
{
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

    // Start có thể đổi thành async UniTaskVoid trong Unity
    async UniTaskVoid Start()
    {
        // Token hủy task nếu object này bị destroy (tránh lỗi khi tắt game)
        var token = this.GetCancellationTokenOnDestroy();

        // 1. Setup ban đầu
        SetupInitialState();

        // 2. Chạy Intro (Chờ chạy xong mới đi tiếp)
        await PlayIntroSequence(token);

        // 3. Bắt đầu Game
        puzzleController.StartGameManually();
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
        // A. Chiếu slide ảnh
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

        // B. Ảnh cuối xuất hiện
        gameplayCanvasGroup.alpha = 1; 
        rightPanelRect.anchoredPosition = Vector2.zero; // Nằm giữa

        // 2. Bật Mask lên (nó đang stretch full màn hình)
        finalImageMaskRect.gameObject.SetActive(true);
        finalImageContent.DOFade(1f, 1f).From(0f).WithCancellation(token);
        await UniTask.Delay(1500, cancellationToken: token);

        // C. GIAI ĐOẠN 1: BIẾN HÌNH THÀNH VUÔNG Ở GIỮA
        Vector2 startSize = finalImageMaskRect.rect.size;
        // Trước khi tween kích thước, phải đổi Anchor về giữa để nó co lại vào tâm
        finalImageMaskRect.anchorMin = new Vector2(0.5f, 0.5f);
        finalImageMaskRect.anchorMax = new Vector2(0.5f, 0.5f);
        finalImageMaskRect.pivot = new Vector2(0.5f, 0.5f);
        // (Mẹo: Khi đổi anchor từ stretch về center, sizeDelta nó sẽ tự tính ra kích thước màn hình hiện tại, không cần set lại)
        finalImageMaskRect.sizeDelta = startSize;

        var seqPhase1 = DOTween.Sequence();
        // Thu nhỏ Mask thành hình vuông (Ảnh bên trong sẽ bị cắt, không bị méo)
        seqPhase1.Join(finalImageMaskRect.DOSizeDelta(targetSquareSize, 1.5f).SetEase(Ease.InOutExpo));
        // Đảm bảo nó nằm đúng giữa (phòng hờ)
        seqPhase1.Join(finalImageMaskRect.DOAnchorPos(Vector2.zero, 1.5f).SetEase(Ease.InOutExpo));

        await seqPhase1.ToUniTask(cancellationToken: token);
        
        // Dừng lại 1 chút ở giữa cho kịch tính
        await UniTask.Delay(500, cancellationToken: token);


        // D. GIAI ĐOẠN 2: TÁCH ĐÔI (THE SPLIT)
        var seqPhase2 = DOTween.Sequence();
        
        // 1. Ảnh lướt sang trái (vào vị trí khung tranh)
        seqPhase2.Join(finalImageMaskRect.DOMove(targetFrameLeft.position, 1.5f).SetEase(Ease.InOutBack));
        
        // 2. Puzzle lướt sang phải (từ giữa ra vị trí đích)
        // Dùng DOAnchorPosX vì nó trượt ngang trong Canvas
        seqPhase2.Join(rightPanelRect.DOAnchorPosX(targetRightPanelPosX, 1.5f).SetEase(Ease.InOutBack));

        await seqPhase2.ToUniTask(cancellationToken: token);

        // E. HOÀN TẤT
        // Gắn cái Mask vào làm con của khung tranh bên trái luôn cho gọn
        finalImageMaskRect.SetParent(targetFrameLeft);
        finalImageMaskRect.anchoredPosition = Vector2.zero;
        
        gameplayCanvasGroup.blocksRaycasts = true;
    }

    // --- LOGIC OUTRO (Được gọi từ PuzzleController) ---
    public void TriggerOutro(PlayerDecision decision)
    {
        PlayOutroSequence(decision, this.GetCancellationTokenOnDestroy()).Forget();
    }

    async UniTaskVoid PlayOutroSequence(PlayerDecision decision, System.Threading.CancellationToken token)
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
        // SceneManager.LoadScene("MainMenu");
    }
}