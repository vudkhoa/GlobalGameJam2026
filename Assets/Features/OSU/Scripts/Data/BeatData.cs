using UnityEngine;

/// <summary>
/// SRP: Store PURE beat data
/// Responsibility: Hold timing + position + size
/// </summary>
[System.Serializable]
public struct BeatData
{
    public float time;       // Thời điểm xuất hiện (giây)
    public Vector2 position; // Vị trí trên màn hình (local)
    public Vector2 size;     // Kích thước beat (sizeDelta)
}