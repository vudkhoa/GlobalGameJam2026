using Cysharp.Threading.Tasks;
using System.Collections.Generic;

public class FourthChapter : BaseTask
{
    public List<BaseTask> tasks;

    public override async UniTask Execute()
    {
        List<UniTask> taskList = new List<UniTask>();
        foreach (BaseTask task in tasks)
        {
            taskList.Add(UniTask.WaitUntil(() => task.doneTask));
        }
        await UniTask.WhenAll(taskList);
    }
}
