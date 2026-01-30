using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System; // Để dùng Action

public class EndingChoiceView : MonoBehaviour
{
    [Header("UI Refs")]
    // public CanvasGroup canvasGroup;
    public Button btnOption1; // Nút Trái (Gấp lại)
    public Button btnOption2; // Nút Phải (Tẩy xóa)
    // public Transform container; // Cái khung chứa 2 nút (để scale animation)

    // Sự kiện gửi kết quả ra ngoài (0: Option 1, 1: Option 2)
    private Action<int> _onChoiceSelected;

    public void Setup(Action<int> callback)
    {
        _onChoiceSelected = callback;

        // Reset UI
        // canvasGroup.alpha = 0f;
        // container.localScale = Vector3.zero;
        gameObject.SetActive(true);

        // Lắng nghe sự kiện click
        btnOption1.onClick.RemoveAllListeners();
        btnOption1.onClick.AddListener(() => OnClick(0));

        btnOption2.onClick.RemoveAllListeners();
        btnOption2.onClick.AddListener(() => OnClick(1));

        // Animation hiện lên
        ShowAnim();
    }

    void ShowAnim()
    {
        // Hiện dần nền đen
        // canvasGroup.DOFade(1f, 1f);
        // // Bung 2 nút ra
        // container.DOScale(1f, 0.8f).SetEase(Ease.OutBack).SetDelay(0.2f);
    }

    void OnClick(int choiceIndex)
    {
        // Tắt nút để không bấm 2 lần
        btnOption1.interactable = false;
        btnOption2.interactable = false;

        // Gửi kết quả về PuzzleController
        _onChoiceSelected?.Invoke(choiceIndex);
    }
}