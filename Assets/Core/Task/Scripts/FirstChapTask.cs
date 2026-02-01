using Cysharp.Threading.Tasks;

public class FirstChapTask : BaseTask
{
    public override async UniTask Execute()
    {
        await UniTask.Yield();
    }
}