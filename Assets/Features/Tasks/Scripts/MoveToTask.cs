using Cysharp.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "MoveTo_Task", menuName = "Tasks/MoveTo")]
public class MoveToTask : BaseTask
{
    [SerializeField] private BaseMovement _movement;

    public override async UniTask<bool> Execute(Transform transform, Vector3 position)
    {
        if (_movement == null) return false;

        // Thực hiện di chuyển cho đến khi đến đích
        while (!_movement.IsArrived(transform, position))
        {
            _movement.Move(transform, position);
            await UniTask.Yield();
        }

        return true;
    }
}
