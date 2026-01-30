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
    public Image finalIntroImage;
    public RectTransform targetFrame; // Khung tranh ở LeftPanel
    public float slideDuration = 3f;

    [Header("2. GAMEPLAY REFS")]
    public CanvasGroup gameplayCanvasGroup;
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
        finalIntroImage.gameObject.SetActive(false);
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
        finalIntroImage.gameObject.SetActive(true);
        await finalIntroImage.DOFade(1f, 1f).From(0f).WithCancellation(token);
        await UniTask.Delay(1500, cancellationToken: token);

        // C. HIỆU ỨNG "BAY VÀO KHUNG" (Chạy song song)
        // Dùng UniTask.WhenAll để chờ cả Move, Size, và Fade Gameplay cùng xong
        RectTransform finalRect = finalIntroImage.rectTransform;
        
        // Lưu lại vị trí và kích thước hiện tại (đang full màn)
        Vector2 currentSize = finalRect.rect.size;
        Vector3 currentPos = finalRect.position;

        // Đổi Anchor về giữa (0.5, 0.5)
        finalRect.anchorMin = new Vector2(0.5f, 0.5f);
        finalRect.anchorMax = new Vector2(0.5f, 0.5f);
        finalRect.pivot = new Vector2(0.5f, 0.5f);

        // Gán lại kích thước cũ để hình không bị giật (Snap)
        finalRect.sizeDelta = currentSize;
        finalRect.position = currentPos;

        // BƯỚC 2: Thực hiện Tween bay và thu nhỏ
        var seq = DOTween.Sequence();
        
        // Bay đến vị trí khung
        seq.Join(finalRect.DOMove(targetFrame.position, 1.5f).SetEase(Ease.InOutExpo));
        
        // Thu nhỏ bằng kích thước khung (Bây giờ nó sẽ hoạt động vì Anchor đã là Center)
        seq.Join(finalRect.DOSizeDelta(targetFrame.rect.size, 1.5f).SetEase(Ease.InOutExpo));
        
        // Hiện Gameplay
        seq.Join(gameplayCanvasGroup.DOFade(1f, 1.5f));

        await seq.ToUniTask(cancellationToken: token);

        // D. Gắn ảnh vào khung luôn
        finalIntroImage.transform.SetParent(targetFrame);
        finalIntroImage.rectTransform.anchoredPosition = Vector2.zero;
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