using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System; // Để dùng Action
using DG.Tweening; // Để dùng DOTween

public class WordOptionView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI wordText;
    [SerializeField] private Button btnComp;
    [SerializeField] private CanvasGroup canvasGroup;
    
    private string _myLetter;
    private int _myNumber;
    private Action<string, WordOptionView> _onClickCallback;

    public void SetupKeyboardKey(string letter, bool isActive, Action<string, WordOptionView> callback)
    {
        _myLetter = letter;
        wordText.text = letter;
        _onClickCallback = callback;

        // Reset state
        gameObject.SetActive(true);
        btnComp.onClick.RemoveAllListeners();
        Debug.Log($"Setting up key '{letter}' with isActive={isActive}");
        if (isActive)
        {
            // Trạng thái Bấm Được
            btnComp.interactable = true;
            canvasGroup.alpha = 1f;
            btnComp.onClick.AddListener(() =>
            {
                Debug.Log($"Key '{_myLetter}' clicked.");
                _onClickCallback?.Invoke(_myLetter, this);
            });
            canvasGroup.blocksRaycasts = true;
        }
        else
        {
            btnComp.interactable = false;
            canvasGroup.alpha = 0.3f;
            canvasGroup.blocksRaycasts = false;
        }
    }

    public void SetupFunctionKey(string icon, Action callback)
    {
        wordText.text = icon; 
        btnComp.interactable = true;
        canvasGroup.alpha = 1f;
        
        btnComp.onClick.RemoveAllListeners();
        btnComp.onClick.AddListener(() => callback?.Invoke());
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