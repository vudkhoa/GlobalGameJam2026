using UnityEngine;

[ExecuteAlways] // Chạy ngay cả trong Edit Mode để bạn thấy kết quả liền
public class ScaleStabilizer : MonoBehaviour
{
    private void LateUpdate()
    {
        if (transform.parent != null)
        {
            // Lấy Scale toàn cầu của cha
            Vector3 parentScale = transform.parent.lossyScale;

            // Tính toán Scale nghịch đảo để giữ cho object này luôn là (1, 1, 1) ngoài thế giới
            // Công thức: LocalScale = DesiredWorldScale / ParentWorldScale
            // Ở đây DesiredWorldScale mình muốn là 1
            transform.localScale = new Vector3(
                1f / parentScale.x,
                1f / parentScale.y,
                1f / parentScale.z
            );
        }
    }
}