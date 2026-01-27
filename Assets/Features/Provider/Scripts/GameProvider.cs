using UnityEngine;

public class GameProvider : MonoSingleton<GameProvider>
{
    [SerializeField] private Transform _player;

    public Transform Player => _player;

    public void RegisterPlayer(Transform player) => _player = player;
}
