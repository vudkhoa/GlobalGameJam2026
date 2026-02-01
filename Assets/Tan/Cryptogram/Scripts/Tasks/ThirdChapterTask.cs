using Cysharp.Threading.Tasks;
using System.Collections.Generic;

public class ThirdChapterTask : BaseTask
{
    public List<BaseTask> tasks;

    public override async UniTask Execute()
    {
        foreach (var task in tasks)
        {
            if (task == null) continue;
            task.doneTask = false;
            task.Execute().Forget();
            await UniTask.WaitUntil(() => task.doneTask == true);
        }
        this.doneTask = true;
    }
}