using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// SRP: Spawn beats using object pool
/// Responsibility: Spawn beats from pool at correct timing
/// </summary>
public class BeatSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BeatCirclePool _beatPool; 
    [SerializeField] private Transform _beatContainer;

    private BeatConfig _beatConfig;
    private List<BeatData> _currentBeats;
    private int _currentBeatIndex = 0;

    public event Action<BeatCircle> OnBeatSpawned;

    // ═══════════════════════════════════════════════════════════
    // INITIALIZATION
    // ═══════════════════════════════════════════════════════════

    public void Initialize(BeatConfig beatConfig)
    {
        _beatConfig = beatConfig;
    }

    public void SetBeats(List<BeatData> beats)
    {
        _currentBeats = beats;
        _currentBeatIndex = 0;

        Debug.Log($"[BeatSpawner] Set {beats.Count} beats");
    }

    // ═══════════════════════════════════════════════════════════
    // UPDATE
    // ═══════════════════════════════════════════════════════════

    public void UpdateSpawning(float gameTime)
    {
        if (_currentBeats == null) return;

        while (_currentBeatIndex < _currentBeats.Count)
        {
            BeatData beatData = _currentBeats[_currentBeatIndex];

            if (gameTime >= beatData.time)
            {
                SpawnBeat(beatData);
                _currentBeatIndex++;
            }
            else
            {
                break;
            }
        }
    }

    private void SpawnBeat(BeatData beatData)
    {
        // Get from pool
        BeatCircle beatCircle = _beatPool.Get();

        if (beatCircle == null)
        {
            Debug.LogError("[BeatSpawner] Failed to get BeatCircle from pool!");
            return;
        }

        // Set parent
        beatCircle.transform.SetParent(_beatContainer, false);

        // Initialize with pool callback
        beatCircle.Initialize(_beatConfig, beatData, OnBeatReturnedToPool);

        OnBeatSpawned?.Invoke(beatCircle);
    }

    // Handle beat returning to pool
    private void OnBeatReturnedToPool(BeatCircle beatCircle)
    {
        _beatPool.Release(beatCircle);
    }

    public bool HasMoreBeats()
    {
        if (_currentBeats == null) return false;
        return _currentBeatIndex < _currentBeats.Count;
    }
}