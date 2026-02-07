using UnityEngine;
using TMPro;
using DG.Tweening; 

public class JournalSlotView : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI textDisplay; 
    public GameObject underlineObj;      
    public GameObject scratchObj;       
    public GameObject highlight;

    [Header("Runtime Data")]
    public string currentText;
    public bool IsFilled { get; private set; } = false;
    public bool IsLowerCase { get; private set; }

    public TextMeshProUGUI numberText; 
    public int assignedNumber;

    public void SetupLetter(string letter, int number, bool isHidden)
    {
        if (!string.IsNullOrEmpty(letter))
        {
            IsLowerCase = char.IsLower(letter[0]);
        }

        currentText = letter;
        assignedNumber = number;

        if (isHidden)
        {
            numberText.text = number.ToString();
            textDisplay.text = ""; 
            underlineObj.SetActive(true);
            numberText.color = new Color(0.5f, 0.5f, 0.5f, 1f); 
        }
        else
        {
            numberText.text = "";
            textDisplay.text = letter;
            underlineObj.SetActive(false);
            numberText.color = Color.white;
        }
    }

    public void SetFocus(bool isFocused)
    {
        if (highlight != null)
            highlight.gameObject.SetActive(isFocused);
    }

    public void AnimateFill(PuzzlePhase phase)
    {
        IsFilled = true;
        textDisplay.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        textDisplay.maxVisibleCharacters = 0;

        string currentText = textDisplay.text;

        float duration = this.currentText.Length * 0.1f;
        DOTween.To(() => textDisplay.maxVisibleCharacters,
                   x => textDisplay.maxVisibleCharacters = x,
                   currentText.Length, duration)
               .SetEase(Ease.Linear)
               .OnComplete(() =>
               {
                   if (this == null || transform == null || gameObject == null) return;
                   if (phase == PuzzlePhase.Normal)
                   {
                       Sequence bouncySeq = DOTween.Sequence();

                       bouncySeq.Append(transform.DOScale(new Vector3(1.2f, 0.8f, 1f), 0.1f).SetEase(Ease.OutQuad)); 
                       bouncySeq.Append(transform.DOScale(new Vector3(0.9f, 1.2f, 1f), 0.15f).SetEase(Ease.OutQuad)); 
                       bouncySeq.Append(transform.DOScale(new Vector3(1f, 1f, 1f), 0.2f).SetEase(Ease.OutElastic, 0.5f, 0.5f)); 

                       textDisplay.DOColor(Color.green, 0.15f).SetLoops(2, LoopType.Yoyo);
                   }
               });

        if (phase == PuzzlePhase.Glitch)
        {
            textDisplay.rectTransform.DOShakeAnchorPos(0.5f, 5f, 20);
            textDisplay.DOColor(Color.green, 0.15f).SetLoops(2, LoopType.Yoyo);

            DOVirtual.DelayedCall(duration + 0.1f, () =>
            {
                if (scratchObj)
                {
                    scratchObj.SetActive(true);
                    scratchObj.transform.DOScale(1f, 0.3f).From(0f).SetEase(Ease.OutBack);
                }
            });
        }
    }

    public void AnimatePop()
    {
        transform.DOKill();
        transform.localScale = Vector3.one;
        transform.DOPunchScale(new Vector3(0.3f, 0.3f, 0), 0.3f, 10, 1);
    }
    
    public void ShowWrongInput(string wrongLetter)
    {
        textDisplay.text = IsLowerCase ? wrongLetter.ToLower() : wrongLetter.ToUpper();
        textDisplay.color = Color.red;
        transform.DOKill(); 
        transform.DOShakePosition(0.5f, new Vector3(5f, 0, 0), 20, 90, false, true);
        DOVirtual.DelayedCall(1f, () => 
        {
            if (!IsFilled)
            {
                textDisplay.text = "";
                textDisplay.color = new Color(0.2f, 0.2f, 0.2f, 1f); 
            }
        });
    }

    public void FillWord(string textToShow)
    {
        textDisplay.text = textToShow;
        Color c = textDisplay.color;
        c.a = 1f;
        textDisplay.color = c;
        IsFilled = true;
    }

    private void OnDestroy()
    {
        transform.DOKill();
    }
}