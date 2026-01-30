using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Dùng Linq để tìm kiếm nhanh
using System.Text.RegularExpressions; // Dùng Regex tách từ

public class PuzzleController : MonoBehaviour
{
    [Header("Data")]
    public PuzzleLevelData levelData;

    [Header("Container References")]
    public Transform journalContainer; // Kéo SentenceContainer vào đây
    public Transform wordPoolContainer; // Kéo BottomPanel_WordPool vào đây

    [Header("Prefab References")]
    public GameObject journalSlotPrefab; // Kéo Prefab Slot vào đây
    public GameObject wordOptionPrefab;  // Kéo Prefab Nút vào đây

    // List quản lý các ô đang hoạt động
    private List<JournalSlotView> _activeSlots = new List<JournalSlotView>();

    void Start()
    {
        if (levelData != null) LoadLevel();
    }

    void LoadLevel()
    {
        // Dọn dẹp cũ
        foreach(Transform t in journalContainer) Destroy(t.gameObject);
        foreach(Transform t in wordPoolContainer) Destroy(t.gameObject);
        _activeSlots.Clear();

        // 1. Tách từ (Logic Regex)
        // Pattern: Lấy mọi thứ, tách riêng phần trong {}
        string[] parts = Regex.Split(levelData.sentence, @"(\{.*?\})|(\s+)"); 
        // Lưu ý: Regex trên hơi phức tạp, dùng cách Split đơn giản hơn ở dưới cho an toàn:
        
        string[] rawWords = levelData.sentence.Split(' ');
        List<string> hiddenWords = new List<string>();

        // 2. Tạo ô trên giấy
        foreach (var raw in rawWords)
        {
            if (string.IsNullOrWhiteSpace(raw)) continue;

            string finalWord = raw;
            bool isHidden = false;

            // Check xem có ngoặc {} không
            if (raw.Contains("{") && raw.Contains("}"))
            {
                isHidden = true;
                finalWord = raw.Replace("{", "").Replace("}", ""); // Xóa ngoặc
                hiddenWords.Add(finalWord); // Lưu vào list để tạo nút
            }

            // Sinh ra ô Slot
            GameObject slotObj = Instantiate(journalSlotPrefab, journalContainer);
            JournalSlotView slotView = slotObj.GetComponent<JournalSlotView>();
            slotView.Setup(finalWord, isHidden);
            
            _activeSlots.Add(slotView);
        }

        // 3. Tạo nút bấm (Word Pool)
        // (Có thể Shuffle list hiddenWords ở đây nếu muốn)
        foreach (var word in hiddenWords)
        {
            GameObject btnObj = Instantiate(wordOptionPrefab, wordPoolContainer);
            WordOptionView btnView = btnObj.GetComponent<WordOptionView>();
            btnView.Setup(word, OnWordClicked); // Đăng ký hàm xử lý click
        }
    }

    // --- LOGIC KHI NGƯỜI CHƠI BẤM NÚT ---
    void OnWordClicked(string clickedWord, WordOptionView btnView)
    {
        // Tìm ô trống ĐẦU TIÊN chưa điền
        var targetSlot = _activeSlots.FirstOrDefault(s => !s.IsFilled);

        if (targetSlot == null) return; // Hết chỗ điền rồi

        // So sánh: Từ bấm vào có giống từ ô đó cần không?
        if (targetSlot.requiredWord == clickedWord)
        {
            // ĐÚNG:
            targetSlot.AnimateFill(levelData.phaseType); // Viết lên giấy
            btnView.Disappear(); // Xóa nút bấm
            CheckWin();
        }
        else
        {
            // SAI:
            btnView.ShakeError(); // Rung nút báo sai
        }
    }

    void CheckWin()
    {
        if (_activeSlots.All(s => s.IsFilled))
        {
            Debug.Log("THẮNG RỒI! CHUYỂN CẢNH ĐI!");
        }
    }
}