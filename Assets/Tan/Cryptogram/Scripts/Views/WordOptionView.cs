using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using DG.Tweening;

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

        gameObject.SetActive(true);
        btnComp.onClick.RemoveAllListeners();
        if (isActive)
        {
            btnComp.interactable = true;
            canvasGroup.alpha = 1f;
            btnComp.onClick.AddListener(() =>
            {
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
        btnComp.onClick.AddListener(() =>
        {
            AnimateClick();
            callback?.Invoke();
        });
    }

    public void AnimateClick()
    {
        transform.DOKill();
        transform.localScale = Vector3.one;
        transform.DOPunchScale(new Vector3(-0.2f, -0.2f, 0), 0.2f, 10, 1);
    }

    public void Disappear()
    {
        transform.DOScale(0f, 0.2f).OnComplete(() => Destroy(gameObject));
    }

    public void ShakeError()
    {
        if (btnComp.interactable == false) return;

        btnComp.interactable = false;

        (transform as RectTransform).DOShakeAnchorPos(0.5f, 10f, 20)
                 .OnComplete(() =>
                 {
                     btnComp.interactable = true;
                 });

        wordText.DOKill(true);
        wordText.DOColor(Color.red, 0.2f).SetLoops(2, LoopType.Yoyo);
    }

    private void OnDestroy()
    {
        transform.DOKill();
    }
}