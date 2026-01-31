using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class EndingChoiceView : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public Transform container; // Chứa 2 nút
    public Button btnDenial;
    public Button btnAcceptance;

    public void Setup(Action<int> onChosen)
    {
        gameObject.SetActive(true);
        canvasGroup.alpha = 0f;
        container.localScale = Vector3.zero;

        // Reset nút
        btnDenial.interactable = true;
        btnAcceptance.interactable = true;

        // Binding sự kiện
        btnDenial.onClick.RemoveAllListeners();
        btnDenial.onClick.AddListener(() => {
            DisableButtons();
            onChosen?.Invoke(0);
        });

        btnAcceptance.onClick.RemoveAllListeners();
        btnAcceptance.onClick.AddListener(() => {
            DisableButtons();
            onChosen?.Invoke(1);
        });

        canvasGroup.DOKill();
        container.DOKill();

        canvasGroup.alpha = 1f;            // Đưa về trong suốt
        container.localScale = Vector3.zero; // Đưa về bé tí

        // Hiện dần lên
        canvasGroup.DOFade(1f, 0.5f);
        
        // // 0.2 giây sau thì bung ra
        // container.DOScale(1f, 0.5f).SetEase(Ease.OutBack).SetDelay(0.2f);
    }

    void DisableButtons()
    {
        btnDenial.interactable = false;
        btnAcceptance.interactable = false;
        canvasGroup.DOFade(0f, 0.5f);
    }
}