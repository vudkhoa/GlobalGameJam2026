using Cysharp.Threading.Tasks;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    [SerializeField] private BaseTask[] _tasks;

    private void OnEnable() => ExecuteTasksAsync().Forget();

    private async UniTask ExecuteTasksAsync()
    {
        foreach (var task in _tasks)
        {
            await task.Execute(); 
        }
    }
}