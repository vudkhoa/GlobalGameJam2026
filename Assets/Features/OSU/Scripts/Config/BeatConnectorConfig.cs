using UnityEngine;

/// <summary>
/// SRP: Store connector line visual configuration ONLY
/// Responsibility: Hold sprite, visual settings for tiled connectors
/// NOTE: Timing auto-sync from BeatConfig at runtime, no manual input needed
/// </summary>
[CreateAssetMenu(fileName = "ConnectorConfig", menuName = "OSU/Beat Connector Config")]
public class BeatConnectorConfig : ScriptableObject
{
    [Header("Connector Sprite")]
    [Tooltip("Sprite để làm connector line (sẽ được tile dọc theo đường nối)")]
    public Sprite connectorSprite;

    [Header("Visual Settings")]
    [Tooltip("Chiều rộng của connector line (world units)")]
    [Range(0.05f, 1f)]
    public float lineWidth = 0.1f;

    [Tooltip("Tile count per world unit (density của sprite)")]
    [Range(1f, 20f)]
    public float tilesPerUnit = 5f;

    [Tooltip("Màu của connector")]
    public Color connectorColor = Color.white;

    [Header("Sorting Layer")]
    [Tooltip("Sorting layer name cho connector (nên thấp hơn beats)")]
    public string sortingLayerName = "Gameplay";

    [Tooltip("Sorting order (nên thấp hơn beats để không che mất)")]
    public int sortingOrder = 0;

#if UNITY_EDITOR
    private void OnValidate()
    {
        lineWidth = Mathf.Max(0.05f, lineWidth);
        tilesPerUnit = Mathf.Max(1f, tilesPerUnit);
    }
#endif
}