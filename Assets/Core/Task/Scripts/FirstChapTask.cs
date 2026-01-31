using Cysharp.Threading.Tasks;
using UnityEngine;

public class FirstChapTask : BaseTask
{
    public override async UniTask Execute()
    {
        UnityEngine.Debug.Log($"{GetType().Name}: Execute started");
        // Thêm delay nhỏ để đảm bảo async hoạt động
        await UniTask.Yield();
        UnityEngine.Debug.Log($"{GetType().Name}: Execute completed");
    }
}