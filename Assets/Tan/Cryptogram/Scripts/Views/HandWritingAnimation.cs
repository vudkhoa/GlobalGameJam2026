using UnityEngine;
using DG.Tweening;

public class HandWritingAnimation : MonoBehaviour
{
    [Header("Cấu hình chuyển động")]
    public float horizontalRange = 30f; // Khoảng cách đưa tay qua trái/phải
    public float verticalJitter = 10f;  // Độ rung nhấp nhô lên xuống
    public float rotationAngle = 6f;   // Độ nghiêng cổ tay
    public float speed = 0.25f;         // Tốc độ một nét viết

    private Sequence _writingSequence;
    private RectTransform _rectTransform;
    private Vector2 _originalPos;

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _originalPos = _rectTransform.anchoredPosition;
    }

    public void StartWriting()
    {
        StopWriting();

        _writingSequence = DOTween.Sequence();

        _writingSequence.Append(_rectTransform.DOAnchorPos(_originalPos + new Vector2(horizontalRange, -verticalJitter), speed).SetEase(Ease.InOutSine));
        _writingSequence.Join(_rectTransform.DORotate(new Vector3(0, 0, rotationAngle), speed).SetEase(Ease.InOutSine));

        _writingSequence.Append(_rectTransform.DOAnchorPos(_originalPos + new Vector2(-horizontalRange, verticalJitter), speed).SetEase(Ease.InOutSine));
        _writingSequence.Join(_rectTransform.DORotate(new Vector3(0, 0, -rotationAngle), speed).SetEase(Ease.InOutSine));

        _writingSequence.SetLoops(-1, LoopType.Yoyo); 
        _writingSequence.SetLink(gameObject); 
    }

    public void StopWriting()
    {
        if (_writingSequence != null)
        {
            _writingSequence.Kill();
        }
        _rectTransform.DOAnchorPos(_originalPos, 0.3f);
        _rectTransform.DORotate(Vector3.zero, 0.3f);
    }

    private void OnDisable()
    {
        StopWriting();
    }
}