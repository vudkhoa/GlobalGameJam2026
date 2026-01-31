using Cysharp.Threading.Tasks;

public class StateLevelComplete : PuzzleState
{
    public StateLevelComplete(PuzzleController controller) : base(controller) { }

    public override async UniTask Enter()
    {
        await UniTask.Yield();
    }

    public override bool CanInteract() => false;
}