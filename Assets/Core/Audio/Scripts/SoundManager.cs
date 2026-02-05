using UnityEngine;

/// <summary>
/// MonoSingleton: Manage all game audio (BGM + SFX)
/// </summary>
public class SoundManager : MonoBehaviour
{
    private static SoundManager _instance;
    public static SoundManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<SoundManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("SoundManager");
                    _instance = go.AddComponent<SoundManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    [Header("Configuration")]
    [SerializeField] private SoundConfig _soundConfig;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource _bgmSource;
    [SerializeField] private AudioSource _sfxSource;

    [Header("Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float _masterVolume = 1f;
    [Range(0f, 1f)]
    [SerializeField] private float _bgmVolume = 0.7f;
    [Range(0f, 1f)]
    [SerializeField] private float _sfxVolume = 1f;

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

        // Create audio sources if not assigned
        if (_bgmSource == null)
        {
            GameObject bgmGO = new GameObject("BGM_Source");
            bgmGO.transform.SetParent(transform);
            _bgmSource = bgmGO.AddComponent<AudioSource>();
            _bgmSource.loop = true;
            _bgmSource.playOnAwake = false;
        }

        if (_sfxSource == null)
        {
            GameObject sfxGO = new GameObject("SFX_Source");
            sfxGO.transform.SetParent(transform);
            _sfxSource = sfxGO.AddComponent<AudioSource>();
            _sfxSource.loop = false;
            _sfxSource.playOnAwake = false;
        }
    }

    private void OnEnable()
    {
        SoundManager.Instance.PlayBGM(SoundType.BGM_Menu);
    }

    // ═══════════════════════════════════════════════════════════
    // PUBLIC API
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Play background music
    /// </summary>
    public void PlayBGM(SoundType type, bool loop = true)
    {
        AudioClip clip = _soundConfig.GetClip(type);
        if (clip == null) return;

        _bgmSource.clip = clip;
        _bgmSource.loop = loop;
        _bgmSource.volume = _bgmVolume * _masterVolume * _soundConfig.GetVolume(type);
        _bgmSource.Play();
    }

    /// <summary>
    /// Stop background music
    /// </summary>
    public void StopBGM()
    {
        _bgmSource.Stop();
    }

    /// <summary>
    /// Pause background music
    /// </summary>
    public void PauseBGM()
    {
        _bgmSource.Pause();
    }

    /// <summary>
    /// Resume background music
    /// </summary>
    public void ResumeBGM()
    {
        _bgmSource.UnPause();
    }

    /// <summary>
    /// Play sound effect
    /// </summary>
    public void PlaySFX(SoundType type)
    {
        AudioClip clip = _soundConfig.GetClip(type);
        if (clip == null) return;

        float volume = _sfxVolume * _masterVolume * _soundConfig.GetVolume(type);
        _sfxSource.PlayOneShot(clip, volume);
    }

    /// <summary>
    /// Set master volume
    /// </summary>
    public void SetMasterVolume(float volume)
    {
        _masterVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
    }

    /// <summary>
    /// Set BGM volume
    /// </summary>
    public void SetBGMVolume(float volume)
    {
        _bgmVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
    }

    /// <summary>
    /// Set SFX volume
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        _sfxVolume = Mathf.Clamp01(volume);
    }

    private void UpdateVolumes()
    {
        if (_bgmSource.clip != null)
        {
            SoundType? currentBGM = GetSoundTypeFromClip(_bgmSource.clip);
            if (currentBGM.HasValue)
            {
                _bgmSource.volume = _bgmVolume * _masterVolume * _soundConfig.GetVolume(currentBGM.Value);
            }
        }
    }

    private SoundType? GetSoundTypeFromClip(AudioClip clip)
    {
        foreach (var mapping in _soundConfig.soundMappings)
        {
            if (mapping.audioClip == clip)
                return mapping.soundType;
        }
        return null;
    }

    // ═══════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════

    public float MasterVolume => _masterVolume;
    public float BGMVolume => _bgmVolume;
    public float SFXVolume => _sfxVolume;
    public bool IsBGMPlaying => _bgmSource.isPlaying;
}
