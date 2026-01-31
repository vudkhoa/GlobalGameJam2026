using UnityEngine;

/// <summary>
/// SRP: Store connector line configuration data
/// Responsibility: Hold sprite, size, spacing, animation settings
/// </summary>
[CreateAssetMenu(fileName = "ConnectorConfig", menuName = "OSU/Beat Connector Config")]
public class BeatConnectorConfig : ScriptableObject
{
    [Header("Connector Sprite")]
    [Tooltip("Sprite để làm connector (dot, circle, square, arrow,...)")]
    public Sprite connectorSprite;

    [Header("Visual Settings")]
    [Tooltip("Kích thước của mỗi connector sprite")]
    public Vector2 spriteSize = new Vector2(8f, 8f);

    [Tooltip("Spacing giữa các sprites (pixels)")]
    [Range(0f, 50f)]
    public float spriteSpacing = 10f;

    [Tooltip("Màu của connector")]
    public Color connectorColor = Color.white;

    [Header("Animation Settings")]
    [Tooltip("Tốc độ animation (sprites/second)")]
    [Range(1f, 50f)]
    public float animationSpeed = 10f;

    [Tooltip("Loại animation")]
    public ConnectorAnimationType animationType = ConnectorAnimationType.Sequential;

    [Header("Fade Settings")]
    [Tooltip("Fade in tại đầu line (0 = no fade)")]
    [Range(0f, 1f)]
    public float fadeInRatio = 0.1f;

    [Tooltip("Fade out tại cuối line (0 = no fade)")]
    [Range(0f, 1f)]
    public float fadeOutRatio = 0.1f;

#if UNITY_EDITOR
    private void OnValidate()
    {
        spriteSize.x = Mathf.Max(1f, spriteSize.x);
        spriteSize.y = Mathf.Max(1f, spriteSize.y);
        spriteSpacing = Mathf.Max(0f, spriteSpacing);
    }
#endif
}

/// <summary>
/// Animation types for connector
/// </summary>
public enum ConnectorAnimationType
{
    Sequential,  // Sprites xuất hiện tuần tự từ A → B
    Wave,        // Wave effect
    Pulse        // Pulse effect
}