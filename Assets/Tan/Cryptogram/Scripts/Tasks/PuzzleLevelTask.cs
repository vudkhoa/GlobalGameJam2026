using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class PuzzleLevelTask : BaseTask
{
    [Header("Settings")]
    public int LevelIndex;
    
    [Header("References")]
    [SerializeField] private PuzzleController _controller;

    public override async UniTask Execute()
    {
        if (_controller == null)
        {
            doneTask = true;
            return;
        }
        _controller.LoadLevelDataOnly(LevelIndex);
        _controller.StartInput();
        // if (LevelIndex > 0)
        // {
        //     await _controller.AnimateLevelEnterAsync();
        // }
        await _controller.AnimateLevelEnterAsync();

        var tcs = new UniTaskCompletionSource();
        void OnComplete() => tcs.TrySetResult();

        _controller.OnLevelCompleted += OnComplete;
        await tcs.Task;
        _controller.OnLevelCompleted -= OnComplete;

        doneTask = true;
    }

    // private void HandleLevelFinished()
    // {
    //     _controller.OnLevelCompleted -= HandleLevelFinished;

    //     this.doneTask = true;
    // }

    // private void OnDisable()
    // {
    //     if (_controller != null)
    //     {
    //         _controller.OnLevelCompleted -= HandleLevelFinished;
    //     }
    // }
}