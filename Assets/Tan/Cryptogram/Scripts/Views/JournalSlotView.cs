using UnityEngine;
using TMPro;
using DG.Tweening; // Nhớ import DOTween
using UnityEngine.UI;

public class JournalSlotView : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI textDisplay;  // Kéo Text vào đây
    public GameObject underlineObj;      // Kéo Image dòng kẻ vào đây
    public GameObject scratchObj;        // Kéo Image vết gạch xóa vào đây
    public GameObject highlight;

    [Header("Runtime Data")]
    public string currentText;          // Từ đúng cần điền (Code tự điền)
    public bool IsFilled { get; private set; } = false;

    // Hàm khởi tạo (Controller sẽ gọi hàm này)
    public TextMeshProUGUI numberText; // Kéo Text nhỏ dưới chân ô vào đây
    public int assignedNumber;

    public void SetupLetter(string letter, int number, bool isHidden)
    {
        currentText = letter; // Bây giờ chỉ là 1 chữ cái
        assignedNumber = number;

        if (isHidden)
        {
            numberText.text = number.ToString();
            textDisplay.text = ""; // Trống để điền
            underlineObj.SetActive(true);
            numberText.color = new Color(0.5f, 0.5f, 0.5f, 1f); // Màu xám nhạt
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

    // Hiệu ứng viết chữ khi người chơi chọn đúng
    public void AnimateFill(PuzzlePhase phase)
    {
        IsFilled = true;
        textDisplay.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        textDisplay.maxVisibleCharacters = 0; // Reset về 0 để chạy hiệu ứng

        string currentText = textDisplay.text;

        // 1. Hiệu ứng viết chữ (Typewriter)
        float duration = this.currentText.Length * 0.1f;
        Debug.Log($"Animating fill for word '{this.currentText}' over {duration} seconds.");
        DOTween.To(() => textDisplay.maxVisibleCharacters,
                   x => textDisplay.maxVisibleCharacters = x,
                   currentText.Length, duration)
               .SetEase(Ease.Linear)
               .OnComplete(() =>
               {
                   // --- HIỆU ỨNG 2: NẢY VÀ LOÉ SÁNG (Khi viết xong) ---
                   if (phase == PuzzlePhase.Normal)
                   {
                       Sequence bouncySeq = DOTween.Sequence();

                       bouncySeq.Append(transform.DOScale(new Vector3(1.2f, 0.8f, 1f), 0.1f).SetEase(Ease.OutQuad)); // Bẹp xuống
                       bouncySeq.Append(transform.DOScale(new Vector3(0.9f, 1.2f, 1f), 0.15f).SetEase(Ease.OutQuad)); // Dãn cao lên
                       bouncySeq.Append(transform.DOScale(new Vector3(1f, 1f, 1f), 0.2f).SetEase(Ease.OutElastic, 0.5f, 0.5f)); // Về chuẩn với độ rung

                       // Loé sáng màu xanh
                       textDisplay.DOColor(Color.green, 0.15f).SetLoops(2, LoopType.Yoyo);
                   }
               });

        // 2. Nếu là màn Glitch -> Rung lắc và Gạch xóa
        if (phase == PuzzlePhase.Glitch)
        {
            // Rung chữ
            textDisplay.rectTransform.DOShakeAnchorPos(0.5f, 5f, 20);
            textDisplay.DOColor(Color.red, 0.2f);

            // Hiện vết gạch xóa sau khi viết xong
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
    
    public void ShowWrongInput(string wrongLetter)
    {
        textDisplay.text = wrongLetter;
        textDisplay.color = Color.red;
        transform.DOKill(); 
        transform.DOShakePosition(0.5f, new Vector3(5f, 0, 0), 20, 90, false, true);
        DOVirtual.DelayedCall(1f, () => 
        {
            if (!IsFilled)
            {
                textDisplay.text = ""; // Xóa chữ
                textDisplay.color = new Color(0.2f, 0.2f, 0.2f, 1f); // Trả lại màu gốc (hoặc màu trống)
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
}