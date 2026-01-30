using Cysharp.Threading.Tasks;

public class StateLevelComplete : PuzzleState
{
    public StateLevelComplete(PuzzleController controller) : base(controller) { }

    public override async UniTask Enter()
    {
        // Chờ 1.5 giây (dùng UniTask.Delay)
        await UniTask.Delay(1500);

        if (_controller.HasMoreLevels())
        {
            // Còn màn -> Chuyển cảnh
            _controller.SwitchState(new StateTransitioning(_controller));
        }
        else
        {
            // Hết màn -> Chọn kết thúc
            _controller.SwitchState(new StateEndingChoice(_controller));
        }
    }
}