using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class BaseTask : ScriptableObject
{
    public abstract UniTask<bool> Execute(Transform transform, Vector3 position);
}
