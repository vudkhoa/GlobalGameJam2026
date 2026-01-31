using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragComponentView : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Image Image;
    [SerializeField] private RectTransform rectTransform; 


    private float returnTimeout;
    private float returnSpeed;

    private Vector2 _distance;
    private bool _isMoving;
    public Vector3 firstPos;
    public bool isPointDown;

    private float _timeWithoutPointerDown;
    private Tween _moveTween;
    private Tween _shakeTween;
    private Vector2 _currentDirection;
    private DragComponentController _controller;
    private Canvas _canvas;
    private bool _hasBeenInteracted = false; // Flag để check đã tương tác chưa
    private float _shakeStrength;
    private float _shakeDuration;

    private void Start()
    {
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }
        _controller = DragComponentController.Instance;
        
        _isMoving = false;
        isPointDown = false;
        _timeWithoutPointerDown = 0f;
        firstPos = gameObject.transform.position;
        returnTimeout = DragComponentController.Instance.AwaitDuration;
        returnSpeed = DragComponentController.Instance.MoveSpeed;
        _shakeStrength = DragComponentController.Instance.ShakeStrength;
        _shakeDuration = DragComponentController.Instance.ShakeDuration;
        _canvas = DragComponentController.Instance.CanvasRef;
    }

    private void Update()
    {
        // Nếu đã Win, không cho di chuyển nữa
        if (_controller != null && _controller.HasWon)
        {
            // Hủy tween nếu đang di chuyển
            if (_moveTween != null)
            {
                _moveTween.Kill();
                _moveTween = null;
                _isMoving = false;
            }
            
            // Dừng shake khi Win
            StopShake();
            return;
        }

        if (_isMoving)
        {
            return;
        }

        // Chỉ bắt đầu timer khi đã tương tác
        if (!_hasBeenInteracted)
        {
            return;
        }

        // Nếu không có pointer down, tăng timer
        if (!isPointDown)
        {
            _timeWithoutPointerDown += Time.deltaTime;

            // Nếu đã vượt quá thời gian timeout và chưa có tween đang chạy
            if (_timeWithoutPointerDown >= returnTimeout && _moveTween == null)
            {
                MoveToRandomEdge();
            }
        }
        else
        {
            // Reset timer khi pointer down lại
            _timeWithoutPointerDown = 0f;
            
            // Hủy tween nếu đang di chuyển
            if (_moveTween != null)
            {
                _moveTween.Kill();
                _moveTween = null;
                _isMoving = false;
            }
            
            // Dừng shake khi user tương tác
            StopShake();
        }
    }

    /// <summary>
    /// Random vector direction và di chuyển đến biên (ưu tiên biên gần nhất)
    /// </summary>
    private void MoveToRandomEdge()
    {
        if (rectTransform == null || _canvas == null)
        {
            return;
        }

        Vector2 currentPos = rectTransform.position;
        
        // Tính direction đến biên gần nhất
        _currentDirection = GetDirectionToNearestEdge(currentPos);
        
        // Thêm một chút random để tạo variation (70% hướng về biên gần nhất, 30% random)
        if (UnityEngine.Random.value > 0.3f)
        {
            // Giữ direction đến biên gần nhất
        }
        else
        {
            // Random direction nhưng vẫn hướng ra ngoài
            Vector2 randomDir = UnityEngine.Random.insideUnitCircle.normalized;
            Vector2 screenCenter = GetScreenCenter();
            Vector2 toCenter = (screenCenter - currentPos).normalized;
            
            if (Vector2.Dot(randomDir, toCenter) > 0)
            {
                randomDir = -randomDir;
            }
            
            // Blend giữa direction đến biên gần nhất và random direction
            _currentDirection = Vector2.Lerp(_currentDirection, randomDir, 0.5f).normalized;
        }

        Vector2 targetEdge = GetEdgePosition(currentPos, _currentDirection);
        float distance = Vector2.Distance(currentPos, targetEdge);
        float duration = distance / returnSpeed;

        _isMoving = true;
        _moveTween = rectTransform.DOMove(targetEdge, duration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                rectTransform.position = targetEdge;
                // Dừng shake khi đến đích
                StopShake();
                // Khi đụng biên, random vector mới đi về biên bất kỳ
                MoveToRandomEdgeFromEdge();
            });

        // Bắt đầu hiệu ứng lắc nhẹ khi di chuyển (sau khi move tween đã được tạo)
        StartShake();
    }

    /// <summary>
    /// Khi đụng biên, random vector mới đi về biên bất kỳ (ưu tiên biên gần nhất)
    /// </summary>
    private void MoveToRandomEdgeFromEdge()
    {
        if (rectTransform == null || _canvas == null)
        {
            _isMoving = false;
            _moveTween = null;
            return;
        }

        Vector2 currentPos = rectTransform.position;
        
        // Tính direction đến biên gần nhất (70% hướng về biên gần nhất, 30% random)
        if (UnityEngine.Random.value > 0.3f)
        {
            _currentDirection = GetDirectionToNearestEdge(currentPos);
        }
        else
        {
            // Random direction
            _currentDirection = UnityEngine.Random.insideUnitCircle.normalized;
        }
        
        Vector2 targetEdge = GetEdgePosition(currentPos, _currentDirection);
        float distance = Vector2.Distance(currentPos, targetEdge);
        float duration = distance / returnSpeed;

        _moveTween = rectTransform.DOMove(targetEdge, duration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                rectTransform.position = targetEdge;
                // Dừng shake khi đến đích
                StopShake();
                // Tiếp tục di chuyển đến biên khác
                MoveToRandomEdgeFromEdge();
            });

        // Tiếp tục shake khi di chuyển
        StartShake();
    }

    /// <summary>
    /// Tính toán vị trí trên biên dựa trên direction và 4 điểm góc biên
    /// </summary>
    private Vector2 GetEdgePosition(Vector2 startPos, Vector2 direction)
    {
        if (_controller == null)
        {
            return startPos;
        }

        // Lấy vị trí world của 4 góc biên
        Vector2 topLeft = _controller.TopLeftCorner != null ? (Vector2)_controller.TopLeftCorner.position : Vector2.zero;
        Vector2 topRight = _controller.TopRightCorner != null ? (Vector2)_controller.TopRightCorner.position : Vector2.zero;
        Vector2 bottomLeft = _controller.BottomLeftCorner != null ? (Vector2)_controller.BottomLeftCorner.position : Vector2.zero;
        Vector2 bottomRight = _controller.BottomRightCorner != null ? (Vector2)_controller.BottomRightCorner.position : Vector2.zero;

        // Tính toán biên từ 4 điểm
        float left = Mathf.Min(topLeft.x, bottomLeft.x);
        float right = Mathf.Max(topRight.x, bottomRight.x);
        float top = Mathf.Max(topLeft.y, topRight.y);
        float bottom = Mathf.Min(bottomLeft.y, bottomRight.y);

        // Tính điểm giao với biên dựa trên direction
        float tMin = float.MaxValue;
        Vector2 edgePos = startPos;

        // Kiểm tra biên trái
        if (direction.x < 0)
        {
            float t = (left - startPos.x) / direction.x;
            if (t > 0 && t < tMin)
            {
                float y = startPos.y + direction.y * t;
                if (y >= bottom && y <= top)
                {
                    tMin = t;
                    edgePos = startPos + direction * t;
                }
            }
        }
        // Kiểm tra biên phải
        else if (direction.x > 0)
        {
            float t = (right - startPos.x) / direction.x;
            if (t > 0 && t < tMin)
            {
                float y = startPos.y + direction.y * t;
                if (y >= bottom && y <= top)
                {
                    tMin = t;
                    edgePos = startPos + direction * t;
                }
            }
        }

        // Kiểm tra biên dưới
        if (direction.y < 0)
        {
            float t = (bottom - startPos.y) / direction.y;
            if (t > 0 && t < tMin)
            {
                float x = startPos.x + direction.x * t;
                if (x >= left && x <= right)
                {
                    tMin = t;
                    edgePos = startPos + direction * t;
                }
            }
        }
        // Kiểm tra biên trên
        else if (direction.y > 0)
        {
            float t = (top - startPos.y) / direction.y;
            if (t > 0 && t < tMin)
            {
                float x = startPos.x + direction.x * t;
                if (x >= left && x <= right)
                {
                    tMin = t;
                    edgePos = startPos + direction * t;
                }
            }
        }

        return edgePos;
    }

    /// <summary>
    /// Tính direction đến biên gần nhất
    /// </summary>
    private Vector2 GetDirectionToNearestEdge(Vector2 currentPos)
    {
        if (_controller == null)
        {
            return UnityEngine.Random.insideUnitCircle.normalized;
        }

        // Lấy vị trí world của 4 góc biên
        Vector2 topLeft = _controller.TopLeftCorner != null ? (Vector2)_controller.TopLeftCorner.position : Vector2.zero;
        Vector2 topRight = _controller.TopRightCorner != null ? (Vector2)_controller.TopRightCorner.position : Vector2.zero;
        Vector2 bottomLeft = _controller.BottomLeftCorner != null ? (Vector2)_controller.BottomLeftCorner.position : Vector2.zero;
        Vector2 bottomRight = _controller.BottomRightCorner != null ? (Vector2)_controller.BottomRightCorner.position : Vector2.zero;

        // Tính toán biên từ 4 điểm
        float left = Mathf.Min(topLeft.x, bottomLeft.x);
        float right = Mathf.Max(topRight.x, bottomRight.x);
        float top = Mathf.Max(topLeft.y, topRight.y);
        float bottom = Mathf.Min(bottomLeft.y, bottomRight.y);

        // Tính khoảng cách đến các biên
        float distToLeft = Mathf.Abs(currentPos.x - left);
        float distToRight = Mathf.Abs(currentPos.x - right);
        float distToTop = Mathf.Abs(currentPos.y - top);
        float distToBottom = Mathf.Abs(currentPos.y - bottom);

        // Tìm biên gần nhất
        float minDist = Mathf.Min(distToLeft, distToRight, distToTop, distToBottom);
        Vector2 direction = Vector2.zero;

        if (minDist == distToLeft)
        {
            // Biên trái gần nhất
            direction = new Vector2(-1f, UnityEngine.Random.Range(-0.3f, 0.3f)).normalized;
        }
        else if (minDist == distToRight)
        {
            // Biên phải gần nhất
            direction = new Vector2(1f, UnityEngine.Random.Range(-0.3f, 0.3f)).normalized;
        }
        else if (minDist == distToTop)
        {
            // Biên trên gần nhất
            direction = new Vector2(UnityEngine.Random.Range(-0.3f, 0.3f), 1f).normalized;
        }
        else
        {
            // Biên dưới gần nhất
            direction = new Vector2(UnityEngine.Random.Range(-0.3f, 0.3f), -1f).normalized;
        }

        return direction;
    }

    /// <summary>
    /// Lấy vị trí center của biên dựa trên 4 điểm góc
    /// </summary>
    private Vector2 GetScreenCenter()
    {
        if (_controller == null)
        {
            return Vector2.zero;
        }

        // Lấy vị trí world của 4 góc biên
        Vector2 topLeft = _controller.TopLeftCorner != null ? (Vector2)_controller.TopLeftCorner.position : Vector2.zero;
        Vector2 topRight = _controller.TopRightCorner != null ? (Vector2)_controller.TopRightCorner.position : Vector2.zero;
        Vector2 bottomLeft = _controller.BottomLeftCorner != null ? (Vector2)_controller.BottomLeftCorner.position : Vector2.zero;
        Vector2 bottomRight = _controller.BottomRightCorner != null ? (Vector2)_controller.BottomRightCorner.position : Vector2.zero;

        // Tính center từ 4 điểm
        Vector2 center = (topLeft + topRight + bottomLeft + bottomRight) / 4f;
        return center;
    }

    /// <summary>
    /// Bắt đầu hiệu ứng lắc nhẹ khi di chuyển
    /// </summary>
    private void StartShake()
    {
        if (rectTransform == null || _shakeTween != null)
        {
            return;
        }

        // Shake rotation để tạo cảm giác lơ lửng, không conflict với DOMove
        _shakeTween = rectTransform.DOShakeRotation(_shakeDuration, _shakeStrength * 0.1f, 10, 90f, false)
            .SetLoops(-1, LoopType.Restart)
            .SetEase(Ease.Linear);
    }

    /// <summary>
    /// Dừng hiệu ứng lắc
    /// </summary>
    private void StopShake()
    {
        if (_shakeTween != null)
        {
            _shakeTween.Kill();
            _shakeTween = null;
            
            // Reset rotation về 0 khi dừng shake
            if (rectTransform != null)
            {
                rectTransform.rotation = Quaternion.identity;
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Không cho tương tác nếu đã Win
        if (_controller != null && _controller.HasWon)
        {
            return;
        }

        if (_isMoving || rectTransform == null)
        {
            return;
        }

        Vector3 newPos = eventData.position - _distance;
        rectTransform.position = newPos;
        transform.SetAsLastSibling();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Không cho tương tác nếu đã Win
        if (_controller != null && _controller.HasWon)
        {
            return;
        }

        if (rectTransform == null)
        {
            return;
        }

        // Đánh dấu đã tương tác
        _hasBeenInteracted = true;

        // Nếu đang di chuyển, cancel move
        if (_isMoving && _moveTween != null)
        {
            _moveTween.Kill();
            _moveTween = null;
            _isMoving = false;
            _timeWithoutPointerDown = 0f;
            
            // Dừng shake khi user click
            StopShake();
        }

        isPointDown = true;
        _distance = new Vector2();
        _distance.x = eventData.position.x - rectTransform.position.x;
        _distance.y = eventData.position.y - rectTransform.position.y;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Không cho tương tác nếu đã Win
        if (_controller != null && _controller.HasWon)
        {
            return;
        }

        isPointDown = false;
        if (_isMoving)
        {
            return;
        }
    }

    /// <summary>
    /// Hủy tất cả DOTween đang chạy (move và shake)
    /// </summary>
    public void KillAllTweens()
    {
        if (_moveTween != null)
        {
            _moveTween.Kill();
            _moveTween = null;
        }

        if (_shakeTween != null)
        {
            _shakeTween.Kill();
            _shakeTween = null;
        }

        _isMoving = false;

        // Reset rotation về 0
        if (rectTransform != null)
        {
            rectTransform.rotation = Quaternion.identity;
        }
    }
}
