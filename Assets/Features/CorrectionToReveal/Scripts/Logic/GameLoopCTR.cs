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
        Construct(_installer.LogicHandler);
    }

    // Dependency Injection Method
    public void Construct(CorrectionLogicHandler logicHandler)
    {
        _logicHandler = logicHandler;
        _logicHandler.OnCorrectionComplete += HandleCorrectionComplete;
    }

    public void StartGame()
    {
        _isGameActive = true;

        // Initialize logic mechanism
        _logicHandler.Initialize();

        OnGameStarted?.Invoke();
    }

    public void EndGame()
    {
        if (!_isGameActive) return;

        _isGameActive = false;

        // Return 0 as default score because ScoreService is not yet available
        OnGameCompleted?.Invoke(0);

        Debug.Log("[GameLoopCTR] Game Ended");
    }

    private void HandleCorrectionComplete()
    {
        if (_isGameActive)
        {
            EndGame();
        }
    }
}
