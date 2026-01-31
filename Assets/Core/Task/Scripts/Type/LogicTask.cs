using Cysharp.Threading.Tasks;
using System.Threading.Tasks;

public class LogicTask : BaseTask
{
    public bool doneTask;
    private void Awake()
    {
        doneTask = false;
    }

    public override async UniTask Execute() 
    {
        await UniTask.WaitUntil(() => doneTask == true);
    }

    public async Task ExecuteAsyncTask(float time)
    {
        await UniTask.WaitForSeconds(time);
        doneTask = true;
    }
}
