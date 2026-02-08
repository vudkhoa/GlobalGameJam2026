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
        await _controller.AnimateLevelEnterAsync();

        var tcs = new UniTaskCompletionSource();
        void OnComplete() => tcs.TrySetResult();

        _controller.OnLevelCompleted += OnComplete;
        await tcs.Task;
        _controller.OnLevelCompleted -= OnComplete;

        doneTask = true;
    }
}