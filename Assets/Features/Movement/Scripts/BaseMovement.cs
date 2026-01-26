using UnityEngine;

public abstract class BaseMovement : ScriptableObject
{
    private const float ARRIVED_DISTANCE = 0.1f;
    [SerializeField] protected float _speed = 5f;
    public abstract void Move(Transform transform, Vector3 destination);

    public bool IsArrived(Transform transform, Vector3 destination) => Vector3.Distance(transform.position, destination) < ARRIVED_DISTANCE;
}
