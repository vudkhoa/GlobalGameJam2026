using System;
using UnityEngine;

/// <summary>
/// Game Loop implementation for the Correction To Reveal mini-game.
/// Manages the lifecycle and state events of the game.
/// </summary>
public class GameLoopCTR : MonoBehaviour, IGameLoop
{
    [SerializeField] private CorrectionInstaller _installer;

    private CorrectionLogicHandler _logicHandler;
    private bool _isGameActive;

    // IGameLoop Events
    public event Action OnGameStarted;
    public event Action<int> OnGameCompleted;

    private void Start()
    {
        if (_installer != null)
        {
            Construct(_installer.LogicHandler);
            StartGame();
        }
        else
        {
            Debug.LogError("[GameLoopCTR] CorrectionInstaller reference is missing!");
        }
    }

    // Dependency Injection Method
    public void Construct(CorrectionLogicHandler logicHandler)
    {
        _logicHandler = logicHandler;

        if (_logicHandler != null)
        {
            Debug.Log("[GameLoopCTR] LogicHandler assigned");
            _logicHandler.OnCorrectionComplete += HandleCorrectionComplete;
        }
    }

    public void StartGame()
    {
        _isGameActive = true;

        // Initialize logic mechanism
        if (_logicHandler != null) _logicHandler.Initialize();

        // Ensure rulers are interactable
        if (_installer != null)
        {
            _installer.RulerSpawner.SetRulersInteractable(true);
        }

        OnGameStarted?.Invoke();
    }

    public void EndGame()
    {
        if (!_isGameActive) return;

        _isGameActive = false;

        // Disable Rulers Interaction when game ends
        if (_installer != null)
        {
            _installer.RulerSpawner.SetRulersInteractable(false);
        }

        // Return 0 as default score because ScoreService is not yet available
        OnGameCompleted?.Invoke(0);

        Debug.Log("[GameLoopCTR] Game Ended");
    }

    private void HandleCorrectionComplete()
    {
        // When correction is complete, end the game immediately
        if (_isGameActive)
        {
            EndGame();
        }
    }

    private void OnDestroy()
    {
        if (_logicHandler != null)
        {
            _logicHandler.OnCorrectionComplete -= HandleCorrectionComplete;
        }
    }
}
