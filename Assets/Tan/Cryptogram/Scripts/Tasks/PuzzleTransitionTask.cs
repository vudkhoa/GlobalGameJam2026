using Cysharp.Threading.Tasks;
using UnityEngine;

public class PuzzleTransitionTask : BaseTask
{
    [SerializeField] private PuzzleController _controller;
    [SerializeField] private float _delaySeconds = 0.5f;

    public override UniTask Execute()
    {
        if (_controller != null)
        {
            RunTransitionSequence().Forget();
        }
        else
        {
            doneTask = true;
        }

        return UniTask.CompletedTask;
    }

    private async UniTaskVoid RunTransitionSequence()
    {
        await UniTask.Delay((int)(_delaySeconds * 1000));
        await _controller.AnimateLevelExitAsync();
        await UniTask.Delay((int)(_delaySeconds * 1000));
        await _controller.AnimateLevelEnterAsync();
        this.doneTask = true;
    }
}