using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Dùng Linq để tìm kiếm nhanh
using System.Text.RegularExpressions; // Dùng Regex tách từ
using DG.Tweening; // Dùng cho hiệu ứng
using UnityEngine.UI;

public class PuzzleController : MonoBehaviour
{
    [Header("Story Data")]
    public List<PuzzleLevelData> storyLevels;
    private int _currentLevelIndex = 0;

    [Header("Container References")]
    public Transform journalContainer; // Kéo SentenceContainer vào đây
    public Transform wordPoolContainer; // Kéo BottomPanel_WordPool vào đây

    [Header("Prefab References")]
    public GameObject lineRowPrefab;
    public GameObject journalSlotPrefab; // Kéo Prefab Slot vào đây
    public GameObject wordOptionPrefab;  // Kéo Prefab Nút vào đây
    public EndingChoiceView choicePanel;

    private PuzzleState _currentState;
    private List<JournalSlotView> _activeSlots = new List<JournalSlotView>();

    void Start()
    {
        // Bắt đầu game: Load level đầu tiên và vào trạng thái Playing
        LoadLevelRaw(_currentLevelIndex);
        SwitchState(new StatePlaying(this));
    }

    void Update()
    {
        if (_currentState != null) _currentState.Update();
    }

    public void SwitchState(PuzzleState newState)
    {
        if (_currentState != null) _currentState.Exit();
        _currentState = newState;
        _currentState.Enter();
    }

    public bool HasMoreLevels()
    {
        return _currentLevelIndex < storyLevels.Count - 1;
    }

    public void LoadNextLevelData()
    {
        _currentLevelIndex++;
        LoadLevelRaw(_currentLevelIndex);
    }

    public bool AreAllSlotsFilled()
    {
        return _activeSlots.All(s => s.IsFilled);
    }

    public void ShowChoiceUI()
    {
        if (choicePanel)
        {
            choicePanel.Setup(HandleFinalDecision);
        }
    }
    
    private void HandleFinalDecision(int choiceIndex)
    {
        if (choiceIndex == 0)
        {
            // LỰA CHỌN 1: GẤP LẠI (DENIAL)
            // Logic: Người chơi từ chối sự thật -> Game Reset lại từ đầu (Vòng lặp vô tận)
            Debug.Log("Người chơi chọn: GẤP LẠI -> Reset Game Loop");
            
            // // Ví dụ: Load lại scene hiện tại
            // UnityEngine.SceneManagement.SceneManager.LoadScene(
            //     UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
            // );
        }
        else
        {
            // LỰA CHỌN 2: TẨY XÓA (ACCEPTANCE)
            // Logic: Chấp nhận buông bỏ -> Hết game -> Chuyển sang Credit hoặc Chapter sau
            Debug.Log("Người chơi chọn: TẨY XÓA -> End Chapter");
            
            // Ví dụ: Load Scene menu hoặc hiện thông báo End
            // UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
            
            // // Hoặc Fade out màn hình đen thui
            // choicePanel.canvasGroup.DOFade(0f, 1f).OnComplete(() => {
            //     Debug.Log("THE END.");
            // });
        }
    }

    void LoadLevelRaw(int index)
    {
        if (index >= storyLevels.Count) return;

        PuzzleLevelData levelData = storyLevels[index];

        // Dọn dẹp cũ
        foreach (Transform t in journalContainer) Destroy(t.gameObject);
        foreach (Transform t in wordPoolContainer) Destroy(t.gameObject);
        _activeSlots.Clear();

        string[] lines = levelData.sentence.Split('/');

        List<string> hiddenWords = new List<string>();

        // 2. TÁCH TỪ THÔNG MINH (LOGIC MỚI)
        // Pattern này nghĩa là: Tìm những cụm bắt đầu bằng {, kết thúc bằng }, 
        // bên trong chứa bất cứ cái gì (kể cả dấu cách).
        foreach (string line in lines)
        {
            // Mỗi dòng -> Tạo 1 cái LineRow_Prefab nằm trong SentenceContainer
            GameObject currentRowObj = Instantiate(lineRowPrefab, journalContainer);
            
            // 2. TÁCH TỪ TRONG DÒNG ĐÓ (Logic Regex cũ)
            string pattern = @"(\{.*?\})";
            string[] segments = Regex.Split(line, pattern);

            foreach (string segment in segments)
            {
                if (string.IsNullOrWhiteSpace(segment)) continue;

                // Xử lý từ ẩn/hiện như cũ
                if (segment.StartsWith("{") && segment.EndsWith("}"))
                {
                    string cleanContent = segment.Substring(1, segment.Length - 2);
                    // QUAN TRỌNG: Cha của ô chữ bây giờ là 'currentRowObj' (cái Hàng), không phải Container to
                    CreateSlot(cleanContent, true, currentRowObj.transform); 
                    hiddenWords.Add(cleanContent);
                }
                else
                {
                    string[] normalWords = segment.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                    foreach (string word in normalWords)
                    {
                        CreateSlot(word, false, currentRowObj.transform);
                    }
                }
            }
        }

        // 3. Tạo nút bấm (Word Pool)
        // (Có thể Shuffle list hiddenWords ở đây nếu muốn)
        foreach (var word in hiddenWords)
        {
            GameObject btnObj = Instantiate(wordOptionPrefab, wordPoolContainer);
            WordOptionView btnView = btnObj.GetComponent<WordOptionView>();
            btnView.Setup(word, OnWordOptionClicked); // Đăng ký hàm xử lý click
        }

        foreach (var slot in _activeSlots)
        {
            // Lệnh này bắt TMP tính toán lại kích thước chữ ngay, không chờ cuối frame
            slot.textDisplay.ForceMeshUpdate(); 
        }

        // BƯỚC 2: Cập nhật Layout của khung chứa
        // (Cần gọi 2 lần để đảm bảo ContentSizeFitter của từng con chạy xong rồi mới đến cha)
        LayoutRebuilder.ForceRebuildLayoutImmediate(journalContainer as RectTransform);
        LayoutRebuilder.ForceRebuildLayoutImmediate(journalContainer as RectTransform);

        Debug.Log($"Loaded Level {index}");
    }

    void CreateSlot(string content, bool isHidden, Transform parentRow)
    {
        GameObject slotObj = Instantiate(journalSlotPrefab, parentRow); 
        JournalSlotView slotView = slotObj.GetComponent<JournalSlotView>();
        slotView.Setup(content, isHidden);
        _activeSlots.Add(slotView);
    }

    // --- LOGIC KHI NGƯỜI CHƠI BẤM NÚT ---
    public void OnWordOptionClicked(string content, WordOptionView btnView)
    {
        // 1. Hỏi State: "Giờ có được bấm không?"
        if (_currentState == null || !_currentState.CanInteract()) return;

        // 2. Logic tìm ô trống (Giữ nguyên như cũ)
        var targetSlot = _activeSlots.FirstOrDefault(s => !s.IsFilled);
        if (targetSlot == null) return;

        if (targetSlot.requiredWord == content)
        {
            PuzzleLevelData currentData = storyLevels[_currentLevelIndex];
            targetSlot.AnimateFill(currentData.phaseType);
            btnView.Disappear();

            // 3. Nếu đang ở State Playing -> Bảo nó kiểm tra thắng thua
            if (_currentState is StatePlaying playingState)
            {
                playingState.CheckWinCondition();
            }
        }
        else
        {
            btnView.ShakeError();
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