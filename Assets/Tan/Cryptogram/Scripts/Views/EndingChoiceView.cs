using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class EndingChoiceView : MonoBehaviour
{
    [Header("--- UI References ---")]
    [SerializeField] private Button _denialButton;    
    [SerializeField] private Button _acceptanceButton;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Animation Settings")]
    [SerializeField] private float _entranceDuration = 0.6f;
    [SerializeField] private float _entranceDelayBetween = 0.1f;
    [SerializeField] private Ease _entranceEase = Ease.OutElastic;

    private Action<int> _onChosenCallback;

    private void OnEnable()
    {
        ResetButtonsState();
    }

    private void ResetButtonsState()
    {
        gameObject.SetActive(true);
        canvasGroup.alpha = 0f;
        canvasGroup.DOKill();

        _denialButton.transform.localScale = Vector3.zero;
        _acceptanceButton.transform.localScale = Vector3.zero;

        _denialButton.interactable = true;
        _acceptanceButton.interactable = true;

        _denialButton.transform.DOKill();
        _acceptanceButton.transform.DOKill();
    }

    public void Setup(Action<int> onChosenCallback)
    {
        _onChosenCallback = onChosenCallback;

        _denialButton.onClick.RemoveAllListeners();
        _denialButton.onClick.AddListener(() => HandleButtonClick(_denialButton, 0));

        _acceptanceButton.onClick.RemoveAllListeners();
        _acceptanceButton.onClick.AddListener(() => HandleButtonClick(_acceptanceButton, 1));

        PlayEntranceAnimation();
    }
    
    private void PlayEntranceAnimation()
    {
        ResetButtonsState();

        Sequence seq = DOTween.Sequence();

        // seq.Append(canvasGroup.DOFade(1f, 0.3f));
        seq.Append(canvasGroup.DOFade(1f, 0.3f));

        seq.Append(_denialButton.transform.DOScale(Vector3.one, _entranceDuration).SetEase(_entranceEase));

        seq.Insert(_entranceDelayBetween, 
                   _acceptanceButton.transform.DOScale(Vector3.one, _entranceDuration).SetEase(_entranceEase));
    }

    private void HandleButtonClick(Button clickedBtn, int choiceIndex)
    {
        _denialButton.interactable = false;
        _acceptanceButton.interactable = false;

        clickedBtn.transform.DOKill(true); 
        clickedBtn.transform.localScale = Vector3.one; 

        clickedBtn.transform.DOPunchScale(new Vector3(-0.1f, -0.1f, 0f), 0.2f, 10, 1)
            .OnComplete(() => 
            {
                _onChosenCallback?.Invoke(choiceIndex);
            });
    }
}