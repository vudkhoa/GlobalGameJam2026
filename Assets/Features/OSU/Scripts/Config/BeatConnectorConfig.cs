using UnityEngine;

/// <summary>
/// SRP: Store connector line configuration data
/// Responsibility: Hold sprite, animation, fade settings for tiled connectors
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
        
    [Header("Fade Animation Settings")]
    [Tooltip("Tốc độ fade in (world units per second) - tốc độ connector xuất hiện")]
    [Range(1f, 20f)]
    public float fadeInSpeed = 5f;

    [Tooltip("Tốc độ fade out (world units per second) - tốc độ connector biến mất")]
    [Range(1f, 20f)]
    public float fadeOutSpeed = 5f;

    [Tooltip("Độ trễ giữa fade in xong và bắt đầu fade out (seconds)")]
    [Range(0f, 2f)]
    public float fadeOutDelay = 0.3f;

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
        fadeInSpeed = Mathf.Max(1f, fadeInSpeed);
        fadeOutSpeed = Mathf.Max(1f, fadeOutSpeed);
    }
#endif
}