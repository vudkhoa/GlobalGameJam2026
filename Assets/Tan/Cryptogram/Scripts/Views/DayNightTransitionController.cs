using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Cysharp.Threading.Tasks;

public class DayNightTransitionController : MonoBehaviour
{
    [Header("--- BACKGROUNDS ---")]
    public CanvasGroup bgDayGroup;
    public CanvasGroup bgNightGroup;

    [Header("--- EARTH SYSTEM ---")]
    public RectTransform earthSpinContainer;
    public Image earthDay;
    public Image earthNight;

    [Header("--- ORBIT SYSTEM ---")]
    public RectTransform orbitPivot;
    public CanvasGroup sunGroup;
    public CanvasGroup moonGroup;

    [Header("--- SETTINGS ---")]
    public float transitionDuration = 3f;
    public float holdDuration = 2f;
    public float earthRotationSpeed = 20f;
    public float orbitIdleSpeed = 10f;
    public float delayTime = 1000f;

    private Tween _orbitIdleTween;
    private Tween _earthSpinTween;

    private void Start()
    {
        // CHỈ Reset về trạng thái tĩnh (đứng yên)
        ResetToDay();
        
        // QUAN TRỌNG: Comment hoặc xóa dòng này đi để nó không tự chạy
        // StartIdleAnimations(); 
    }

    public void ResetToDay()
    {
        SetAlpha(bgDayGroup, 1);
        SetAlpha(bgNightGroup, 0);

        earthDay.DOFade(1, 0);
        earthNight.DOFade(0, 0);

        SetAlpha(sunGroup, 1);
        SetAlpha(moonGroup, 0);

        // Reset vị trí về 0 (đứng yên)
        orbitPivot.localRotation = Quaternion.Euler(0, 0, 0);
        
        // Nếu muốn chắc chắn, kill luôn tween tại đây
        _earthSpinTween?.Kill();
        _orbitIdleTween?.Kill();
    }

    // Hàm này sẽ được gọi nội bộ khi PlayDayNightCycleAsync bắt đầu
    private void StartIdleAnimations()
    {
        _earthSpinTween?.Kill();
        _orbitIdleTween?.Kill();

        // 1. Trái đất quay
        _earthSpinTween = earthSpinContainer
            .DORotate(new Vector3(0, 0, -360), 360f / earthRotationSpeed, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Incremental)
            .SetLink(gameObject);

        // 2. Hệ mặt trời quay chậm
        _orbitIdleTween = orbitPivot
            .DORotate(new Vector3(0, 0, -360), 360f / orbitIdleSpeed, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Incremental)
            .SetLink(gameObject);
    }

    // --- MAIN FUNCTION ---
    public async UniTask PlayDayNightCycleAsync(System.Threading.CancellationToken token)
    {
        // 1. Bắt đầu animation quay (Idle) tại đây
        // Lúc này người chơi mới thấy trái đất bắt đầu quay
        StartIdleAnimations();

        // 2. Đợi 1 chút để người chơi ngắm cảnh "Ban ngày đang quay" (ví dụ 1 giây)
        await UniTask.Delay(1000, cancellationToken: token);

        // 3. Chuẩn bị chuyển sang Ban đêm -> Kill cái quay chậm của mặt trời
        _orbitIdleTween?.Kill();

        // 4. Bắt đầu chuỗi chuyển cảnh (Sequence)
        Sequence seq = DOTween.Sequence();
        seq.SetLink(gameObject);

        // Background
        seq.Join(bgDayGroup.DOFade(0, transitionDuration));
        seq.Join(bgNightGroup.DOFade(1, transitionDuration));

        // Earth Skin
        seq.Join(earthDay.DOFade(0, transitionDuration));
        seq.Join(earthNight.DOFade(1, transitionDuration));

        // Sun Sets / Moon Rises (Xoay nhanh)
        Vector3 currentRot = orbitPivot.localEulerAngles;
        Vector3 targetRot = new Vector3(0, 0, currentRot.z - 180f);
        
        seq.Join(orbitPivot
            .DOLocalRotate(targetRot, transitionDuration, RotateMode.Fast)
            .SetEase(Ease.InOutSine));

        // Fade Sun/Moon
        seq.Join(sunGroup.DOFade(0, transitionDuration * 0.5f));
        seq.Join(moonGroup.DOFade(1, transitionDuration).SetDelay(transitionDuration * 0.3f));

        await seq.ToUniTask(cancellationToken: token);

        // Hold Night state
        await UniTask.Delay((int)(holdDuration * 1000), cancellationToken: token);
        
        // (Tuỳ chọn) Fade out toàn bộ Panel khi xong
        var mainCG = GetComponent<CanvasGroup>();
        if(mainCG != null) 
            await mainCG.DOFade(0, 1f).SetLink(gameObject).ToUniTask(cancellationToken: token);
    }

    private void SetAlpha(CanvasGroup cg, float alpha)
    {
        if (cg != null) cg.alpha = alpha;
    }
}