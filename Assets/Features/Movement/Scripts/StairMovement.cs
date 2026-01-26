using UnityEngine;

[CreateAssetMenu(fileName = "Stair_Movement", menuName = "Movement/Stair")]
public class StairMovement : BaseMovement
{
    [SerializeField] private float _stepHeight = 0.5f; // Độ cao mỗi bậc thang
    [SerializeField] private float _stepDepth = 0.3f; // Độ sâu bậc thang (vuông góc với hướng di chuyển)
    [SerializeField] private float _transitionSpeed = 10f; // Tốc độ chuyển đổi giữa các bậc

    private float _currentStepProgress = 0f; // Tiến trình trong bước hiện tại (0->1)
    private Vector3 _stepStartPosition;
    private Vector3 _verticalDirection; // Hướng di chuyển dọc (lên/xuống)
    private Vector3 _perpendicularDirection; // Hướng vuông góc (bậc thang)
    private float _currentStepHeight; // Độ cao thực tế của bước hiện tại
    private bool _isInitialized = false;

    public override void Move(Transform transform, Vector3 destination)
    {
        if (IsArrived(transform, destination)) return;

        // Khởi tạo hoặc bắt đầu bước mới
        if (!_isInitialized || _currentStepProgress >= 1f)
        {
            _stepStartPosition = transform.position;

            Vector3 toDestination = destination - transform.position;

            // Xác định hướng di chuyển dọc (lên hoặc xuống)
            float verticalDistance = toDestination.y;
            _verticalDirection = verticalDistance > 0 ? Vector3.up : Vector3.down;

            // Tính độ cao thực tế của bước (Min của khoảng cách còn lại và step height)
            _currentStepHeight = Mathf.Min(Mathf.Abs(verticalDistance), _stepHeight);

            // Tính hướng vuông góc (bậc thang)
            // Lấy hướng ngang từ vị trí hiện tại đến đích
            Vector3 horizontalToDestination = new Vector3(toDestination.x, 0, toDestination.z);

            if (horizontalToDestination.magnitude > 0.01f)
            {
                _perpendicularDirection = horizontalToDestination.normalized;
            }
            else
            {
                // Nếu không có hướng ngang, dùng forward mặc định
                _perpendicularDirection = Vector3.forward;
            }

            _currentStepProgress = 0f;
            _isInitialized = true;
        }

        // Tính toán tiến trình
        float progressDelta = _transitionSpeed * Time.deltaTime;
        _currentStepProgress += progressDelta;

        // Giới hạn progress
        float clampedProgress = Mathf.Min(_currentStepProgress, 1f);

        // Nội suy vị trí theo đường bậc thang thẳng đứng
        // Giai đoạn 1 (0 -> 0.5): Di chuyển ngang (bậc thang)
        // Giai đoạn 2 (0.5 -> 1): Di chuyển dọc (lên/xuống)
        Vector3 currentPosition;

        if (clampedProgress < 0.5f)
        {
            // Giai đoạn di chuyển ngang (bậc thang)
            float horizontalProgress = clampedProgress * 2f; // Map 0->0.5 thành 0->1
            Vector3 horizontalOffset = _perpendicularDirection * (_stepDepth * horizontalProgress);
            currentPosition = _stepStartPosition + horizontalOffset;
        }
        else
        {
            // Giai đoạn di chuyển dọc (lên/xuống)
            float verticalProgress = (clampedProgress - 0.5f) * 2f; // Map 0.5->1 thành 0->1
            Vector3 horizontalOffset = _perpendicularDirection * _stepDepth;
            Vector3 verticalOffset = _verticalDirection * (_currentStepHeight * verticalProgress);
            currentPosition = _stepStartPosition + horizontalOffset + verticalOffset;
        }

        transform.position = currentPosition;
    }
}
