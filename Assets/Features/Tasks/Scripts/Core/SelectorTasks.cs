using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "Selector_Task", menuName = "Tasks/Selector")]
public class SelectorTasks : BaseTask
{
    [SerializeField] private List<BaseTask> _tasks;

    public override async UniTask<bool> Execute(Transform transform, Vector3 position)
    {
        if (_tasks == null || _tasks.Count == 0) return false;

        foreach (var task in _tasks)
        {
            if (task == null) continue;

            bool success = await task.Execute(transform, position);
            if (success)
            {
                return true;
            }
        }

        return false;
    }
}
