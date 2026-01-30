using UnityEngine;

/// <summary>
/// SRP: Store PURE beat data
/// Responsibility: Hold timing + position only
/// </summary>
[System.Serializable]
public class BeatData
{
    public float time;       // Thời điểm xuất hiện (giây)
    public Vector2 position; // Vị trí trên màn hình (local)
}