using Cysharp.Threading.Tasks;
using UnityEngine;

public class PuzzleTransitionTask : BaseTask
{
    [SerializeField] private PuzzleController _controller;
    [SerializeField] private float _delaySeconds = 0.5f;

    public override async UniTask Execute()
    {
        await UniTask.Delay((int)(_delaySeconds * 1000));
        await _controller.AnimateLevelExitAsync();
        await UniTask.Delay((int)(_delaySeconds * 1000));
        await _controller.AnimateLevelEnterAsync();
    }
}