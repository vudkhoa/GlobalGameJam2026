using UnityEngine;

[CreateAssetMenu(fileName = "ZigZag_Trajectory", menuName = "Audition/Trajectories/ZigZag")]
public class ZigZagTrajectory : TrajectoryConfig
{
    [Header("ZigZag Settings")]
    [Range(0f, 360f)]
    public float direction = 0f;

    [Tooltip("Độ rộng zigzag (World Unit)")]
    [Range(0f, 5f)]
    public float amplitude = 1.5f;

    [Range(1, 20)]
    public int zigzagCount = 3;

    [Range(0.5f, 5f)]
    public float desiredGap = 1.5f;

    public bool forceFullScreen = false;

    public ZigZagPattern pattern = ZigZagPattern.Sharp;
    public enum ZigZagPattern { Sharp, Smooth }

    public override Vector2 EvaluatePosition(float t, int index, int totalCount)
    {
        // 1. Lấy giới hạn màn hình
        Vector2 bounds = GetDynamicScreenBounds();

        // 2. Tính hướng chính và hướng vuông góc (để zigzag)
        float rad = direction * Mathf.Deg2Rad;
        Vector2 mainDir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        Vector2 perpDir = new Vector2(-mainDir.y, mainDir.x);

        // 3. Tính không gian an toàn (Trừ đi amplitude để đỉnh không bị cắt)
        // Ví dụ: Nếu zigzag theo chiều dọc, ta phải trừ chiều cao đi 1 đoạn bằng amplitude
        float safeX = Mathf.Max(0, bounds.x - Mathf.Abs(perpDir.x * amplitude));
        float safeY = Mathf.Max(0, bounds.y - Mathf.Abs(perpDir.y * amplitude));

        // Tính chiều dài tối đa của trục chính trong vùng an toàn này
        float xLimit = (mainDir.x != 0) ? Mathf.Abs(safeX / mainDir.x) : float.MaxValue;
        float yLimit = (mainDir.y != 0) ? Mathf.Abs(safeY / mainDir.y) : float.MaxValue;
        float maxAllowedLength = Mathf.Min(xLimit, yLimit) * 2f;

        // 4. Auto Fit Gap
        float calculatedGap = desiredGap;
        if (totalCount > 1)
        {
            float desiredLen = (totalCount - 1) * desiredGap;
            if (forceFullScreen || desiredLen > maxAllowedLength)
            {
                calculatedGap = maxAllowedLength / (totalCount - 1);
            }
        }

        // 5. Tính vị trí
        float totalLength = (totalCount - 1) * calculatedGap;
        Vector2 start = -mainDir * (totalLength * 0.5f);
        Vector2 basePos = start + mainDir * (index * calculatedGap);

        // Cộng thêm offset Zigzag
        float progress = totalCount > 1 ? (float)index / (totalCount - 1) : 0.5f;
        float zigzagPhase = progress * zigzagCount;
        float zigzagOffset = 0f;

        if (pattern == ZigZagPattern.Sharp)
            zigzagOffset = (Mathf.PingPong(zigzagPhase * 2f, 2f) - 1f) * amplitude;
        else
            zigzagOffset = Mathf.Sin(zigzagPhase * Mathf.PI * 2f) * amplitude;

        return basePos + perpDir * zigzagOffset;
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        trajectoryName = $"ZigZag {zigzagCount}x (Fit={(forceFullScreen ? "Full" : "Auto")})";
    }
#endif
}