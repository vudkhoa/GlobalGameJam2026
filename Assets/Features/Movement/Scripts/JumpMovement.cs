using UnityEngine;

[CreateAssetMenu(fileName = "Jump_Movement", menuName = "Movement/Jump")]
public class JumpMovement : BaseMovement
{
    [SerializeField] private float _jumpHeight = 2f;
    [SerializeField] private float _stepDistance = 2f; // Khoảng cách mỗi bước nhảy

    private float _currentStepProgress = 0f; // Tiến trình trong bước nhảy hiện tại (0->1)
    private Vector3 _stepStartPosition;
    private Vector3 _stepDirection; // Hướng cố định cho bước nhảy hiện tại
    private Vector3 _jumpDirection; // Hướng nhảy (vuông góc) cố định cho bước
    private float _currentStepDistance; // Khoảng cách thực tế của bước hiện tại
    private bool _isInitialized = false;

    public override void Move(Transform transform, Vector3 destination)
    {
        if (IsArrived(transform, destination)) return;

        // Khởi tạo hoặc bắt đầu bước nhảy mới
        if (!_isInitialized || _currentStepProgress >= 1f)
        {
            _stepStartPosition = transform.position;
            _stepDirection = (destination - transform.position).normalized;

            // Tính khoảng cách còn lại đến đích
            float remainingDistance = Vector3.Distance(transform.position, destination);

            // Sử dụng Min của khoảng cách còn lại và step distance
            _currentStepDistance = Mathf.Min(remainingDistance, _stepDistance);

            // Tính toán hướng nhảy (vuông góc với hướng di chuyển)
            if (Mathf.Abs(Vector3.Dot(_stepDirection, Vector3.up)) > 0.99f)
            {
                // Nếu di chuyển theo chiều dọc, nhảy theo chiều ngang
                _jumpDirection = Vector3.right;
            }
            else
            {
                // Nếu di chuyển theo chiều ngang, nhảy theo chiều dọc (vuông góc với hướng di chuyển)
                _jumpDirection = Vector3.ProjectOnPlane(Vector3.up, _stepDirection).normalized;
            }

            _currentStepProgress = 0f;
            _isInitialized = true;
        }

        // Tính toán tiến trình trong bước nhảy
        float moveDistance = _speed * Time.deltaTime;
        _currentStepProgress += moveDistance / _currentStepDistance;

        // Giới hạn progress không vượt quá 1 (sẽ reset ở frame tiếp theo)
        float clampedProgress = Mathf.Min(_currentStepProgress, 1f);

        // Tính toán vị trí cơ bản dọc theo hướng di chuyển
        Vector3 basePosition = _stepStartPosition + _stepDirection * (clampedProgress * _currentStepDistance);

        // Tính toán độ cao theo parabola: h(t) = 4 * height * t * (1 - t)
        // Công thức parabola đỉnh tại t=0.5, bắt đầu và kết thúc tại h=0
        float parabolaHeight = 4f * _jumpHeight * clampedProgress * (1f - clampedProgress);

        // Vị trí cuối cùng = vị trí cơ bản + offset parabola
        transform.position = basePosition + _jumpDirection * parabolaHeight;
    }
}
