using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class BaseTask : MonoBehaviour
{
    public abstract UniTask<bool> Execute();
}