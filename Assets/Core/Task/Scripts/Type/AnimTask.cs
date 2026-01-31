using Cysharp.Threading.Tasks;

public class AnimTask : BaseTask
{
    public override async UniTask Execute()
    {
        await UniTask.WaitForSeconds(1f);
    }
}
