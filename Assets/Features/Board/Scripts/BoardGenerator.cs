using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Board_Generator", menuName = "Features/Board/Board_Generator")]
public class BoardGenerator : ScriptableObject
{
    [SerializeField] private int _width;
    [SerializeField] private int _height;
    [SerializeField] private Transform _prefab;

    public List<Transform> GenerateBoard(Transform parent)
    {
        List<Transform> board = new List<Transform>();

        var renderer = _prefab.GetComponentInChildren<Renderer>();
        Vector3 spacing = renderer == null ? Vector3.one : renderer.bounds.size;

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                var tile = Instantiate(_prefab, Helper.CalculateCenterPosition(x, y, _width, _height, spacing), Quaternion.identity);
                tile.SetParent(parent);

                board.Add(tile);
            }
        }

        return board;
    }
}