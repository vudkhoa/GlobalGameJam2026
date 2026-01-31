using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public abstract class BaseTask : MonoBehaviour
{
    public abstract UniTask Execute();
}