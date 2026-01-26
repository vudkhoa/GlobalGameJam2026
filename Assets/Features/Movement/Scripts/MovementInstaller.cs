using UnityEngine;

public class MovementInstaller : MonoBehaviour
{
    [SerializeField] private BaseMovement _movement;
    [SerializeField] private Transform _destination;

    private void Update()
    {
        if (_movement != null && _destination != null)
        {
            _movement.Move(transform, _destination.position);
        }
    }
}
