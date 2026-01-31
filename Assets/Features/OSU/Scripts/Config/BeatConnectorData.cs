using UnityEngine;

/// <summary>
/// SRP: Data for beat connector configuration
/// Responsibility: Store connector visual settings
/// </summary>
[System.Serializable]
public class BeatConnectorData
{
    [Header("Connector Visual")]
    [Tooltip("Sprite dùng để tạo line nối giữa các beat")]
    public Sprite connectorSprite;

    [Tooltip("Kích thước của mỗi sprite unit (width x height)")]
    public Vector2 spriteSize = new Vector2(50f, 10f);

    [Tooltip("Màu của connector")]
    public Color connectorColor = Color.white;

    [Header("Animation Settings")]
    [Tooltip("Tốc độ spawn của connector sprites (sprites per second)")]
    [Range(5f, 50f)]
    public float spawnSpeed = 20f;

    [Tooltip("Thời gian fade in của mỗi sprite")]
    [Range(0.1f, 1f)]
    public float fadeInDuration = 0.2f;

    [Header("Layout")]
    [Tooltip("Overlap giữa các sprite (0 = không overlap, 0.5 = overlap 50%)")]
    [Range(0f, 0.8f)]
    public float overlapFactor = 0.3f;
}