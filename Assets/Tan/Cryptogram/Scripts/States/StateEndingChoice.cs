using Cysharp.Threading.Tasks;

public class StateEndingChoice : PuzzleState
{
    public StateEndingChoice(PuzzleController controller) : base(controller) { }

    public override async UniTask Enter()
    {
        _controller.ShowChoiceUI();
        await UniTask.Yield();
    }
}