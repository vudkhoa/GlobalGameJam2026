using Cysharp.Threading.Tasks;
using UnityEngine;

public class PuzzleLevelTask : BaseTask
{
    [Header("Settings")]
    public int LevelIndex;
    
    [Header("References")]
    [SerializeField] private PuzzleController _controller;

    public override UniTask Execute()
    {
        if (_controller == null)
        {
            doneTask = true;
            return UniTask.CompletedTask;
        }
        _controller.OnLevelCompleted += HandleLevelFinished;
        _controller.StartLevel(LevelIndex);
        return UniTask.CompletedTask;
    }

    private void HandleLevelFinished()
    {
        _controller.OnLevelCompleted -= HandleLevelFinished;
        Debug.Log($"Task Level {LevelIndex} Finished via Event!");
        this.doneTask = true;
    }

    private void OnDisable()
    {
        if (_controller != null)
        {
            _controller.OnLevelCompleted -= HandleLevelFinished;
        }
    }
}