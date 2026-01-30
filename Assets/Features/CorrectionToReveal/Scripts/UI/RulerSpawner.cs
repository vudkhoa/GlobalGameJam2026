using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages spawning and layout of ruler UI elements
/// Single Responsibility: UI instantiation and positioning
/// </summary>
public class RulerSpawner
{
    private readonly GameObject _rulerPrefab;
    private readonly Transform _parentTransform;
    private readonly float _spacing;

    private readonly List<RulerUI> _spawnedRulers = new List<RulerUI>();

    public RulerSpawner(GameObject rulerPrefab, Transform parentTransform, float spacing)
    {
        _rulerPrefab = rulerPrefab;
        _parentTransform = parentTransform;
        _spacing = spacing;
    }

    /// <summary>
    /// Spawn a ruler UI element
    /// </summary>
    public RulerUI SpawnRuler(string label, float minValue, float maxValue, float currentValue, System.Action<float> onValueChanged)
    {
        GameObject rulerObj = Object.Instantiate(_rulerPrefab, _parentTransform);
        RulerUI ruler = rulerObj.GetComponent<RulerUI>();

        if (ruler == null)
        {
            Debug.LogError("RulerUI component not found on prefab!");
            Object.Destroy(rulerObj);
            return null;
        }

        // Initialize ruler
        ruler.Initialize(label, minValue, maxValue, currentValue, onValueChanged);

        // Position ruler
        PositionRuler(rulerObj.transform, _spawnedRulers.Count);

        _spawnedRulers.Add(ruler);

        return ruler;
    }

    /// <summary>
    /// Position ruler based on index
    /// </summary>
    private void PositionRuler(Transform rulerTransform, int index)
    {
        RectTransform rectTransform = rulerTransform as RectTransform;
        if (rectTransform != null)
        {
            // Position vertically with spacing
            Vector2 anchoredPosition = new Vector2(0, -index * _spacing);
            rectTransform.anchoredPosition = anchoredPosition;
        }
    }

    /// <summary>
    /// Clear all spawned rulers
    /// </summary>
    public void ClearRulers()
    {
        foreach (var ruler in _spawnedRulers)
        {
            if (ruler != null)
            {
                Object.Destroy(ruler.gameObject);
            }
        }

        _spawnedRulers.Clear();
    }

    /// <summary>
    /// Get all spawned rulers
    /// </summary>
    public IReadOnlyList<RulerUI> GetRulers()
    {
        return _spawnedRulers.AsReadOnly();
    }
}
