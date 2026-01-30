using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System; // Để dùng Action
using DG.Tweening; // Để dùng DOTween

public class WordOptionView : MonoBehaviour
{
    public TextMeshProUGUI wordText;
    public Button btnComp;
    
    private string _myWord;
    private Action<string, WordOptionView> _onClickCallback;

    public void Setup(string word, Action<string, WordOptionView> callback)
    {
        _myWord = word;
        wordText.text = word;
        _onClickCallback = callback;

        // Xóa sự kiện cũ, thêm sự kiện mới
        btnComp.onClick.RemoveAllListeners();
        btnComp.onClick.AddListener(() => {
            _onClickCallback?.Invoke(_myWord, this);
        });
    }

    public void Disappear()
    {
        // Hiệu ứng biến mất khi chọn đúng
        transform.DOScale(0f, 0.2f).OnComplete(() => Destroy(gameObject));
    }

    public void ShakeError()
    {
        // Nếu nút đang bị tắt (đang rung), thì không làm gì cả
        if (btnComp.interactable == false) return;

        btnComp.interactable = false;

        (transform as RectTransform).DOShakeAnchorPos(0.5f, 10f, 20)
                 .OnComplete(() => {
                     btnComp.interactable = true;
                 });

        // Hiệu ứng màu
        wordText.DOKill(true);
        wordText.DOColor(Color.red, 0.2f).SetLoops(2, LoopType.Yoyo);
    }
}