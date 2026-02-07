using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Threading;

public class DayNightTransitionController : MonoBehaviour
{
    [Header("--- PIVOT ROTATION ---")]
    [SerializeField] private RectTransform _celestialPivot; 

    [Header("--- EARTH SETUP ---")]
    [SerializeField] private RectTransform _earthContainer;
    [SerializeField] private CanvasGroup _earthDayGroup;
    [SerializeField] private Image _earthDayImage; 
    [SerializeField] private CanvasGroup _earthNightGroup;
    [SerializeField] private float _earthRotationDuration = 20f; 

    [Header("--- CELESTIAL BODIES (LOCK ROTATION) ---")]
    [SerializeField] private RectTransform _sunRect;
    [SerializeField] private RectTransform _moonRect;

    [Header("--- SKY BACKGROUNDS ---")]
    [SerializeField] private CanvasGroup _daySkyGroup;
    [SerializeField] private Image _daySkyImage; 
    [SerializeField] private CanvasGroup _nightSkyGroup;

    [Header("--- ANIMATION SETTINGS ---")]
    [SerializeField] private float _cycleDuration = 3.0f; 

    private void Start()
    {
        if (_earthContainer != null)
        {
            _earthContainer
                .DORotate(new Vector3(0, 0, -360), _earthRotationDuration, RotateMode.FastBeyond360)
                .SetLoops(-1, LoopType.Incremental) 
                .SetEase(Ease.Linear);
        }
        
        ResetToDay();
    }

    private void LateUpdate()
    {
        if (_sunRect != null) _sunRect.rotation = Quaternion.identity;
        if (_moonRect != null) _moonRect.rotation = Quaternion.identity;
    }

    public void ResetToDay()
    {
        if (_daySkyGroup) _daySkyGroup.alpha = 1f;
        if (_nightSkyGroup) _nightSkyGroup.alpha = 0f;

        if (_daySkyImage) _daySkyImage.color = Color.white;
        if (_earthDayImage) _earthDayImage.color = Color.white;

        if (_earthDayGroup) _earthDayGroup.alpha = 1f;
        if (_earthNightGroup) _earthNightGroup.alpha = 0f;

        if (_celestialPivot) 
        {
            _celestialPivot.DOKill();
            _celestialPivot.localEulerAngles = Vector3.zero;
        }
    }

    public async UniTask PlayDayNightCycleAsync(CancellationToken token)
    {
        Sequence seq = DOTween.Sequence();

        seq.Append(_celestialPivot.DORotate(new Vector3(0, 0, -180), _cycleDuration, RotateMode.FastBeyond360)
            .SetEase(Ease.InOutSine));

        float switchTime = _cycleDuration * 0.4f;
        float fadeDuration = _cycleDuration * 0.4f;

        seq.Insert(switchTime, _daySkyGroup.DOFade(0f, fadeDuration));
        
        if (_daySkyImage != null && _earthDayImage != null)
        {
            seq.Insert(switchTime, _daySkyImage.DOColor(new Color(0.2f, 0.2f, 0.2f, 1f), fadeDuration));
            seq.Insert(switchTime, _earthDayImage.DOColor(new Color(0.2f, 0.2f, 0.2f, 1f), fadeDuration));
        }

        seq.Insert(switchTime, _nightSkyGroup.DOFade(1f, fadeDuration));

        seq.Insert(switchTime, _earthDayGroup.DOFade(0f, fadeDuration));
        seq.Insert(switchTime, _earthNightGroup.DOFade(1f, fadeDuration));

        await seq.ToUniTask(cancellationToken: token);
    }
}