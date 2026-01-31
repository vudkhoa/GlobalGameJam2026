using UnityEngine;

/// <summary>
/// SRP: Store PURE beat data
/// Responsibility: Hold timing + position + size + sprites
/// ✅ Data-driven: sprite set passed from trajectory config
/// </summary>
[System.Serializable]
public struct BeatData
{
    public float time;       // Thời điểm xuất hiện (giây)
    public Vector2 position; // Vị trí trên màn hình (local)
    public Vector2 size;     // Kích thước beat (sizeDelta)

    // ✅ Sprite set for this beat (passed from trajectory config)
    public BeatSpriteSet spriteSet; // Reference to sprite set (can be null to use default)
}


