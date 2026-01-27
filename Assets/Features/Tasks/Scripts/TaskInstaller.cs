using System.Collections.Generic;
using UnityEngine;

public class TaskInstaller : MonoBehaviour
{
    [SerializeField] private List<BaseTask> _tasks;
    public List<BaseTask> Tasks => _tasks;
}