using Cysharp.Threading.Tasks;

public class FirstChapTask : BaseTask
{
    public override UniTask Execute()
    {
        return UniTask.FromResult(true);
    }
}