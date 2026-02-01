using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// SRP: Manage BeatCircle object pool
/// Responsibility: Pool creation, get, release
/// </summary>
public class BeatCirclePool : MonoBehaviour
{
    [Header("Pool Settings")]
    [SerializeField] private GameObject _beatCirclePrefab;
    [SerializeField] private Transform _poolContainer;
    [SerializeField] private int _defaultCapacity = 30;
    [SerializeField] private int _maxSize = 50;

    private ObjectPool<BeatCircle> _pool;

    // ═══════════════════════════════════════════════════════════
    // INITIALIZATION
    // ═══════════════════════════════════════════════════════════

    private void Awake()
    {
        _pool = new ObjectPool<BeatCircle>(
            createFunc: OnCreate,
            actionOnGet: OnGet,
            actionOnRelease: OnRelease,
            actionOnDestroy: OnDestroyPoolObject,
            collectionCheck: true,
            defaultCapacity: _defaultCapacity,
            maxSize: _maxSize
        );

        // Pre-warm pool
        PreWarmPool(_defaultCapacity);
    }

    // ═══════════════════════════════════════════════════════════
    // POOL CALLBACKS
    // ═══════════════════════════════════════════════════════════

    private BeatCircle OnCreate()
    {
        GameObject obj = Instantiate(_beatCirclePrefab, _poolContainer);
        BeatCircle beatCircle = obj.GetComponent<BeatCircle>();

        if (beatCircle == null)
        {
            Destroy(obj);
            return null;
        }

        obj.SetActive(false);
        return beatCircle;
    }

    private void OnGet(BeatCircle beatCircle)
    {
        beatCircle.gameObject.SetActive(true);
    }

    private void OnRelease(BeatCircle beatCircle)
    {
        beatCircle.gameObject.SetActive(false);
    }

    private void OnDestroyPoolObject(BeatCircle beatCircle)
    {
        if (beatCircle != null)
        {
            Destroy(beatCircle.gameObject);
        }
    }

    // ═══════════════════════════════════════════════════════════
    // PUBLIC API
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Get a BeatCircle from pool
    /// </summary>
    public BeatCircle Get()
    {
        return _pool.Get();
    }

    /// <summary>
    /// Release a BeatCircle back to pool
    /// </summary>
    public void Release(BeatCircle beatCircle)
    {
        if (beatCircle == null) return;

        _pool.Release(beatCircle);
    }

    /// <summary>
    /// Pre-warm pool with initial objects
    /// </summary>
    private void PreWarmPool(int count)
    {
        var temp = new BeatCircle[count];

        for (int i = 0; i < count; i++)
        {
            temp[i] = _pool.Get();
        }

        for (int i = 0; i < count; i++)
        {
            _pool.Release(temp[i]);
        }
    }

    // ═══════════════════════════════════════════════════════════
    // STATS
    // ═══════════════════════════════════════════════════════════

    public int CountActive => _pool.CountActive;
    public int CountInactive => _pool.CountInactive;
    public int CountAll => _pool.CountAll;

    [ContextMenu("Log Pool Stats")]
    private void LogPoolStats()
    {
        // Pool stats logging removed for production
    }

    // ═══════════════════════════════════════════════════════════
    // CLEANUP
    // ═══════════════════════════════════════════════════════════

    private void OnDestroy()
    {
        _pool?.Clear();
    }
}