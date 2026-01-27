using UnityEngine;

public class RegisterAsPlayer : MonoBehaviour
{
    private void OnEnable()
    {
        GameProvider.Instance.RegisterPlayer(transform);
    }
}