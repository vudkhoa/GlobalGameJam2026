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
            return;
        }

        _controller.LoadLevelDataOnly(LevelIndex);

        await _controller.RunLevelAndWaitAsync();
    }
}