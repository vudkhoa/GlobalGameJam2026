using UnityEngine;
using TMPro;
using DG.Tweening; // Nhớ import DOTween

public class JournalSlotView : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI textDisplay;  // Kéo Text vào đây
    public GameObject underlineObj;      // Kéo Image dòng kẻ vào đây
    public GameObject scratchObj;        // Kéo Image vết gạch xóa vào đây

    [Header("Runtime Data")]
    public string requiredWord;          // Từ đúng cần điền (Code tự điền)
    public bool IsFilled { get; private set; } = false;

    // Hàm khởi tạo (Controller sẽ gọi hàm này)
    public void Setup(string word, bool isHidden)
    {
        requiredWord = word;
        IsFilled = !isHidden; // Nếu không ẩn -> coi như đã điền rồi

        if (isHidden)
        {
            textDisplay.text = "";           // Ẩn chữ đi
            underlineObj.SetActive(true);    // Hiện dòng kẻ
        }
        else
        {
            textDisplay.text = word;         // Hiện chữ sẵn
            underlineObj.SetActive(false);   // Tắt dòng kẻ (hoặc để tùy design)
        }

        if (scratchObj != null) scratchObj.SetActive(false); // Luôn tắt gạch xóa lúc đầu
    }

    // Hiệu ứng viết chữ khi người chơi chọn đúng
    public void AnimateFill(PuzzlePhase phase)
    {
        IsFilled = true;
        textDisplay.text = requiredWord;
        textDisplay.maxVisibleCharacters = 0; // Reset về 0 để chạy hiệu ứng

        // 1. Hiệu ứng viết chữ (Typewriter)
        float duration = requiredWord.Length * 0.1f;
        DOTween.To(() => textDisplay.maxVisibleCharacters, 
                   x => textDisplay.maxVisibleCharacters = x, 
                   requiredWord.Length, duration)
               .SetEase(Ease.Linear);

        // 2. Nếu là màn Glitch -> Rung lắc và Gạch xóa
        if (phase == PuzzlePhase.Glitch)
        {
            // Rung chữ
            textDisplay.rectTransform.DOShakeAnchorPos(0.5f, 5f, 20);
            textDisplay.DOColor(Color.red, 0.2f);

            // Hiện vết gạch xóa sau khi viết xong
            DOVirtual.DelayedCall(duration + 0.1f, () => {
                if(scratchObj) {
                    scratchObj.SetActive(true);
                    scratchObj.transform.DOScale(1f, 0.3f).From(0f).SetEase(Ease.OutBack);
                }
            });
        }
    }
}