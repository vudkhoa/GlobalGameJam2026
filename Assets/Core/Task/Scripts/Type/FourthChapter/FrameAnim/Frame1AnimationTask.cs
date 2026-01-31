using Cysharp.Threading.Tasks;

public class Frame1AnimationTask : BaseTask
{
    public override UniTask Execute()
    {
        AwaitDoneTask(3f);
        return UniTask.WaitUntil(() => doneTask == true);
    }

    private async void AwaitDoneTask(float time)
    {
        await UniTask.WaitForSeconds(time);
        this.doneTask = true;
    }
}