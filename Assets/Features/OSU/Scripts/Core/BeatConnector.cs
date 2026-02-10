using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SRP: Factory & Manager.
/// Chịu trách nhiệm tính toán vị trí, spawn các chấm (Dots) và gắn Animator.
/// </summary>
public class BeatConnector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _connectorContainer;

    // List theo dõi để clean up nếu cần
    private List<BeatConnectorAnimator> _activeConnectors = new List<BeatConnectorAnimator>();

    // Track current BeatConfig for timing sync
    private BeatConfig _currentBeatConfig;

    /// <summary>
    /// Update BeatConfig để sync timing với beat hiện tại
    /// </summary>
    public void SetBeatConfig(BeatConfig beatConfig)
    {
        _currentBeatConfig = beatConfig;
    }

    /// <summary>
    /// Spawn connector từ rìa beat này sang rìa beat kia (edge-to-edge)
    /// Tự động sync timing và size từ BeatConfig
    /// </summary>
    public void SpawnConnector(Vector2 startPos, Vector2 endPos, BeatConnectorConfig config, BeatConfig beatConfig, float beatInterval)
    {
        if (config == null || config.connectorSprite == null) return;
        if (beatConfig == null)
        {
            Debug.LogWarning("[BeatConnector] BeatConfig is null! Cannot spawn connector.");
            return;
        }

        // ✅ TỰ ĐỘNG TÍNH RÌA BEAT (Edge positions) từ BeatConfig
        // Lấy bán kính inner ring (target area) từ BeatConfig
        float beatRadius = beatConfig.InnerRingWorldScale * 0.5f;

        // Tính vector từ start đến end
        Vector2 fullDirection = endPos - startPos;
        float fullDistance = fullDirection.magnitude;

        // Nếu 2 beat quá gần (hoặc trùng nhau) thì không spawn
        if (fullDistance < beatRadius * 2f)
        {
            return;
        }

        Vector2 direction = fullDirection.normalized;

        // Điều chỉnh startPos và endPos để spawn từ rìa
        Vector2 actualStartPos = startPos + direction * beatRadius;
        Vector2 actualEndPos = endPos - direction * beatRadius;

        // 1. Tạo Container (Object cha chứa cả dây)
        GameObject containerObj = new GameObject("Connector_Container");
        containerObj.transform.SetParent(_connectorContainer);
        containerObj.transform.position = Vector3.zero;

        // 2. Tính toán toán học (Math Logic)
        Vector2 connectorDirection = actualEndPos - actualStartPos;
        float distance = connectorDirection.magnitude;
        float angle = Mathf.Atan2(connectorDirection.y, connectorDirection.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle);

        int dotCount = Mathf.CeilToInt(distance * config.tilesPerUnit);
        dotCount = Mathf.Clamp(dotCount, 0, 100);

        List<SpriteRenderer> spawnedDots = new List<SpriteRenderer>();

        // 3. Spawn Dots (Object Creation Logic)
        for (int i = 0; i <= dotCount; i++)
        {
            float t = (dotCount > 0) ? (float)i / dotCount : 0f;
            Vector2 pos = Vector2.Lerp(actualStartPos, actualEndPos, t);

            GameObject dotObj = new GameObject($"Dot_{i}");
            dotObj.transform.SetParent(containerObj.transform);
            dotObj.transform.position = pos;
            dotObj.transform.rotation = rotation;

            SpriteRenderer sr = dotObj.AddComponent<SpriteRenderer>();
            sr.sprite = config.connectorSprite;
            sr.color = new Color(config.connectorColor.r, config.connectorColor.g, config.connectorColor.b, 0f);

            sr.size = new Vector2(config.lineWidth, config.lineWidth);
            sr.drawMode = SpriteDrawMode.Simple;
            sr.sortingLayerName = config.sortingLayerName;
            sr.sortingOrder = config.sortingOrder;

            spawnedDots.Add(sr);
        }

        // 4. Gắn Animator và kích hoạt (Wiring) - ✅ Truyền BeatConfig và beatInterval vào
        BeatConnectorAnimator animator = containerObj.AddComponent<BeatConnectorAnimator>();
        _activeConnectors.Add(animator);

        animator.Initialize(spawnedDots, config, beatConfig, beatInterval, () =>
        {
            if (_activeConnectors != null) _activeConnectors.Remove(animator);
        });
    }

    public void ClearAllConnectors()
    {
        foreach (var connector in _activeConnectors)
        {
            if (connector != null) Destroy(connector.gameObject);
        }
        _activeConnectors.Clear();
    }

    private void OnDestroy()
    {
        ClearAllConnectors();
    }
}