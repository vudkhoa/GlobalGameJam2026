using UnityEngine;

public class TapInput : MonoBehaviour
{
    [SerializeField] private TapMechanic mechanic;

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) 
        {
            if (mechanic != null)
            {
                mechanic.AddPressure();
            }
        }
    }
}