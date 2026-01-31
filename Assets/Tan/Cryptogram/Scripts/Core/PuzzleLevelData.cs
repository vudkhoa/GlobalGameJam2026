using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct CharID
{
    public string character; 
    public int id;          
}

[CreateAssetMenu(menuName = "Florence/LevelData")]
public class PuzzleLevelData : ScriptableObject
{
    [Header("Nội dung (Bọc từ cần ẩn trong {})")]
    [TextArea(3, 5)]
    public string sentence = "Hôm nay {tôi} cảm thấy {rất} buồn.";

    [Header("Cấu hình")]
    public float typingSpeed = 0.05f;
    public PuzzlePhase phaseType = PuzzlePhase.Normal;

    [Header("Cấu hình số thủ công (Tùy chọn)")]
    [Tooltip("Nếu để trống, game sẽ tự sinh số. Nếu điền, game sẽ dùng số này.")]
    public List<CharID> manualMapping;

    
}

public enum PuzzlePhase { Normal, Glitch }