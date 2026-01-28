using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "DiceVisualConfig", menuName = "Features/Dice/VisualConfig")]
public class DiceVisualConfig : ScriptableObject
{
    [Tooltip("List of sprites for each dice face. Index 0 corresponds to Value 1, Index 1 to Value 2, etc.")]
    [SerializeField] private List<Sprite> _faceSprites = new();

    public Sprite GetFaceSprite(int value)
    {
        // Adjust for 0-based indexing (Value 1 -> Index 0)
        int index = value - 1;

        if (index < 0 || index >= _faceSprites.Count)
        {
            Debug.LogWarning($"DiceVisualConfig: No sprite found for value {value}. Returning null.");
            return null;
        }

        return _faceSprites[index];
    }
}
