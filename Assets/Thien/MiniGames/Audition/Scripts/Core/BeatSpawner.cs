using System;
using System.Collections.Generic;
using UnityEngine;

public class BeatSpawner : MonoBehaviour
{
    [Header("Configurations")]
    [SerializeField] private BeatConfig _beatConfig;

    [Header("References")]
    [SerializeField] private BeatCirclePool _beatPool;
    [SerializeField] private Transform _beatContainer;

    private List<BeatData> _currentBeats;
    private int _currentBeatIndex = 0;

    public event Action<BeatCircle> OnBeatSpawned;

    // ✅ NEW: Cho phép update BeatConfig runtime
    public void UpdateBeatConfig(BeatConfig beatConfig)
    {
        _beatConfig = beatConfig;
        Debug.Log($"[BeatSpawner] Updated BeatConfig: {beatConfig.name}");
    }

    public void SetBeats(List<BeatData> beats)
    {
        _currentBeats = beats;
        _currentBeatIndex = 0;

        Debug.Log($"[BeatSpawner] Set {beats.Count} beats");
    }

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
        BeatCircle beatCircle = _beatPool.Get();

        if (beatCircle == null)
        {
            Debug.LogError("[BeatSpawner] Failed to get BeatCircle from pool!");
            return;
        }

        beatCircle.transform.SetParent(_beatContainer, false);

        // ✅ Sử dụng BeatConfig hiện tại của phase
        beatCircle.Initialize(_beatConfig, beatData, OnBeatReturnedToPool);

        OnBeatSpawned?.Invoke(beatCircle);
    }

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