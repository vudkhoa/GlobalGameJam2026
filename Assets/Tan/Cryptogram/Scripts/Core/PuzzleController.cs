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
    public CanvasGroup  puzzleContentGroup; 
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
    // private PuzzleState _currentState;
    private List<JournalSlotView> _activeSlots = new List<JournalSlotView>();
    private Dictionary<char, int> _charToNumberMap = new Dictionary<char, int>();
    private List<char> _uniqueHiddenChars = new List<char>();
    private JournalSlotView _focusedSlot;
    private Vector3 _initialUiPosition;

    private UniTaskCompletionSource<bool> _levelCompletionSource;

    private bool _isInputActive = false;

    public event Action OnLevelCompleted;


    private void Awake()
    {
        if (puzzleContentGroup != null)
        {
            _initialUiPosition = puzzleContentGroup.transform.localPosition;
        }
    }

    public void SignalLevelCompleted()
    {
        OnLevelCompleted?.Invoke();
    }

    public void LoadLevelDataOnly(int index)
    {
        CurrentLevelIndex = index;
        LoadLevelRaw(index);
    }

    public void StartLevel(int levelIndex)
    {
        LoadLevelDataOnly(levelIndex);
        _isInputActive = true;
    }

    public void StartInput()
    {
        _isInputActive = true;
    }

    public void StopInput()
    {
        _isInputActive = false;
    }

    public void LoadLevelRaw(int index)
    {
        if (index >= storyLevels.Count) return;

        PuzzleLevelData levelData = storyLevels[index];

        foreach (Transform t in journalContainer) 
        {
            t.DOKill();
            Destroy(t.gameObject);
        }
        
        foreach (Transform t in wordPoolContainer) 
        {
            t.DOKill();
            Destroy(t.gameObject);
        }
        _activeSlots.Clear();
        _charToNumberMap.Clear();
        _uniqueHiddenChars.Clear();

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

        string allHiddenParts = string.Join("", Regex.Matches(levelData.sentence, @"\{(.*?)\}")
                                     .Cast<Match>().Select(m => m.Groups[1].Value));

        int numberCounter = 1;
        if (_charToNumberMap.Count > 0)
            numberCounter = _charToNumberMap.Values.Max() + 1;

        foreach (char c in allHiddenParts)
        {
            char upperC = char.ToUpper(c);
            if (char.IsLetterOrDigit(upperC) && !_charToNumberMap.ContainsKey(upperC))
            {
                _charToNumberMap.Add(upperC, numberCounter);
                numberCounter++;
            }

            if (!_uniqueHiddenChars.Contains(upperC) && char.IsLetterOrDigit(upperC))
            {
                _uniqueHiddenChars.Add(upperC);
            }
        }

        GenerateJournalUI(levelData.sentence);

        Build3x3Grid();

        LayoutRebuilder.ForceRebuildLayoutImmediate(journalContainer as RectTransform);
        LayoutRebuilder.ForceRebuildLayoutImmediate(wordPoolContainer as RectTransform);

        var firstEmptySlot = _activeSlots.FirstOrDefault(s => !s.IsFilled);
        if (firstEmptySlot != null)
        {
            SetCurrentFocus(firstEmptySlot);
        }
        else
        {

        }
    }

    private void GenerateJournalUI(string sentence)
    {
        string[] lines = sentence.Split('/');
        foreach (string line in lines)
        {
            GameObject currentRowObj = Instantiate(lineRowPrefab, journalContainer);
            Transform currentRowTransform = currentRowObj.transform;

            string pattern = @"(\{.*?\})";
            string[] segments = Regex.Split(line, pattern);

            foreach (string segment in segments)
            {
                if (string.IsNullOrEmpty(segment)) continue;

                if (segment.StartsWith("{") && segment.EndsWith("}"))
                {
                    string content = segment.Substring(1, segment.Length - 2);
                    foreach (char c in content)
                    {
                        char upperC = char.ToUpper(c);
                        int assignedNumber = _charToNumberMap.ContainsKey(upperC) ? _charToNumberMap[upperC] : 0;
                        CreateLetterSlot(c.ToString(), assignedNumber, true, currentRowTransform);
                    }
                }
                else
                {
                    foreach (char c in segment)
                    {
                        if (c == ' ')
                        {
                            CreateSpace(currentRowTransform);
                            continue;
                        }
                        CreateLetterSlot(c.ToString(), 0, false, currentRowTransform);
                    }
                }
            }
        }
    }

    void CreateLetterSlot(string content, int number, bool isHidden, Transform parent)
    {
        GameObject slotObj = Instantiate(journalSlotPrefab, parent);
        JournalSlotView slotView = slotObj.GetComponent<JournalSlotView>();
        slotView.SetupLetter(content, number, isHidden);
        if (isHidden) _activeSlots.Add(slotView);
    }

    private void CreateSpace(Transform parent)
    {
        Instantiate(spacePrefab, parent);
    }

    void Build3x3Grid()
    {
        List<char> keyPool = new List<char>(_uniqueHiddenChars);

        if (keyPool.Count > 9)
        {
            Debug.LogWarning($"Level có {keyPool.Count} ký tự unique, nhưng lưới chỉ có 9 ô. Sẽ cắt bớt!");
            keyPool = keyPool.Take(9).ToList();
        }

        while (keyPool.Count < 9)
        {
            char noiseChar = GetRandomNoiseChar(keyPool);
            keyPool.Add(noiseChar);
        }

        keyPool = keyPool.OrderBy(x => UnityEngine.Random.value).ToList();

        GameObject row0 = Instantiate(keyboardRowPrefab, keyboardContainer);
        for (int i = 0; i < 3; i++) SpawnCharacterKey(keyPool[i].ToString(), row0.transform);

        GameObject row1 = Instantiate(keyboardRowPrefab, keyboardContainer);
        SpawnFunctionKey("◄", OnArrowLeftClicked, row1.transform);
        for (int i = 3; i < 6; i++) SpawnCharacterKey(keyPool[i].ToString(), row1.transform);
        SpawnFunctionKey("►", OnArrowRightClicked, row1.transform);

        GameObject row2 = Instantiate(keyboardRowPrefab, keyboardContainer);
        for (int i = 6; i < 9; i++) SpawnCharacterKey(keyPool[i].ToString(), row2.transform);
    }

    char GetRandomNoiseChar(List<char> existingChars)
    {
        while (true)
        {
            char c = (char)('A' + UnityEngine.Random.Range(0, 26));
            if (!existingChars.Contains(c)) return c;
        }
    }

    void SpawnCharacterKey(string letter, Transform parent)
    {
        GameObject btnObj = Instantiate(wordOptionPrefab, parent);
        WordOptionView btnView = btnObj.GetComponent<WordOptionView>();

        btnView.SetupKeyboardKey(letter, true, OnKeyboardKeyPressed);
    }

    void SpawnFunctionKey(string icon, System.Action callback, Transform parent)
    {
        GameObject btnObj = Instantiate(wordOptionPrefab, parent);
        WordOptionView btnView = btnObj.GetComponent<WordOptionView>();
        btnView.SetupFunctionKey(icon, callback);
    }

    public void OnKeyboardKeyPressed(string letter, WordOptionView btnView)
    {
        if (!_isInputActive || _focusedSlot == null) return;

        btnView.AnimateClick();

        int targetSlotNumber = _focusedSlot.assignedNumber;
    
        char inputChar = char.ToUpper(letter[0]);
        int inputNumber = _charToNumberMap.ContainsKey(inputChar) ? _charToNumberMap[inputChar] : -1;
        JournalSlotView currentSlot = _focusedSlot; 

        if (inputNumber == targetSlotNumber)
        {
            currentSlot.FillWord(currentSlot.currentText);
            currentSlot.AnimatePop();
            
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
            btnView.ShakeError();
        }
    }

    public void OnArrowLeftClicked() => NavigateFocus(-1);
    public void OnArrowRightClicked() => NavigateFocus(1);

    void NavigateFocus(int direction)
    {
        if (_activeSlots.Count == 0 || _focusedSlot == null) return;

        int currentIndex = _activeSlots.IndexOf(_focusedSlot);
        if (currentIndex == -1) return;

        for (int i = 1; i < _activeSlots.Count; i++)
        {
            int checkIndex = (currentIndex + (direction * i)) % _activeSlots.Count;
            if (checkIndex < 0) checkIndex += _activeSlots.Count;

            var candidateSlot = _activeSlots[checkIndex];

            if (!candidateSlot.IsFilled)
            {
                SetCurrentFocus(candidateSlot);
                return;
            }
        }
    }

    void NavigateToNextEmptySlot()
    {
        var nextEmpty = _activeSlots.FirstOrDefault(s => !s.IsFilled);
        if (nextEmpty != null)
        {
            SetCurrentFocus(nextEmpty);
        }
    }
    
    void SetCurrentFocus(JournalSlotView slot)
    {
        if (slot == null || slot.IsFilled) return;
        if (_focusedSlot != null) _focusedSlot.SetFocus(false);
        _focusedSlot = slot;
        if (_focusedSlot != null) _focusedSlot.SetFocus(true);
    }   

    private void CheckWinCondition()
    {
        if (_activeSlots.All(s => s.IsFilled))
        {
            HandleLevelCompleteSequence().Forget();
        }
    }
    
    private async UniTaskVoid HandleLevelCompleteSequence()
    {
        StopInput();

        var token = this.GetCancellationTokenOnDestroy();
        PuzzleLevelData currentData = storyLevels[CurrentLevelIndex];

        await UniTask.Delay(500, cancellationToken: token);

        if (currentData.phaseType == PuzzlePhase.Glitch)
        {
            await HandleGlitchEffect(_activeSlots.Last());
        }
        
        SignalLevelCompleted();
    }

    private async UniTask HandleGlitchEffect(JournalSlotView slot)
    {
        var token = this.GetCancellationTokenOnDestroy();

        await UniTask.Delay(500, cancellationToken: token);

        foreach (var activeSlot in _activeSlots)
        {
            (activeSlot.transform as RectTransform).DOShakeAnchorPos(1f, 10f, 20);
            activeSlot.textDisplay.color = Color.red;
            activeSlot.textDisplay.text = GetScaryText(activeSlot.textDisplay.text); 
        }

        (slot.transform as RectTransform).DOShakeAnchorPos(1f, 20f, 30);

        await UniTask.Delay(2000, cancellationToken: token);

        keyboardContainer.gameObject.SetActive(false);
    }

    string GetScaryText(string original)
    {
        return "?"; 
    }

    public async UniTask AnimateLevelExitAsync()
    {
        _isInputActive = false;

        var token = this.GetCancellationTokenOnDestroy();

        Vector3 exitPos = _initialUiPosition + new Vector3(0, 500f, 0);
                
        await puzzleContentGroup.transform
            .DOLocalMove(exitPos, fadeDuration).SetEase(Ease.InBack)
            .ToUniTask(cancellationToken: token);
        
        puzzleContentGroup.alpha = 0f;
    }

    public async UniTask AnimateLevelEnterAsync()
    {
        var token = this.GetCancellationTokenOnDestroy();

        Vector3 startPos = _initialUiPosition + new Vector3(0f, -500f, 0f);
        puzzleContentGroup.transform.localPosition = startPos;

        puzzleContentGroup.DOFade(1f, fadeDuration);
        await puzzleContentGroup.transform
            .DOLocalMove(_initialUiPosition, fadeDuration).SetEase(Ease.OutBack)
            .ToUniTask(cancellationToken: token);

        puzzleContentGroup.alpha = 1f;

        _isInputActive = true;
    }

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

    private async void OnFinalDecisionMade(int choiceIndex)
    {
        PlayerDecision decision = (choiceIndex == 0) ? PlayerDecision.Denial : PlayerDecision.Acceptance;


        choiceView.gameObject.SetActive(false);

        if (flowManager != null)
        {
            await flowManager.TriggerOutro(decision);
        }
        SignalLevelCompleted();
    }

    public async UniTask<int> ShowEndingChoiceAndWaitAsync()
    {
        keyboardContainer.gameObject.SetActive(false);
        
        var tcs = new UniTaskCompletionSource<int>();

        if (choiceView != null)
        {
            choiceView.Setup((resultIndex) => 
            {
                tcs.TrySetResult(resultIndex);
            });
        }
        else
        {
            Debug.LogError("Chưa gán EndingChoiceView!");
            tcs.TrySetResult(0);
        }

        return await tcs.Task;
    }
}