using UnityEngine;

/// <summary>
/// A MonoBehaviour singleton that works within a single scene.
/// It does not persist across scene loads (no DontDestroyOnLoad).
/// </summary>
public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<T>();
                if (_instance == null)
                {
                    Debug.LogError($"[MonoSingleton] Instance of {typeof(T)} not found in the scene.");
                }
            }
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning($"[MonoSingleton] Duplicate instance of {typeof(T)} detected. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
        _instance = (T)this;
    }
}

/// <summary>
/// A standard C# singleton.
/// </summary>
public abstract class Singleton<T> where T : class, new()
{
    private static T _instance;
    private static readonly object _lock = new object();

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new T();
                    }
                }
            }
            return _instance;
        }
    }
}
