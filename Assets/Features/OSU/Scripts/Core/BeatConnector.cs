using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SRP: Manage beat connector spawning between trajectories
/// Responsibility: Create and manage connector views from config data
/// </summary>
public class BeatConnector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _connectorContainer;

    [Header("Beat Timing Reference")]
    [SerializeField] private BeatConfig _beatConfig; // ✅ NEW: Để lấy shrink duration

    private List<BeatConnectorAnimator> _activeConnectors = new List<BeatConnectorAnimator>();

    // ✅ NEW: Method để set beat config từ GameLoop
    public void SetBeatConfig(BeatConfig beatConfig)
    {
        _beatConfig = beatConfig;
    }

    // ═══════════════════════════════════════════════════════════
    // PUBLIC API
    // ═══════════════════════════════════════════════════════════

    public void SpawnConnector(Vector2 endPosOfCurrentTrajectory, Vector2 startPosOfNextTrajectory, BeatConnectorConfig config)
    {
        if (config == null || config.connectorSprite == null)
        {
            Debug.LogWarning("[BeatConnector] No connector config or sprite found. Skipping.");
            return;
        }

        // ✅ Get beat shrink duration
        float beatShrinkDuration = _beatConfig != null ? _beatConfig.shrinkDuration : 1.5f;

        // Create connector from config (no prefab needed)
        GameObject connectorObj = new GameObject("BeatConnector");
        connectorObj.transform.SetParent(_connectorContainer);

        SpriteRenderer sr = connectorObj.AddComponent<SpriteRenderer>();
        BeatConnectorAnimator animator = connectorObj.AddComponent<BeatConnectorAnimator>();

        // ✅ Pass beat duration vào Initialize
        animator.Initialize(endPosOfCurrentTrajectory, startPosOfNextTrajectory, config, beatShrinkDuration, () =>
        {
            _activeConnectors.Remove(animator);
        });

        _activeConnectors.Add(animator);
    }

    public void ClearAllConnectors()
    {
        foreach (var connector in _activeConnectors)
        {
            if (connector != null)
            {
                Destroy(connector.gameObject);
            }
        }
        _activeConnectors.Clear();
    }

    private void OnDestroy()
    {
        ClearAllConnectors();
    }
}