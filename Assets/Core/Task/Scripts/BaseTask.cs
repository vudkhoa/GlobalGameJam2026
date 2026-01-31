using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public abstract class BaseTask : MonoBehaviour
{
    public bool doneTask;

    private void Awake()
    {
        doneTask = false;
    }

    public abstract UniTask Execute();
}