using UnityEngine;

[CreateAssetMenu(fileName = "LinearMovement", menuName = "Movement/Linear")]
public class LinearMovement : BaseMovement
{
    public override void Move(Transform transform, Vector3 destination)
    {
        if (IsArrived(transform, destination)) return;

        transform.position = Vector3.MoveTowards(transform.position, destination, _speed * Time.deltaTime);
    }
}
