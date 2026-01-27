using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class TaskExecutor : MonoBehaviour
{
    private TaskInstaller[] _taskInstallers;
    [SerializeField] private List<BaseTask> _tasks;

    private void OnEnable()
    {
        Debug.Log($"[{nameof(TaskExecutor)}] OnEnable called on {name}");
        _taskInstallers = GetComponentsInChildren<TaskInstaller>();
        Debug.Log($"[{nameof(TaskExecutor)}] Found {_taskInstallers.Length} TaskInstaller(s)");

        _tasks = new List<BaseTask>();

        foreach (var installer in _taskInstallers)
        {
            if (installer.Tasks == null) continue;
            // if (!installer.gameObject.activeInHierarchy) continue; // GetComponentsInChildren already checks active by default unless includeInactive is true

            foreach (var task in installer.Tasks)
            {
                _tasks.Add(task);
            }
        }
        Debug.Log($"[{nameof(TaskExecutor)}] Registered {_tasks.Count} tasks");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Execute Tasks");
            ExecuteTasks().Forget();
        }
    }

    [ContextMenu("Execute Tasks")]
    public async UniTask ExecuteTasks()
    {
        Transform player = GameProvider.Instance.Player;

        if (_taskInstallers == null || _taskInstallers.Length == 0)
        {
            Debug.LogWarning($"{name}: No task installers assigned.");
            return;
        }

        if (_tasks == null || _tasks.Count == 0)
        {
            Debug.LogWarning($"{name}: No tasks assigned.");
            return;
        }

        for (int i = 0; i < _taskInstallers.Length; i++)
        {
            var target = _taskInstallers[i];
            if (target == null) continue;

            var task = _tasks[i % _tasks.Count];

            if (task != null)
            {
                await task.Execute(player, target.transform.position);
            }
        }
    }
}
