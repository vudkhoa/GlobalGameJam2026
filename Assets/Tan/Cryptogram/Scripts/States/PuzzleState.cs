using Cysharp.Threading.Tasks;

public abstract class PuzzleState
{
    protected PuzzleController _controller;
    public PuzzleState(PuzzleController controller) => _controller = controller;

    public virtual async UniTask Enter() { await UniTask.Yield(); }
    public virtual async UniTask Exit() { await UniTask.Yield(); }
    public virtual void Update() { }
    public virtual bool CanInteract() { return false; }
}