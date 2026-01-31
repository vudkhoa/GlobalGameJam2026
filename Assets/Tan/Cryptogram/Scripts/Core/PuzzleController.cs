using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text.RegularExpressions; // Dùng để tách từ {abc}
using System.Linq;                    // Dùng LINQ (First, All)
using DG.Tweening;                    // Animation
using Cysharp.Threading.Tasks;
using System;        // Async/Await

public class PuzzleController : MonoBehaviour
{
    [Header("--- KEYBOARD CONFIG ---")]
    public GameObject keyboardRowPrefab; 
    public Transform keyboardContainer; 

    private readonly string[] _keyboardLayout = new string[]
    {
        "QWERTYUIOP",
        "ASDFGHJKL",
        "ZXCVBNM"
    };
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
    public GameObject spacePrefab;

    // --- INTERNAL STATE ---
    private PuzzleState _currentState;
    private List<JournalSlotView> _activeSlots = new List<JournalSlotView>();
    private Dictionary<char, int> _charToNumberMap = new Dictionary<char, int>();
    private List<char> _uniqueHiddenChars = new List<char>();
    private JournalSlotView _focusedSlot;

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
        SwitchState(new StatePlaying(this));
    }

    void Update() => _currentState?.Update();

    public void SwitchState(PuzzleState newState) => RunStateTransition(newState).Forget();

    private async UniTaskVoid RunStateTransition(PuzzleState newState)
    {
        if (_currentState != null) await _currentState.Exit();
        _currentState = newState;
        await _currentState.Enter();
    }

    // ========================================================================
    // 2. CORE LOGIC: LOAD LEVEL (CRYPTOGRAM STYLE)
    // ========================================================================

    public void LoadLevelRaw(int index)
    {
        if (index >= storyLevels.Count) return;

        PuzzleLevelData levelData = storyLevels[index];

        // A. RESET DATA
        foreach (Transform t in journalContainer) Destroy(t.gameObject);
        foreach (Transform t in wordPoolContainer) Destroy(t.gameObject);
        _activeSlots.Clear();
        _charToNumberMap.Clear();
        _uniqueHiddenChars.Clear();

        // B. XỬ LÝ MANUAL MAPPING (ID CỨNG) TRƯỚC
        if (levelData.manualMapping != null)
        {
            foreach (var item in levelData.manualMapping)
            {
                if (!string.IsNullOrEmpty(item.character))
                {
                    char c = char.ToUpper(item.character[0]);
                    if (!_charToNumberMap.ContainsKey(c))
                        _charToNumberMap.Add(c, item.id);
                }
            }
        }

        // C. QUÉT CÂU ĐỂ TÌM CÁC KÝ TỰ ẨN VÀ GÁN SỐ TỰ ĐỘNG (NẾU CHƯA CÓ)
        // Regex: Lấy nội dung trong ngoặc { }
        string allHiddenParts = string.Join("", Regex.Matches(levelData.sentence, @"\{(.*?)\}")
                                     .Cast<Match>().Select(m => m.Groups[1].Value));

        // Tìm số lớn nhất hiện có để gán tiếp, tránh trùng lặp
        int numberCounter = 1;
        if (_charToNumberMap.Count > 0)
            numberCounter = _charToNumberMap.Values.Max() + 1;

        foreach (char c in allHiddenParts)
        {
            char upperC = char.ToUpper(c);
            // Chỉ xử lý nếu là chữ cái hoặc số (bỏ qua dấu câu trong ngoặc nếu có)
            if (char.IsLetterOrDigit(upperC) && !_charToNumberMap.ContainsKey(upperC))
            {
                _charToNumberMap.Add(upperC, numberCounter);
                numberCounter++;
            }

            // Thêm vào danh sách để tạo nút bấm tí nữa
            if (!_uniqueHiddenChars.Contains(upperC) && char.IsLetterOrDigit(upperC))
            {
                _uniqueHiddenChars.Add(upperC);
            }
        }
        
        GenerateJournalUI(levelData.sentence);

        BuildFullKeyboard();

        LayoutRebuilder.ForceRebuildLayoutImmediate(journalContainer as RectTransform);
        LayoutRebuilder.ForceRebuildLayoutImmediate(wordPoolContainer as RectTransform);

        var firstEmptySlot = _activeSlots.FirstOrDefault(s => !s.IsFilled);
        if (firstEmptySlot != null)
        {
            SetCurrentFocus(firstEmptySlot);
        }
        else
        {
            Debug.LogError("Không tìm thấy ô trống nào để Focus!");
        }

        Debug.Log($"Loaded Level {index} Cryptogram Mode.");
    }

    private void GenerateJournalUI(string sentence)
    {
        string[] lines = sentence.Split('/');
        foreach (string line in lines)
        {
            GameObject currentRowObj = Instantiate(lineRowPrefab, journalContainer);
            Transform currentRowTransform = currentRowObj.transform;

            // Tách các cụm {abc} và văn bản thường
            string pattern = @"(\{.*?\})";
            string[] segments = Regex.Split(line, pattern);

            foreach (string segment in segments)
            {
                if (string.IsNullOrEmpty(segment)) continue;

                if (segment.StartsWith("{") && segment.EndsWith("}"))
                {
                    // Đây là phần ẩn: {abc}
                    string content = segment.Substring(1, segment.Length - 2);
                    foreach (char c in content)
                    {
                        char upperC = char.ToUpper(c);
                        // Lấy ID từ Map
                        int assignedNumber = _charToNumberMap.ContainsKey(upperC) ? _charToNumberMap[upperC] : 0;
                        // Tạo Slot ẩn
                        CreateLetterSlot(c.ToString(), assignedNumber, true, currentRowTransform);
                    }
                }
                else
                {
                    // Đây là văn bản thường
                    foreach (char c in segment)
                    {
                        if (c == ' ')
                        {
                            CreateSpace(currentRowTransform);
                            continue;
                        }
                        // Tạo Slot hiện sẵn (không có số ID dưới chân)
                        CreateLetterSlot(c.ToString(), 0, false, currentRowTransform);
                    }
                }
            }
        }
    }

    // --- CÁC HÀM HỖ TRỢ SINH UI ---
    void CreateLetterSlot(string content, int number, bool isHidden, Transform parent)
    {
        GameObject slotObj = Instantiate(journalSlotPrefab, parent);
        JournalSlotView slotView = slotObj.GetComponent<JournalSlotView>();
        // Giả sử JournalSlotView của bạn đã có hàm SetupLetter như thảo luận trước
        slotView.SetupLetter(content, number, isHidden);

        if (isHidden) _activeSlots.Add(slotView);
    }

    private void CreateSpace(Transform parent)
    {
        Instantiate(spacePrefab, parent);
    }

    void BuildFullKeyboard()
    {
        // Duyệt qua 3 hàng: QWERTY..., ASDF..., ZXCV...
        for (int rowIndex = 0; rowIndex < _keyboardLayout.Length; rowIndex++)
        {
            string rowString = _keyboardLayout[rowIndex];
            
            // Tạo 1 Hàng (Row Container)
            GameObject rowObj = Instantiate(keyboardRowPrefab, keyboardContainer);
            
            // Nếu là hàng cuối (ZXCVBNM), thêm nút Mũi tên TRÁI trước
            if (rowIndex == 2)
            {
                SpawnFunctionKey("◄", OnArrowLeftClicked, rowObj.transform);
            }

            // Sinh các phím chữ cái
            foreach (char c in rowString)
            {
                SpawnCharacterKey(c.ToString(), rowObj.transform);
            }

            // Nếu là hàng cuối, thêm nút Mũi tên PHẢI sau
            if (rowIndex == 2)
            {
                SpawnFunctionKey("►", OnArrowRightClicked, rowObj.transform);
            }
        }
    }

    void SpawnCharacterKey(string letter, Transform parent)
    {
        GameObject btnObj = Instantiate(wordOptionPrefab, parent);
        WordOptionView btnView = btnObj.GetComponent<WordOptionView>();

        // LOGIC QUAN TRỌNG: 
        // Active = Chữ này nằm trong danh sách CẦN ĐIỀN (_uniqueHiddenChars).
        // Inactive = Chữ này đã hiện sẵn hoặc không có trong level.
        bool isActive = _uniqueHiddenChars.Contains(letter[0]);

        btnView.SetupKeyboardKey(letter, isActive, OnKeyboardKeyPressed);
    }

    void SpawnFunctionKey(string icon, System.Action callback, Transform parent)
    {
        GameObject btnObj = Instantiate(wordOptionPrefab, parent);
        WordOptionView btnView = btnObj.GetComponent<WordOptionView>();
        btnView.SetupFunctionKey(icon, callback);
    }


    // ========================================================================
    // 3. GAMEPLAY LOGIC (INPUT)
    // ========================================================================

    public void OnKeyboardKeyPressed(string letter, WordOptionView btnView)
    {
        // LOG 1: Kiểm tra xem hàm có được gọi không
        Debug.Log($"[Controller] Received input: {letter}. Checking conditions...");

        if (_currentState == null)
        {
            Debug.LogError("LỖI: _currentState đang NULL! Bạn đã gọi StartGameManually chưa?");
            return;
        }

        if (!_currentState.CanInteract())
        {
            Debug.LogWarning("State hiện tại không cho phép tương tác.");
            return;
        }

        // LOG 2: Kiểm tra xem đã chọn ô nào chưa
        if (_focusedSlot == null)
        {
            Debug.LogError("LỖI: _focusedSlot đang NULL! Người chơi chưa chọn ô nào để điền.");
            return;
        }
        int targetSlotNumber = _focusedSlot.assignedNumber; // Số ID của ô vuông (Ví dụ: 5)
    
        char inputChar = char.ToUpper(letter[0]);
        int inputNumber = _charToNumberMap.ContainsKey(inputChar) ? _charToNumberMap[inputChar] : -1;
        JournalSlotView currentSlot = _focusedSlot; 

        if (inputNumber == targetSlotNumber)
        {
            currentSlot.FillWord(currentSlot.currentText);
            
            PuzzleLevelData currentData = storyLevels[CurrentLevelIndex];
            if (currentData.phaseType == PuzzlePhase.Glitch)
                currentSlot.AnimateFill(PuzzlePhase.Glitch);
            else
                currentSlot.AnimateFill(PuzzlePhase.Normal);

            CheckWinCondition();
            
            NavigateToNextEmptySlot();
        }
        else
        {
            currentSlot.ShowWrongInput(letter);
        }
    }

    // Khi bấm mũi tên TRÁI
    public void OnArrowLeftClicked()
    {
        NavigateFocus(-1);
    }

    // Khi bấm mũi tên PHẢI
    public void OnArrowRightClicked()
    {
        NavigateFocus(1);
    }

    // Logic điều hướng Focus
    void NavigateFocus(int direction)
    {
        if (_activeSlots.Count == 0 || _focusedSlot == null) return;

        // Tìm index của ô hiện tại trong danh sách Active Slots
        int currentIndex = _activeSlots.IndexOf(_focusedSlot);
        if (currentIndex == -1) return;

        // Tính index mới (vòng tròn)
        int newIndex = (currentIndex + direction + _activeSlots.Count) % _activeSlots.Count;
        
        SetCurrentFocus(_activeSlots[newIndex]);
    }

    void NavigateToNextEmptySlot()
    {
        // Tìm ô trống tiếp theo từ vị trí hiện tại
        // Logic đơn giản: tìm ô đầu tiên chưa điền
        var nextEmpty = _activeSlots.FirstOrDefault(s => !s.IsFilled);
        if (nextEmpty != null)
        {
            SetCurrentFocus(nextEmpty);
        }
    }
    
    void SetCurrentFocus(JournalSlotView slot)
    {
        // Bỏ focus ô cũ
        if (_focusedSlot != null)
            _focusedSlot.SetFocus(false);

        // Đặt focus ô mới
        _focusedSlot = slot;
        if (_focusedSlot != null)
            _focusedSlot.SetFocus(true);
    }   

    public void OnLetterOptionClicked(string letter, int number, WordOptionView btnView)
    {
        if (_currentState == null || !_currentState.CanInteract()) return;

        foreach (var slot in _activeSlots)
        {
            slot.FillWord(letter); // Điền chữ cái vào

            // Xử lý Animation tùy Phase
            PuzzleLevelData currentData = storyLevels[CurrentLevelIndex];
            if (currentData.phaseType == PuzzlePhase.Glitch)
            {
                slot.AnimateFill(PuzzlePhase.Glitch);
            }
            else
            {
                slot.AnimateFill(PuzzlePhase.Normal);
                slot.underlineObj.SetActive(false); // Ẩn gạch chân khi điền xong
                slot.numberText.gameObject.SetActive(false); // Ẩn số dưới chân khi điền xong
            }
        }

        btnView.Disappear();

        CheckWinCondition();
    }

    private void CheckWinCondition()
    {
        // Nếu tất cả các ô ẩn đã được điền hết
        if (_activeSlots.All(s => s.IsFilled))
        {
            PuzzleLevelData currentData = storyLevels[CurrentLevelIndex];

            if (currentData.phaseType == PuzzlePhase.Glitch)
            {
                // Nếu là màn Glitch -> Chạy hiệu ứng kinh dị
                var lastSlot = _activeSlots.Last(); 
                HandleGlitchEffect(lastSlot).Forget();
            }
            else
            {
                // Nếu là màn thường -> Thắng
                if (_currentState is StatePlaying playingState)
                    playingState.OnLevelCleared();
            }
        }
    }

    // ========================================================================
    // 4. HIỆU ỨNG GLITCH & TRANSITIONS
    // ========================================================================

    private async UniTaskVoid HandleGlitchEffect(JournalSlotView slot)
    {
        var token = this.GetCancellationTokenOnDestroy();

        // Chờ 0.5s sau khi điền chữ cuối cùng
        await UniTask.Delay(500, cancellationToken: token);

        // Hiệu ứng Glitch: Rung lắc, Đổi màu, Đổi chữ
        // Lưu ý: Có thể loop qua tất cả _activeSlots để đổi chữ hàng loạt cho sợ
        foreach (var activeSlot in _activeSlots)
        {
            (activeSlot.transform as RectTransform).DOShakeAnchorPos(1f, 10f, 20);
            activeSlot.textDisplay.color = Color.red;
            activeSlot.textDisplay.text = GetScaryText(activeSlot.textDisplay.text); 
        }

        // Rung mạnh slot cuối
        (slot.transform as RectTransform).DOShakeAnchorPos(1f, 20f, 30);

        // Chờ thêm chút cho người chơi hoảng
        await UniTask.Delay(2000, cancellationToken: token);

        // Chuyển sang màn hình lựa chọn (Ending)
        keyboardContainer.gameObject.SetActive(false); // Ẩn bàn phím
        SwitchState(new StateEndingChoice(this));
    }

    // Hàm random chữ ghê rợn (Optional)
    string GetScaryText(string original)
    {
        // Logic đơn giản: Đổi tất cả thành ký tự lạ hoặc chữ DỐI TRÁ
        return "?"; 
    }

    // --- ANIMATION CHUYỂN LEVEL ---
    public async UniTask AnimateLevelExitAsync()
    {
        var token = this.GetCancellationTokenOnDestroy();
        await puzzleContentGroup.transform
            .DOLocalMoveY(150f, fadeDuration).SetRelative(true)
            .ToUniTask(cancellationToken: token);
        
        puzzleContentGroup.alpha = 0f;
        puzzleContentGroup.transform.DOLocalMoveY(-300f, 0f).SetRelative(true); 
    }

    public async UniTask AnimateLevelEnterAsync()
    {
        var token = this.GetCancellationTokenOnDestroy();
        puzzleContentGroup.DOFade(1f, fadeDuration);
        await puzzleContentGroup.transform
            .DOLocalMoveY(150f, fadeDuration).SetRelative(true).SetEase(Ease.OutBack)
            .ToUniTask(cancellationToken: token);
    }

    // --- HELPER ---
    public void LoadNextLevelData()
    {
        CurrentLevelIndex++;
        LoadLevelRaw(CurrentLevelIndex);
    }

    public bool HasMoreLevels() => CurrentLevelIndex < storyLevels.Count - 1;

    public void ShowChoiceUI()
    {
        if (choiceView != null) choiceView.Setup(OnFinalDecisionMade);
    }

    private void OnFinalDecisionMade(int choiceIndex)
    {
        PlayerDecision decision = (choiceIndex == 0) ? PlayerDecision.Denial : PlayerDecision.Acceptance;
        Debug.Log($"Chosen ending: {decision}");
        choiceView.gameObject.SetActive(false);
        // if (flowManager != null) flowManager.TriggerOutro(decision);
    }
}