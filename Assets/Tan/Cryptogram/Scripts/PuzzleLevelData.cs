using UnityEngine;

[CreateAssetMenu(menuName = "Florence/LevelData")]
public class PuzzleLevelData : ScriptableObject
{
    [Header("Nội dung (Bọc từ cần ẩn trong {})")]
    [TextArea(3, 5)]
    public string sentence = "Hôm nay {tôi} cảm thấy {rất} buồn.";

    [Header("Cấu hình")]
    public float typingSpeed = 0.05f;
    public PuzzlePhase phaseType = PuzzlePhase.Normal;
}

public enum PuzzlePhase { Normal, Glitch }