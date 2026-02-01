using Cysharp.Threading.Tasks;

public class StatePlaying : PuzzleState
{
    public StatePlaying(PuzzleController controller) : base(controller) { }

    public override async UniTask Enter()
    {
        // Có thể thêm anim nhỏ ở đây nếu cần
        await UniTask.Yield();
    }

    public override bool CanInteract() => true;

    public void OnLevelCleared()
    {
        _controller.SwitchState(new StateLevelComplete(_controller));
        _controller.SignalLevelCompleted();
    }
}