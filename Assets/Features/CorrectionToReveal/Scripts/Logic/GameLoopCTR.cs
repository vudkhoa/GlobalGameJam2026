using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Game Loop implementation for the Correction To Reveal mini-game.
/// Manages the lifecycle, state events, and visual flow of the game.
/// Refactored to support multi-level progression and polished animations.
/// </summary>
public class GameLoopCTR : MonoBehaviour, IGameLoop
{
    [SerializeField] private CorrectionInstaller _installer;

    private CorrectionLogicHandler _logicHandler;
    private CorrectionAnimationManager _animator;
    private bool _isGameActive;

    // IGameLoop Events
    public event Action OnGameStarted;
    public event Action<int> OnGameCompleted;

    private void Start()
    {
        if (_installer != null)
        {
            // Inject dependencies from Installer
            Construct(_installer.LogicHandler, _installer.AnimationManager);
            StartGame();
        }
        else
        {
            // CorrectionInstaller reference is missing
        }
    }

    // Dependency Injection Method
    public void Construct(CorrectionLogicHandler logicHandler, CorrectionAnimationManager animator)
    {
        _logicHandler = logicHandler;
        _animator = animator;

        if (_logicHandler != null)
        {
            _logicHandler.OnCorrectionComplete += HandleCorrectionComplete;
        }
    }

    public void StartGame()
    {
        _isGameActive = true;

        // Initialize logic mechanism
        if (_logicHandler != null) _logicHandler.Initialize();

        // Ensure rulers are interactable
        if (_installer != null && _installer.RulerSpawner != null)
        {
            _installer.RulerSpawner.SetRulersInteractable(true);
        }

        // Play Entrance Animation
        if (_animator != null)
        {
            _animator.PlayLevelStart();
        }

        OnGameStarted?.Invoke();
    }

    public void EndGame()
    {
        if (!_isGameActive) return;

        _isGameActive = false;

        // Disable Rulers Interaction when game ends
        if (_installer != null && _installer.RulerSpawner != null)
        {
            _installer.RulerSpawner.SetRulersInteractable(false);
        }

        // Return 0 as default score
        OnGameCompleted?.Invoke(0);
        GetComponentInParent<BaseTask>()?.CompletedTask();
    }

    private async void HandleCorrectionComplete()
    {
        if (!_isGameActive) return;

        // Play Success Animation and Wait
        if (_animator != null)
        {
            // Disable interaction during celebration
            if (_installer != null && _installer.RulerSpawner != null)
                _installer.RulerSpawner.SetRulersInteractable(false);

            await _animator.PlayLevelComplete();
        }

        // Try to advance to next level using the Installer
        if (_installer != null && _installer.AdvanceLevel())
        {
            // Restart game loop for new level (This triggers StartGame -> PlayLevelStart)
            StartGame();
        }
        else
        {
            // No more levels - Game Over
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
