using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Manages spawning and layout of ruler UI elements
/// Single Responsibility: UI instantiation and positioning
/// </summary>
public class RulerSpawner
{
    private GameObject _rulerPrefab; // Mutable if needed? No, kept constant usually
    private Transform _parentTransform;
    private float _spacing;

    private readonly List<RulerUI> _spawnedRulers = new List<RulerUI>();

    public RulerSpawner(GameObject rulerPrefab, Transform parentTransform, float spacing)
    {
        _rulerPrefab = rulerPrefab;
        _parentTransform = parentTransform;
        _spacing = spacing;
        _spacing = spacing;
    }

    public void SetSpacing(float spacing)
    {
        _spacing = spacing;
    }

    // Pooling
    private readonly Queue<RulerUI> _rulerPool = new Queue<RulerUI>();

    /// <summary>
    /// Smartly repopulate rulers based on new parameters (Reuse objects)
    /// </summary>
    public void RepopulateRulers(List<ShaderParameter> parameters, System.Action<string, float> onValueChangedCallback)
    {
        // 1. Return all active rulers to pool
        foreach (var ruler in _spawnedRulers)
        {
            ruler.gameObject.SetActive(false);
            _rulerPool.Enqueue(ruler);
        }
        _spawnedRulers.Clear();

        // 2. Spawn or Reuse for new parameters
        for (int i = 0; i < parameters.Count; i++)
        {
            var param = parameters[i];
            RulerUI ruler = GetRuler();

            // Initialize
            ruler.Initialize(
                param.DisplayName,
                param.MinValue,
                param.MaxValue,
                param.CurrentValue,
                (val) => onValueChangedCallback?.Invoke(param.PropertyName, val)
            );

            // Position
            PositionRuler(ruler.transform, i);
            _spawnedRulers.Add(ruler);
        }
    }

    private RulerUI GetRuler()
    {
        RulerUI ruler;
        if (_rulerPool.Count > 0)
        {
            ruler = _rulerPool.Dequeue();
            ruler.gameObject.SetActive(true);
        }
        else
        {
            GameObject obj = Object.Instantiate(_rulerPrefab, _parentTransform);
            ruler = obj.GetComponent<RulerUI>();
        }
        return ruler;
    }

    // Removed legacy SpawnRuler and ClearRulers in favor of RepopulateRulers pattern

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
    /// Get all spawned rulers
    /// </summary>
    public IReadOnlyList<RulerUI> GetRulers()
    {
        return _spawnedRulers.AsReadOnly();
    }

    /// <summary>
    /// Enable or disable interaction for all spawned rulers
    /// </summary>
    public void SetRulersInteractable(bool interactable)
    {
        foreach (var ruler in _spawnedRulers)
        {
            if (ruler != null)
            {
                ruler.SetInteractable(interactable);
            }
        }
    }
}
