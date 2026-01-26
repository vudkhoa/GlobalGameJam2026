using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "Sequence_Task", menuName = "Tasks/Sequence")]
public class SequenceTasks : BaseTask
{
    [SerializeField] private List<BaseTask> _tasks;

    public override async UniTask<bool> Execute(Transform transform, Vector3 position)
    {
        if (_tasks == null || _tasks.Count == 0) return true;

        foreach (var task in _tasks)
        {
            if (task == null) continue;

            bool success = await task.Execute(transform, position);
            if (!success)
            {
                return false;
            }
        }

        return true;
    }
}
