using UnityEngine;

[CreateAssetMenu(fileName = "Wave_Trajectory", menuName = "Audition/Trajectories/Wave")]
public class WaveTrajectory : TrajectoryConfig
{
    [Header("Wave Settings")]
    public bool moveRight = true;

    [Tooltip("Khoảng cách mong muốn giữa các beat")]
    [Range(0.5f, 5f)]
    public float desiredGap = 1.5f;

    [Tooltip("Ép trải dài hết chiều ngang màn hình")]
    public bool forceFullScreen = false;

    [Tooltip("Chiều cao sóng (World Unit)")]
    [Range(0f, 5f)]
    public float waveAmplitude = 1.5f;

    [Range(0.1f, 5f)]
    public float waveFrequency = 1f;

    [Header("Randomness")]
    public int pathSeed = 0;
    [Range(0f, 1f)]
    public float randomness = 0.3f;

    public override Vector2 EvaluatePosition(float t, int index, int totalCount)
    {
        Random.State oldState = Random.state;
        if (pathSeed != 0) Random.InitState(pathSeed + index);

        // 1. Lấy giới hạn màn hình
        Vector2 bounds = GetDynamicScreenBounds();

        // Giới hạn chiều ngang
        float maxAllowedWidth = bounds.x * 2f;

        // 2. Tính toán Gap (Auto Fit chiều ngang)
        float calculatedGap = desiredGap;
        if (totalCount > 1)
        {
            float desiredLength = (totalCount - 1) * desiredGap;
            if (forceFullScreen || desiredLength > maxAllowedWidth)
            {
                calculatedGap = maxAllowedWidth / (totalCount - 1);
            }
        }

        // 3. Tính X (Dàn đều ngang)
        float totalLength = (totalCount - 1) * calculatedGap;
        float xOffset = index * calculatedGap;
        float xPos = moveRight ? (-totalLength * 0.5f + xOffset) : (totalLength * 0.5f - xOffset);

        // 4. Tính Y (Sóng + Random) -> Tự động kẹp trong chiều cao màn hình
        // Đảm bảo sóng không cao quá màn hình
        float safeAmplitude = Mathf.Min(waveAmplitude, bounds.y * 0.9f);

        float progress = totalCount > 1 ? (float)index / (totalCount - 1) : 0.5f;
        float baseWave = Mathf.Sin(progress * waveFrequency * Mathf.PI * 2f) * safeAmplitude;

        float noise = Mathf.PerlinNoise(index * 0.5f, pathSeed * 0.1f);
        float randomVal = (noise - 0.5f) * safeAmplitude * randomness;

        float yPos = baseWave + randomVal;

        // Clamp lại lần cuối để chắc chắn không bay ra ngoài (đặc biệt là Y)
        yPos = Mathf.Clamp(yPos, -bounds.y, bounds.y);

        Random.state = oldState;
        return new Vector2(xPos, yPos);
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        trajectoryName = $"Wave {(moveRight ? "Right" : "Left")} (Fit={(forceFullScreen ? "Full" : "Auto")})";
    }
#endif
}