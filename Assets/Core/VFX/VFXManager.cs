using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// MonoSingleton: Manage all VFX with object pooling
/// </summary>
public class VFXManager : MonoBehaviour
{
    private static VFXManager _instance;
    public static VFXManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<VFXManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("VFXManager");
                    _instance = go.AddComponent<VFXManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    // Pool for each effect prefab
    private Dictionary<GameObject, Queue<GameObject>> _effectPools = new Dictionary<GameObject, Queue<GameObject>>();
    private Dictionary<GameObject, GameObject> _activePrefabMap = new Dictionary<GameObject, GameObject>(); // instance -> prefab

    [Header("Pool Settings")]
    [SerializeField] private int _initialPoolSize = 5;
    [SerializeField] private Transform _poolContainer;

    private void Awake()
    {
        // Singleton pattern
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        // Create pool container
        if (_poolContainer == null)
        {
            GameObject container = new GameObject("VFX_Pool");
            container.transform.SetParent(transform);
            _poolContainer = container.transform;
        }
    }

    // ═══════════════════════════════════════════════════════════
    // PUBLIC API
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Spawn VFX at position
    /// </summary>
    public GameObject SpawnVFX(GameObject effectPrefab, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (effectPrefab == null)
        {
            return null;
        }

        GameObject instance = GetFromPool(effectPrefab);

        instance.transform.position = position;
        instance.transform.rotation = rotation;
        instance.transform.SetParent(parent);
        instance.SetActive(true);

        // Auto-return to pool after particle system finishes
        ParticleSystem ps = instance.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            StartCoroutine(ReturnToPoolAfterDelay(instance, effectPrefab, ps.main.duration + ps.main.startLifetime.constantMax));
        }

        return instance;
    }

    /// <summary>
    /// Spawn VFX at transform
    /// </summary>
    public GameObject SpawnVFX(GameObject effectPrefab, Transform target)
    {
        return SpawnVFX(effectPrefab, target.position, target.rotation, target);
    }

    /// <summary>
    /// Return VFX to pool manually
    /// </summary>
    public void ReturnToPool(GameObject instance)
    {
        if (instance == null) return;

        if (_activePrefabMap.TryGetValue(instance, out GameObject prefab))
        {
            instance.SetActive(false);
            instance.transform.SetParent(_poolContainer);

            if (!_effectPools.ContainsKey(prefab))
            {
                _effectPools[prefab] = new Queue<GameObject>();
            }

            _effectPools[prefab].Enqueue(instance);
            _activePrefabMap.Remove(instance);
        }
        else
        {
            // Instance not found in active map
        }
    }

    /// <summary>
    /// Pre-warm pool for specific effect
    /// </summary>
    public void PrewarmPool(GameObject effectPrefab, int count)
    {
        if (effectPrefab == null) return;

        if (!_effectPools.ContainsKey(effectPrefab))
        {
            _effectPools[effectPrefab] = new Queue<GameObject>();
        }

        for (int i = 0; i < count; i++)
        {
            GameObject instance = Instantiate(effectPrefab, _poolContainer);
            instance.SetActive(false);
            _effectPools[effectPrefab].Enqueue(instance);
        }
    }

    /// <summary>
    /// Clear all pools
    /// </summary>
    public void ClearAllPools()
    {
        foreach (var pool in _effectPools.Values)
        {
            while (pool.Count > 0)
            {
                GameObject instance = pool.Dequeue();
                if (instance != null)
                {
                    Destroy(instance);
                }
            }
        }

        _effectPools.Clear();
        _activePrefabMap.Clear();
    }

    // ═══════════════════════════════════════════════════════════
    // INTERNAL
    // ═══════════════════════════════════════════════════════════

    private GameObject GetFromPool(GameObject prefab)
    {
        // Initialize pool if needed
        if (!_effectPools.ContainsKey(prefab))
        {
            _effectPools[prefab] = new Queue<GameObject>();
            PrewarmPool(prefab, _initialPoolSize);
        }

        GameObject instance;

        // Try to reuse from pool
        while (_effectPools[prefab].Count > 0)
        {
            instance = _effectPools[prefab].Dequeue();

            // Check if instance still exists (not destroyed)
            if (instance != null)
            {
                _activePrefabMap[instance] = prefab;
                return instance;
            }
        }

        // Pool empty, create new instance
        instance = Instantiate(prefab, _poolContainer);
        _activePrefabMap[instance] = prefab;
        return instance;
    }

    private System.Collections.IEnumerator ReturnToPoolAfterDelay(GameObject instance, GameObject prefab, float delay)
    {
        yield return new WaitForSeconds(delay);

        // Check if still active (might have been manually returned)
        if (instance != null && instance.activeInHierarchy)
        {
            ReturnToPool(instance);
        }
    }

    // ═══════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════

    public int ActiveEffectCount => _activePrefabMap.Count;
    public int PooledEffectCount
    {
        get
        {
            int count = 0;
            foreach (var pool in _effectPools.Values)
            {
                count += pool.Count;
            }
            return count;
        }
    }
}
