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

    [Header("Animation Settings")]
    public float animDuration = 0.5f;
    public float delayBetweenButtons = 0.15f;

    public void Setup(Action<int> onChosen)
    {
        gameObject.SetActive(true);
        canvasGroup.alpha = 0f;
        canvasGroup.DOKill();
        container.localScale = Vector3.zero;

        btnDenial.transform.localScale = Vector3.zero;
        btnAcceptance.transform.localScale = Vector3.zero;
        btnDenial.transform.DOKill();
        btnAcceptance.transform.DOKill();

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

        canvasGroup.alpha = 1f;           
        container.localScale = Vector3.zero; 

        canvasGroup.DOFade(1f, animDuration).SetLink(gameObject);

        btnDenial.transform.DOScale(1f, animDuration)
            .SetEase(Ease.OutBack)
            .SetLink(btnDenial.gameObject);

        btnAcceptance.transform.DOScale(1f, animDuration)
            .SetEase(Ease.OutBack)
            .SetDelay(delayBetweenButtons) 
            .SetLink(btnAcceptance.gameObject);
        
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