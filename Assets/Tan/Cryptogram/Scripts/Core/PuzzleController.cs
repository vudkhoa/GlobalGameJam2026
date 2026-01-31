using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text.RegularExpressions; // Dùng để tách từ {abc}
using System.Linq;                    // Dùng LINQ (First, All)
using DG.Tweening;                    // Animation
using Cysharp.Threading.Tasks;        // Async/Await

public class PuzzleController : MonoBehaviour
{
    [Header("--- 1. INTEGRATION ---")]
    [Tooltip("Kéo ChapterDirector vào đây")]
    public ChapterFlowManager flowManager;
    
    [Tooltip("Kéo UI Ending Choice Panel vào đây")]
    public EndingChoiceView choiceView;
    
    [Tooltip("Để false khi chạy game thật. Để true nếu muốn test riêng Level này")]
    public bool autoStart = false;

    [Header("--- 2. ANIMATION REFS ---")]
    [Tooltip("Kéo cái RightPanel (chứa cả Sentence và WordPool) vào đây để làm hiệu ứng bay")]
    public CanvasGroup puzzleContentGroup; 
    public float fadeDuration = 0.5f;

    [Header("--- 3. DATA & CONFIG ---")]
    public List<PuzzleLevelData> storyLevels;
    public int CurrentLevelIndex { get; private set; } = 0;

    [Header("--- 4. UI CONTAINERS ---")]
    [Tooltip("Khung chứa các Hàng Chữ (SentenceContainer)")]
    public Transform journalContainer; 
    [Tooltip("Khung chứa các nút bấm (BottomPanel)")]
    public Transform wordPoolContainer;

    [Header("--- 5. PREFABS ---")]
    [Tooltip("Prefab Hàng Ngang (LineRow)")]
    public GameObject lineRowPrefab; 
    [Tooltip("Prefab Ô Chữ (JournalSlot)")]
    public GameObject journalSlotPrefab; 
    [Tooltip("Prefab Nút Bấm (WordOption)")]
    public GameObject wordOptionPrefab;

    // --- INTERNAL STATE ---
    private PuzzleState _currentState;
    private List<JournalSlotView> _activeSlots = new List<JournalSlotView>();

    // ========================================================================
    // 1. KHỞI TẠO & FSM
    // ========================================================================

    void Start()
    {
        if (autoStart) StartGameManually();
    }

    public void StartGameManually()
    {
        CurrentLevelIndex = 0;
        LoadLevelRaw(CurrentLevelIndex);
        
        // Bắt đầu vào trạng thái CHƠI
        SwitchState(new StatePlaying(this));
    }

    void Update()
    {
        _currentState?.Update();
    }

    // Hàm chuyển đổi State
    public void SwitchState(PuzzleState newState)
    {
        RunStateTransition(newState).Forget();
    }

    private async UniTaskVoid RunStateTransition(PuzzleState newState)
    {
        if (_currentState != null) await _currentState.Exit();
        _currentState = newState;
        await _currentState.Enter();
    }

    // ========================================================================
    // 2. CORE LOGIC: LOAD LEVEL
    // ========================================================================

    public void LoadLevelRaw(int index)
    {
        if (index >= storyLevels.Count) return;

        PuzzleLevelData levelData = storyLevels[index];

        // A. DỌN DẸP CŨ
        foreach (Transform t in journalContainer) Destroy(t.gameObject);
        foreach (Transform t in wordPoolContainer) Destroy(t.gameObject);
        _activeSlots.Clear();

        List<string> hiddenWords = new List<string>();

        // B. TÁCH DÒNG (Line Rows) - Dùng ký tự '/' để xuống dòng
        string[] lines = levelData.sentence.Split('/');

        foreach (string line in lines)
        {
            // Tạo một Hàng Mới (LineRow Prefab)
            GameObject currentRowObj = Instantiate(lineRowPrefab, journalContainer);
            Transform currentRowTransform = currentRowObj.transform;

            // C. TÁCH TỪ TRONG DÒNG (Regex)
            // Pattern: Tìm cụm {abc} hoặc giữ nguyên văn bản thường
            string pattern = @"(\{.*?\})"; 
            string[] segments = Regex.Split(line, pattern);

            foreach (string segment in segments)
            {
                if (string.IsNullOrWhiteSpace(segment)) continue;

                // Xử lý cụm từ ẩn {từ cần điền}
                if (segment.StartsWith("{") && segment.EndsWith("}"))
                {
                    string cleanWord = segment.Substring(1, segment.Length - 2); // Xóa ngoặc
                    CreateSlot(cleanWord, true, currentRowTransform);
                    hiddenWords.Add(cleanWord);
                }
                // Xử lý văn bản thường -> Tách theo dấu cách
                else
                {
                    string[] normalWords = segment.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                    foreach (string word in normalWords)
                    {
                        CreateSlot(word, false, currentRowTransform);
                    }
                }
            }
        }

        // D. TẠO NÚT BẤM (Word Pool)
        foreach (var word in hiddenWords)
        {
            GameObject btnObj = Instantiate(wordOptionPrefab, wordPoolContainer);
            WordOptionView btnView = btnObj.GetComponent<WordOptionView>();
            btnView.Setup(word, OnWordOptionClicked);
        }

        // E. FIX UI LAYOUT (Bắt buộc cập nhật ngay lập tức để không bị chồng chéo)
        LayoutRebuilder.ForceRebuildLayoutImmediate(journalContainer as RectTransform);
        LayoutRebuilder.ForceRebuildLayoutImmediate(wordPoolContainer as RectTransform);

        Debug.Log($"Loaded Level {index}: {levelData.name}");
    }

    // Hàm phụ để tạo ô chữ gọn code hơn
    void CreateSlot(string content, bool isHidden, Transform parent)
    {
        GameObject slotObj = Instantiate(journalSlotPrefab, parent);
        JournalSlotView slotView = slotObj.GetComponent<JournalSlotView>();
        slotView.Setup(content, isHidden);
        
        if (isHidden) _activeSlots.Add(slotView); 
    }

    // ========================================================================
    // 3. INPUT HANDLING & GAMEPLAY LOGIC
    // ========================================================================

    public void OnWordOptionClicked(string content, WordOptionView btnView)
    {
        // Hỏi State xem có được bấm không (Transitioning thì cấm bấm)
        if (_currentState == null || !_currentState.CanInteract()) return;

        // Tìm ô trống đầu tiên
        var targetSlot = _activeSlots.FirstOrDefault(s => !s.IsFilled);
        if (targetSlot == null) return;

        PuzzleLevelData currentData = storyLevels[CurrentLevelIndex];

        // Kiểm tra đúng từ không
        if (CheckMatch(targetSlot.currentText, content, currentData))
        {
            // --- CASE A: GLITCH LEVEL (Màn cuối) ---
            if (currentData.phaseType == PuzzlePhase.Glitch)
            {
                // 1. Vẫn cho bay vào bình thường
                targetSlot.AnimateFill(PuzzlePhase.Glitch);
                btnView.Disappear();

                // 2. Chạy logic Glitch bất đồng bộ
                if (_activeSlots.All(s => s.IsFilled))
                {
                    HandleGlitchEffect(targetSlot).Forget();
                }
            }
            // --- CASE B: NORMAL LEVEL ---
            else
            {
                targetSlot.FillWord(content);
                targetSlot.AnimateFill(PuzzlePhase.Normal);
                btnView.Disappear();

                // Check Win
                if (_activeSlots.All(s => s.IsFilled))
                {
                    if (_currentState is StatePlaying playingState)
                        playingState.OnLevelCleared();
                }
            }
        }
        else
        {
            // Sai từ -> Rung nút báo lỗi
            btnView.ShakeError();
        }
    }
    
    bool CheckMatch(string requiredWord, string inputWord, PuzzleLevelData data)
    {
        string cleanRequired = CleanString(requiredWord);
        string cleanInput = CleanString(inputWord);

        // Trường hợp 1: Đúng y chang từ gốc (VD: Cười == Cười)
        if (cleanRequired == cleanInput) return true;

        // Trường hợp 2: Kiểm tra trong danh sách tráo đổi
        if (data.interchangeableGroups != null && data.interchangeableGroups.Count > 0)
        {
            foreach (string groupLine in data.interchangeableGroups)
            {
                // Tách danh sách, chuẩn hóa từng từ trong danh sách luôn
                var groupWords = groupLine.Split(',')
                    .Select(w => CleanString(w))
                    .ToList();

                // Kiểm tra xem CẢ 2 từ có nằm trong nhóm này không
                bool hasRequired = groupWords.Contains(cleanRequired);
                bool hasInput = groupWords.Contains(cleanInput);

                if (hasRequired && hasInput)
                {
                    return true;
                }
            }
        }
        return false;
    }

    string CleanString(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return "";
        return raw.Trim().ToLower().Normalize(System.Text.NormalizationForm.FormC);
    }

    // Xử lý hiệu ứng Glitch (Màn cuối)
    private async UniTaskVoid HandleGlitchEffect(JournalSlotView slot)
    {
        var token = this.GetCancellationTokenOnDestroy();

        // Chờ 0.5s sau khi điền
        await UniTask.Delay(500, cancellationToken: token);

        // Hiệu ứng Glitch: Rung lắc, Đỏ lòm, Đổi chữ
        (slot.transform as RectTransform).DOShakeAnchorPos(1f, 15f, 30);
        slot.textDisplay.color = Color.red;
        slot.textDisplay.text = "DỐI TRÁ"; // Hoặc "ĐAU KHỔ", "KẾT THÚC"

        // Chờ thêm chút cho người chơi hoảng
        await UniTask.Delay(1500, cancellationToken: token);

        // Ép chuyển sang màn hình lựa chọn (Ending)
        SwitchState(new StateEndingChoice(this));
    }

    // ========================================================================
    // 4. ANIMATION TRANSITION (Được gọi bởi StateTransitioning)
    // ========================================================================

    // Bay đi (Level cũ)
    public async UniTask AnimateLevelExitAsync()
    {
        var token = this.GetCancellationTokenOnDestroy();

        // Move Up + Fade Out
        await puzzleContentGroup.transform
            .DOLocalMoveY(150f, fadeDuration).SetRelative(true)
            .ToUniTask(cancellationToken: token);
        
        // Đảm bảo ẩn hẳn
        puzzleContentGroup.alpha = 0f;
        
        // Reset vị trí về "Dưới đất" để chuẩn bị bay lên
        // (Vị trí gốc - 300)
        puzzleContentGroup.transform.DOLocalMoveY(-300f, 0f).SetRelative(true); 
    }

    // Bay vào (Level mới)
    public async UniTask AnimateLevelEnterAsync()
    {
        var token = this.GetCancellationTokenOnDestroy();

        // Fade In
        puzzleContentGroup.DOFade(1f, fadeDuration);
        
        // Move Up về vị trí gốc (Vị trí gốc - 300 + 150 = Vị trí giữa... Logic này cần cân chỉnh tùy Scene thực tế)
        // Cách tốt nhất là lưu originalPos lúc Start, ở đây mình dùng SetRelative move lên lại 150
        await puzzleContentGroup.transform
            .DOLocalMoveY(150f, fadeDuration).SetRelative(true).SetEase(Ease.OutBack)
            .ToUniTask(cancellationToken: token);
    }

    // ========================================================================
    // 5. HELPER METHODS & ENDING
    // ========================================================================

    public void LoadNextLevelData()
    {
        CurrentLevelIndex++;
        LoadLevelRaw(CurrentLevelIndex);
    }

    public bool HasMoreLevels()
    {
        return CurrentLevelIndex < storyLevels.Count - 1;
    }

    // Được gọi bởi StateEndingChoice
    public void ShowChoiceUI()
    {
        if (choiceView != null)
        {
            choiceView.Setup(OnFinalDecisionMade);
        }
    }

    // Callback khi người chơi bấm nút chọn Ending
    private void OnFinalDecisionMade(int choiceIndex)
    {
        // Convert int sang Enum
        PlayerDecision decision = (choiceIndex == 0) ? PlayerDecision.Denial : PlayerDecision.Acceptance;

        // // Lưu vào GameManager
        // if (GameManager.Instance != null)
        // {
        //     GameManager.Instance.SaveDecision(decision);
        // }

        Debug.Log($"Chosen ending: {decision}");

        // Tắt bảng chọn
        choiceView.gameObject.SetActive(false);

        // Báo cho Director chạy Outro
        if (flowManager != null)
        {
            flowManager.TriggerOutro(decision);
        }
    }
}