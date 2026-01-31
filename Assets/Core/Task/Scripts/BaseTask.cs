using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class BaseTask : MonoBehaviour
{
    public bool doneTask;

    private void Awake()
    {
        doneTask = false;
    }

    public void CompletedTask() => doneTask = true;

    public abstract UniTask Execute();
}