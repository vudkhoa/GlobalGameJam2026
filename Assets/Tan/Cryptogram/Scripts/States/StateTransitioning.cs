using Cysharp.Threading.Tasks;

public class StateTransitioning : PuzzleState
{
    public StateTransitioning(PuzzleController controller) : base(controller) { }

    public override async UniTask Enter()
    {
        // 1. Bay Level cũ đi
        await _controller.AnimateLevelExitAsync();

        // 2. Load Data Level mới (Lúc này màn hình trống)
        _controller.LoadNextLevelData();

        // 3. Bay Level mới vào
        await _controller.AnimateLevelEnterAsync();

        // 4. Quay lại chơi
        _controller.SwitchState(new StatePlaying(_controller));
    }
}