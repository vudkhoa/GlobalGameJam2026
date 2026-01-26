using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class TaskExecutor : MonoBehaviour
{
    [SerializeField] private List<Transform> _waypoints;
    [SerializeField] private List<BaseTask> _tasks;

    private void Start()
    {
        ExecuteTasks().Forget();
    }

    [ContextMenu("Execute Tasks")]
    public async UniTask ExecuteTasks()
    {
        if (_waypoints == null || _waypoints.Count == 0)
        {
            Debug.LogWarning($"{name}: No waypoints assigned.");
            return;
        }

        if (_tasks == null || _tasks.Count == 0)
        {
            Debug.LogWarning($"{name}: No tasks assigned.");
            return;
        }

        for (int i = 0; i < _waypoints.Count; i++)
        {
            var target = _waypoints[i];
            if (target == null) continue;

            // Lấy task tương ứng, nếu hết task thì quay vòng lại từ đầu
            var task = _tasks[i % _tasks.Count];

            if (task != null)
            {
                await task.Execute(transform, target.position);
            }
        }
    }
}
