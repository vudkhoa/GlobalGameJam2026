using Cysharp.Threading.Tasks;

public class FirstChapTask : BaseTask
{
    public override UniTask<bool> Execute()
    {

        return UniTask.FromResult(true);
    }
}