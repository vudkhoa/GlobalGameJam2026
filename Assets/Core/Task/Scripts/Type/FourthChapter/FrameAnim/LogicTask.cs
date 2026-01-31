using Cysharp.Threading.Tasks;
using System.Threading.Tasks;

public class LogicTask : BaseTask
{
    public override UniTask Execute() 
    {
        return UniTask.WaitUntil(() => doneTask == true);
    }

    public async Task ExecuteAsyncTask(float time)
    {
        await UniTask.WaitForSeconds(time);
        doneTask = true;
    }
}
