using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

/// <summary>
/// SRP: Chỉ chịu trách nhiệm điều khiển Animation của danh sách các chấm (Dots)
/// Không chịu trách nhiệm tạo object hay tính toán vị trí.
/// 
/// </summary>
public class BeatConnectorAnimator : MonoBehaviour
{
    private List<SpriteRenderer> _dots;
    private BeatConnectorConfig _config;
    private BeatConfig _beatConfig;
    private float _beatInterval; // Thời gian để connector nối đến beat B
    private CancellationTokenSource _cts;

    public void Initialize(List<SpriteRenderer> dots, BeatConnectorConfig config, BeatConfig beatConfig, float beatInterval, Action onComplete)
    {
        _dots = dots;
        _config = config;
        _beatConfig = beatConfig;
        _beatInterval = beatInterval;

        Cleanup();
        _cts = new CancellationTokenSource();

        PlayAnimationSequence(onComplete).Forget();
    }

    private async UniTaskVoid PlayAnimationSequence(Action onComplete)
    {
        try
        {
            if (_beatConfig == null)
            {
                Debug.LogWarning("[BeatConnectorAnimator] BeatConfig is null! Cannot animate.");
                onComplete?.Invoke();
                Destroy(gameObject);
                return;
            }

            float totalDots = _dots.Count;
            float shrinkDuration = _beatConfig.shrinkDuration;

            // ═══════════════════════════════════════════════════════════
            // ✅ PHASE 1: FADE IN - Nối từ A đến B đúng lúc B xuất hiện
            // Dùng beatInterval làm timing chuẩn
            // ═══════════════════════════════════════════════════════════

            float fadeInTotalDuration = _beatInterval; //Hoàn thành đúng lúc beat B spawn
            float fadeInStepDelay = totalDots > 1 ? fadeInTotalDuration / totalDots : 0f;
            float fadeInPerDotDuration = fadeInTotalDuration * 0.3f; // Mỗi dot fade nhanh

            for (int i = 0; i < _dots.Count; i++)
            {
                Color c = _dots[i].color;
                c.a = 0f;
                _dots[i].color = c;

                _dots[i].DOFade(_config.connectorColor.a, fadeInPerDotDuration)
                    .SetDelay(i * fadeInStepDelay)
                    .SetEase(Ease.OutQuad);
            }

            // Đợi fade in xong (đúng lúc beat B xuất hiện)
            float waitFadeIn = fadeInTotalDuration + fadeInPerDotDuration;
            await UniTask.Delay(TimeSpan.FromSeconds(waitFadeIn), cancellationToken: _cts.Token);

            // ═══════════════════════════════════════════════════════════
            // ✅ PHASE 2: WAIT - Chờ đến 50% shrink của beat B
            // Beat B vừa spawn, giờ phải đợi nó shrink được 50%
            // ═══════════════════════════════════════════════════════════

            float waitUntilHalfShrink = shrinkDuration * 0.5f;

            // Trừ đi thời gian fade in overlap (nếu fade in lâu hơn 50% shrink)
            float remainingWait = waitUntilHalfShrink - Mathf.Min(waitFadeIn, shrinkDuration * 0.5f);

            if (remainingWait > 0)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(remainingWait), cancellationToken: _cts.Token);
            }

            // ═══════════════════════════════════════════════════════════
            // ✅ PHASE 3: FADE OUT - Từ 50% đến 90% shrink
            // Connector biến mất trước khi beat shrink xong
            // ═══════════════════════════════════════════════════════════

            float fadeOutTotalDuration = shrinkDuration * 0.4f; // 40% duration (50% -> 90%)
            float fadeOutStepDelay = totalDots > 1 ? fadeOutTotalDuration / totalDots : 0f;
            float fadeOutPerDotDuration = fadeOutTotalDuration * 0.3f;

            for (int i = 0; i < _dots.Count; i++)
            {
                _dots[i].DOFade(0f, fadeOutPerDotDuration)
                    .SetDelay(i * fadeOutStepDelay)
                    .SetEase(Ease.InQuad);
            }

            float waitFadeOut = fadeOutTotalDuration + fadeOutPerDotDuration;
            await UniTask.Delay(TimeSpan.FromSeconds(waitFadeOut), cancellationToken: _cts.Token);

            onComplete?.Invoke();
            Destroy(gameObject);
        }
        catch (OperationCanceledException)
        {
            // Do nothing
        }
    }

    private void Cleanup()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;

        if (_dots != null)
        {
            foreach (var dot in _dots)
            {
                if (dot != null) DOTween.Kill(dot);
            }
        }
    }

    private void OnDestroy()
    {
        Cleanup();
    }
}