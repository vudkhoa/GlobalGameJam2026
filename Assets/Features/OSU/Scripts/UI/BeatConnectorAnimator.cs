using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// SRP: Handle beat connector animations
/// Responsibility: Animate connector line từ beat này sang beat khác
/// </summary>
public class BeatConnectorAnimator : MonoBehaviour
{
    private List<Image> _activeSprites = new List<Image>();
    private Coroutine _currentAnimation;

    // ═══════════════════════════════════════════════════════════
    // PUBLIC API
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Tạo connector line từ start position đến end position
    /// </summary>
    public void ConnectBeats(
        Vector2 startPos,
        Vector2 endPos,
        BeatConnectorData data,
        Transform parent,
        Action onComplete = null)
    {
        if (data == null || data.connectorSprite == null)
        {
            onComplete?.Invoke();
            return;
        }

        StopCurrentAnimation();
        _currentAnimation = StartCoroutine(AnimateConnectorRoutine(startPos, endPos, data, parent, onComplete));
    }

    public void StopCurrentAnimation()
    {
        if (_currentAnimation != null)
        {
            StopCoroutine(_currentAnimation);
            _currentAnimation = null;
        }

        CleanupSprites();
    }

    // ═══════════════════════════════════════════════════════════
    // PRIVATE METHODS
    // ═══════════════════════════════════════════════════════════

    private IEnumerator AnimateConnectorRoutine(
        Vector2 startPos,
        Vector2 endPos,
        BeatConnectorData data,
        Transform parent,
        Action onComplete)
    {
        CleanupSprites();

        // Tính toán
        Vector2 direction = endPos - startPos;
        float distance = direction.magnitude;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        direction.Normalize();

        // Tính số lượng sprites cần thiết
        float effectiveWidth = data.spriteSize.x * (1f - data.overlapFactor);
        int spriteCount = Mathf.CeilToInt(distance / effectiveWidth);
        spriteCount = Mathf.Max(1, spriteCount);

        // Spawn delay dựa trên speed
        float spawnDelay = 1f / data.spawnSpeed;

        // Spawn từng sprite
        for (int i = 0; i < spriteCount; i++)
        {
            float offset = i * effectiveWidth;
            Vector2 spritePos = startPos + direction * offset;

            CreateAndAnimateSprite(spritePos, angle, data, parent);

            yield return new WaitForSeconds(spawnDelay);
        }

        // Chờ fade in hoàn tất
        yield return new WaitForSeconds(data.fadeInDuration);

        onComplete?.Invoke();
    }

    private void CreateAndAnimateSprite(Vector2 position, float angle, BeatConnectorData data, Transform parent)
    {
        // Create sprite GameObject
        GameObject spriteObj = new GameObject("ConnectorSprite");
        spriteObj.transform.SetParent(parent, false);

        // Setup RectTransform
        RectTransform rectTransform = spriteObj.AddComponent<RectTransform>();
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = data.spriteSize;
        rectTransform.rotation = Quaternion.Euler(0f, 0f, angle);

        // Setup Image
        Image image = spriteObj.AddComponent<Image>();
        image.sprite = data.connectorSprite;
        image.color = new Color(
            data.connectorColor.r,
            data.connectorColor.g,
            data.connectorColor.b,
            0f); // Start transparent

        // Fade in animation
        image.DOFade(data.connectorColor.a, data.fadeInDuration)
            .SetEase(Ease.OutCubic);

        _activeSprites.Add(image);
    }

    private void CleanupSprites()
    {
        foreach (var sprite in _activeSprites)
        {
            if (sprite != null)
            {
                Destroy(sprite.gameObject);
            }
        }
        _activeSprites.Clear();
    }

    private void OnDestroy()
    {
        StopCurrentAnimation();
    }
}