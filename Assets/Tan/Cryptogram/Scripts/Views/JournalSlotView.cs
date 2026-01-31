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

    [Header("Runtime Data")]
    public string currentText;          // Từ đúng cần điền (Code tự điền)
    public bool IsFilled { get; private set; } = false;

    // Hàm khởi tạo (Controller sẽ gọi hàm này)
    public void Setup(string word, bool isHidden)
    {
        currentText = word;
        IsFilled = !isHidden; // Nếu không ẩn -> coi như đã điền rồi

        if (isHidden)
        {
            textDisplay.text = word; 
        
            // 2. Chỉnh màu chữ về trong suốt (Alpha = 0)
            textDisplay.color = new Color(0, 0, 0, 0); 
            
            // 3. Hiện dòng kẻ
            if (underlineObj) underlineObj.SetActive(true);
        }
        else
        {
            textDisplay.text = word;         // Hiện chữ sẵn
            textDisplay.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            underlineObj.SetActive(false);   // Tắt dòng kẻ (hoặc để tùy design)
        }

        if (scratchObj != null) scratchObj.SetActive(false); // Luôn tắt gạch xóa lúc đầu
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
               .OnComplete(() => {
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

    public void FillWord(string textToShow)
    {
        textDisplay.text = textToShow;
        Color c = textDisplay.color;
        c.a = 1f;
        textDisplay.color = c;
        IsFilled = true;
    }
}