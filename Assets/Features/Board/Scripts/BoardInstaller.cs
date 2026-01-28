using UnityEngine;

public class BoardInstaller : MonoBehaviour
{
    [SerializeField] private BoardGenerator _boardGenerator;

    private void OnEnable()
    {
        _boardGenerator.GenerateBoard(this.transform);
    }
}