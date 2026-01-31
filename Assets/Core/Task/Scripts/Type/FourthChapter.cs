using Cysharp.Threading.Tasks;
using System.Collections.Generic;

public class FourthChapter : BaseTask
{
    public List<BaseTask> tasks;

    public override async UniTask Execute()
    {
        foreach (var task in tasks)
        {
            if (task == null) continue;
            await task.Execute();
        }
    }
}
