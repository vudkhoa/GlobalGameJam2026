using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Florence/LevelData")]
public class PuzzleLevelData : ScriptableObject
{
    [Header("Nội dung (Bọc từ cần ẩn trong {})")]
    [TextArea(3, 5)]
    public string sentence = "Hôm nay {tôi} cảm thấy {rất} buồn.";

    [Header("Cấu hình")]
    public float typingSpeed = 0.05f;
    public PuzzlePhase phaseType = PuzzlePhase.Normal;

    [Header("Cấu hình Tráo Đổi (Tùy chọn)")]
    [Tooltip("Viết các từ có thể đổi chỗ vào cùng 1 dòng, cách nhau bằng dấu phẩy.\nVí dụ: mệt mỏi,trống rỗng")]
    public List<string> interchangeableGroups;
}

public enum PuzzlePhase { Normal, Glitch }