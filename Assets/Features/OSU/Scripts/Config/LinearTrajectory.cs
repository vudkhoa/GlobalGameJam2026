using UnityEngine;

[CreateAssetMenu(fileName = "Linear_Trajectory", menuName = "Audition/Trajectories/Linear")]
public class LinearTrajectory : TrajectoryConfig
{
    [Header("Linear Settings")]
    [Tooltip("Góc của đường thẳng (0° = phải, 90° = lên, 180° = trái)")]
    [Range(0f, 360f)]
    public float angle = 0f;

    [Tooltip("Khoảng cách mong muốn giữa các beat (World Unit). Sẽ tự co lại nếu quá dài.")]
    [Range(0.5f, 5f)]
    public float desiredGap = 1.5f;

    [Tooltip("Nếu bật: Tự động giãn khoảng cách để đường thẳng chạm 2 mép màn hình")]
    public bool forceFullScreen = false;

    public override Vector2 EvaluatePosition(float t, int index, int totalCount)
    {
        // 1. Lấy giới hạn màn hình thực tế
        Vector2 bounds = GetDynamicScreenBounds();
        float dynamicMaxX = bounds.x;
        float dynamicMaxY = bounds.y;

        // 2. Tính hướng
        float rad = angle * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

        // 3. Tính toán chiều dài tối đa cho phép trong cái hộp màn hình này
        // Công thức: Tìm giao điểm của tia (direction) với hình chữ nhật (bounds)
        float xLimit = (direction.x != 0) ? Mathf.Abs(dynamicMaxX / direction.x) : float.MaxValue;
        float yLimit = (direction.y != 0) ? Mathf.Abs(dynamicMaxY / direction.y) : float.MaxValue;

        // Đường kính tối đa (từ tâm ra 2 phía)
        float maxAllowedLength = Mathf.Min(xLimit, yLimit) * 2f;

        // 4. Tính toán Gap thực tế (Auto Fit)
        float calculatedGap = desiredGap;

        if (totalCount > 1)
        {
            float desiredTotalLength = (totalCount - 1) * desiredGap;

            if (forceFullScreen)
            {
                // Ép full screen: Chia đều độ dài tối đa
                calculatedGap = maxAllowedLength / (totalCount - 1);
            }
            else if (desiredTotalLength > maxAllowedLength)
            {
                // Nếu dài quá màn hình: Co Gap lại cho vừa khít
                calculatedGap = maxAllowedLength / (totalCount - 1);
            }
        }

        // 5. Tính vị trí cuối cùng
        float totalLength = (totalCount - 1) * calculatedGap;

        // Bắt đầu từ nửa bên này, đi về phía bên kia để tâm đường thẳng trùng tâm màn hình (0,0)
        Vector2 start = -direction * (totalLength * 0.5f);
        Vector2 position = start + direction * (index * calculatedGap);

        return position;
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        trajectoryName = $"Linear {angle:F0}° (Fit={(forceFullScreen ? "Full" : "Auto")})";
    }
#endif
}