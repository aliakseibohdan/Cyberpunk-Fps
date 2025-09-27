using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Mixer References")]
    [SerializeField] private AudioMixer masterMixer;
    [SerializeField] private AudioMixerGroup musicGroup;
    [SerializeField] private AudioMixerGroup sfxGroup;
    [SerializeField] private AudioMixerGroup uiGroup;
    [SerializeField] private AudioMixerGroup dialogueGroup;
    [SerializeField] private AudioMixerGroup ambienceGroup;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource musicSourceSecondary; // For seamless crossfades
    [SerializeField] private AudioSource ambienceSource;
    [SerializeField] private AudioSource ambienceSourceSecondary; // For seamless crossfades
    [SerializeField] private int initialPoolSize = 20;
    [SerializeField] private int maxPoolSize = 50;

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 1f)] public float dialogueVolume = 1f;
    [Range(0f, 1f)] public float ambienceVolume = 1f;

    [Header("Performance Settings")]
    [SerializeField] private int fadeUpdatesPerSecond = 30;
    [SerializeField] private int cleanupFrameInterval = 15; // Reduced frequency

    // Optimized data structures
    private Dictionary<string, AudioClip> audioLibrary;
    private Queue<AudioSource> audioSourcePool;
    private HashSet<AudioSource> activeSources;
    private List<AudioSource> sourcesToReturn;

    // Optimized fade system
    private FadeState musicFadeState;
    private FadeState ambienceFadeState;
    private float fadeUpdateInterval;
    private float lastFadeUpdateTime;
    private int frameCount;

    private AudioSource activeMusicSource;
    private AudioSource inactiveMusicSource;
    private AudioSource activeAmbienceSource;
    private AudioSource inactiveAmbienceSource;

    private struct FadeState
    {
        public bool isActive;
        public float startVolume;
        public float targetVolume;
        public float currentTime;
        public float duration;
        public AudioClip targetClip;
        public bool isCrossFade;
        public bool useSecondarySource;
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        Initialize();
    }

    private void Initialize()
    {
        audioSourcePool = new Queue<AudioSource>(initialPoolSize);
        activeSources = new HashSet<AudioSource>();
        sourcesToReturn = new List<AudioSource>(initialPoolSize);
        audioLibrary = new Dictionary<string, AudioClip>();

        // Setup dual sources for seamless crossfades
        if (musicSourceSecondary == null && musicSource != null)
        {
            GameObject secondaryMusic = new("MusicSourceSecondary");
            secondaryMusic.transform.SetParent(transform);
            musicSourceSecondary = secondaryMusic.AddComponent<AudioSource>();
            CopyAudioSourceProperties(musicSource, musicSourceSecondary);
        }

        if (ambienceSourceSecondary == null && ambienceSource != null)
        {
            GameObject secondaryAmbience = new("AmbienceSourceSecondary");
            secondaryAmbience.transform.SetParent(transform);
            ambienceSourceSecondary = secondaryAmbience.AddComponent<AudioSource>();
            CopyAudioSourceProperties(ambienceSource, ambienceSourceSecondary);
        }

        activeMusicSource = musicSource;
        inactiveMusicSource = musicSourceSecondary;
        activeAmbienceSource = ambienceSource;
        inactiveAmbienceSource = ambienceSourceSecondary;

        fadeUpdateInterval = 1f / fadeUpdatesPerSecond;

        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewAudioSource();
        }

        LoadAudioClips();

        musicFadeState = new FadeState { isActive = false };
        ambienceFadeState = new FadeState { isActive = false };
    }

    private void CopyAudioSourceProperties(AudioSource source, AudioSource target)
    {
        target.outputAudioMixerGroup = source.outputAudioMixerGroup;
        target.volume = source.volume;
        target.pitch = source.pitch;
        target.loop = source.loop;
        target.spatialBlend = source.spatialBlend;
        target.playOnAwake = false;
    }

    private void CreateNewAudioSource()
    {
        if (audioSourcePool.Count >= maxPoolSize) return;

        GameObject sourceObject = new GameObject("PooledAudioSource", typeof(AudioSource));
        sourceObject.transform.SetParent(transform);
        AudioSource source = sourceObject.GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        audioSourcePool.Enqueue(source);
    }

    private void LoadAudioClips()
    {
        AudioClip[] clips = Resources.LoadAll<AudioClip>("Audio");
        audioLibrary = new Dictionary<string, AudioClip>(clips.Length);

        foreach (AudioClip clip in clips)
        {
            audioLibrary[clip.name] = clip;
            // Removed preloading - let Unity handle it to avoid hitches
        }
    }

    private void Update()
    {
        frameCount++;

        // Update fades only when active, at reduced frequency
        bool needsFadeUpdate = (musicFadeState.isActive || ambienceFadeState.isActive) &&
                              (Time.time - lastFadeUpdateTime >= fadeUpdateInterval);

        if (needsFadeUpdate)
        {
            UpdateFades();
            lastFadeUpdateTime = Time.time;
        }

        // Cleanup sources less frequently
        if (activeSources.Count > 0 && frameCount % cleanupFrameInterval == 0)
        {
            CleanupFinishedSources();
        }
    }

    private void UpdateFades()
    {
        // No budget system - just update both fades efficiently
        if (musicFadeState.isActive)
        {
            UpdateMusicFade();
        }

        if (ambienceFadeState.isActive)
        {
            UpdateAmbienceFade();
        }
    }

    private void UpdateMusicFade()
    {
        musicFadeState.currentTime += fadeUpdateInterval;
        float progress = Mathf.Clamp01(musicFadeState.currentTime / musicFadeState.duration);

        AudioSource source = musicFadeState.useSecondarySource ? inactiveMusicSource : activeMusicSource;
        source.volume = LinearLerp(musicFadeState.startVolume, musicFadeState.targetVolume, progress);

        if (progress >= 1f)
        {
            HandleFadeCompletion(ref musicFadeState, source, true);
        }
    }

    private void UpdateAmbienceFade()
    {
        ambienceFadeState.currentTime += fadeUpdateInterval;
        float progress = Mathf.Clamp01(ambienceFadeState.currentTime / ambienceFadeState.duration);

        AudioSource source = ambienceFadeState.useSecondarySource ? inactiveAmbienceSource : activeAmbienceSource;
        source.volume = LinearLerp(ambienceFadeState.startVolume, ambienceFadeState.targetVolume, progress);

        if (progress >= 1f)
        {
            HandleFadeCompletion(ref ambienceFadeState, source, false);
        }
    }

    private void HandleFadeCompletion(ref FadeState fadeState, AudioSource source, bool isMusic)
    {
        if (fadeState.isCrossFade && fadeState.targetClip != null)
        {
            // For crossfades, prepare the secondary source and swap
            AudioSource secondarySource = isMusic ? inactiveMusicSource : inactiveAmbienceSource;
            AudioSource primarySource = isMusic ? activeMusicSource : activeAmbienceSource;

            // Set up secondary source with new clip
            secondarySource.clip = fadeState.targetClip;
            secondarySource.volume = 0f;
            secondarySource.Play();

            // Swap active/inactive sources
            if (isMusic)
            {
                (activeMusicSource, inactiveMusicSource) = (inactiveMusicSource, activeMusicSource);
            }
            else
            {
                (activeAmbienceSource, inactiveAmbienceSource) = (inactiveAmbienceSource, activeAmbienceSource);
            }

            // Stop old source
            source.Stop();

            // Start fade in on new active source
            fadeState.startVolume = 0f;
            fadeState.targetVolume = isMusic ? musicVolume : ambienceVolume;
            fadeState.currentTime = 0f;
            fadeState.isCrossFade = false;
            fadeState.targetClip = null;
            fadeState.useSecondarySource = !fadeState.useSecondarySource;
        }
        else
        {
            fadeState.isActive = false;
            source.volume = fadeState.targetVolume;

            if (fadeState.targetVolume <= 0.01f)
            {
                source.Stop();
            }
        }
    }

    private void CleanupFinishedSources()
    {
        sourcesToReturn.Clear();

        // Use direct iteration without allocation
        foreach (AudioSource source in activeSources)
        {
            if (!source.isPlaying)
            {
                sourcesToReturn.Add(source);
            }
        }

        foreach (AudioSource source in sourcesToReturn)
        {
            ReturnAudioSourceToPool(source);
            activeSources.Remove(source);
        }
    }

    // Fastest possible interpolation
    private float LinearLerp(float a, float b, float t)
    {
        return a + (b - a) * t;
    }

    private AudioSource GetAudioSource()
    {
        if (audioSourcePool.Count == 0)
        {
            if (audioSourcePool.Count + activeSources.Count < maxPoolSize)
            {
                CreateNewAudioSource();
            }
            else
            {
                return GetOldestActiveSource();
            }
        }

        AudioSource source = audioSourcePool.Dequeue();
        activeSources.Add(source);
        return source;
    }

    private AudioSource GetOldestActiveSource()
    {
        // Quick check for any non-looping, non-essential source
        foreach (AudioSource source in activeSources)
        {
            if (!source.loop && source.outputAudioMixerGroup != dialogueGroup)
            {
                source.Stop();
                ReturnAudioSourceToPool(source);
                activeSources.Remove(source);
                return GetAudioSource();
            }
        }

        // If no suitable source found, expand pool slightly
        if (audioSourcePool.Count + activeSources.Count < maxPoolSize + 5)
        {
            CreateNewAudioSource();
            return audioSourcePool.Dequeue();
        }

        // Last resort: use the first available source
        foreach (AudioSource source in activeSources)
        {
            if (source.outputAudioMixerGroup != dialogueGroup)
            {
                source.Stop();
                ReturnAudioSourceToPool(source);
                activeSources.Remove(source);
                return GetAudioSource();
            }
        }

        return null; // Should rarely happen
    }

    private void ReturnAudioSourceToPool(AudioSource source)
    {
        source.clip = null;
        source.outputAudioMixerGroup = null;
        source.loop = false;
        source.spatialBlend = 0f;
        audioSourcePool.Enqueue(source);
    }

    public void PlaySound(string clipName, AudioMixerGroup mixerGroup,
                         float volume = 1f, float pitch = 1f,
                         bool loop = false, Vector3 position = default)
    {
        if (!audioLibrary.TryGetValue(clipName, out AudioClip clip))
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogWarning($"Audio clip {clipName} not found in library");
#endif
            return;
        }

        AudioSource source = GetAudioSource();
        if (source == null) return;

        source.transform.position = position;
        source.clip = clip;
        source.outputAudioMixerGroup = mixerGroup;
        source.volume = volume;
        source.pitch = Mathf.Clamp(pitch, 0.1f, 3f);
        source.loop = loop;
        source.Play();
    }

    public void PlaySoundAtPoint(string clipName, Vector3 position, AudioMixerGroup mixerGroup,
                               float volume = 1f, float spatialBlend = 1f)
    {
        if (!audioLibrary.TryGetValue(clipName, out AudioClip clip))
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogWarning($"Audio clip {clipName} not found in library");
#endif
            return;
        }

        AudioSource source = GetAudioSource();
        if (source == null) return;

        source.transform.position = position;
        source.clip = clip;
        source.outputAudioMixerGroup = mixerGroup;
        source.volume = volume;
        source.pitch = 1f;
        source.loop = false;
        source.spatialBlend = spatialBlend;
        source.Play();
    }

    public void PlayUISound(string clipName, float volume = 1f)
        => PlaySound(clipName, uiGroup, volume);

    public void PlayDialogueSound(string clipName, float volume = 1f)
        => PlaySound(clipName, dialogueGroup, volume);

    public void PlaySFXSound(string clipName, float volume = 1f, Vector3 position = default)
        => PlaySound(clipName, sfxGroup, volume, 1f, false, position);

    public void PlaySFXAtPoint(string clipName, Vector3 position, float volume = 1f)
        => PlaySoundAtPoint(clipName, position, sfxGroup, volume, 1f);

    public void PlayMusic(string clipName, float fadeDuration = 1f)
    {
        if (!audioLibrary.TryGetValue(clipName, out AudioClip clip)) return;

        musicFadeState.isActive = false;

        musicFadeState = new FadeState
        {
            isActive = true,
            startVolume = activeMusicSource.volume,
            targetVolume = 0f,
            currentTime = 0f,
            duration = fadeDuration,
            targetClip = clip,
            isCrossFade = true,
            useSecondarySource = (activeMusicSource == musicSourceSecondary)
        };
    }

    public void PlayAmbience(string clipName, float fadeDuration = 2f)
    {
        if (!audioLibrary.TryGetValue(clipName, out AudioClip clip)) return;

        ambienceFadeState.isActive = false;

        ambienceFadeState = new FadeState
        {
            isActive = true,
            startVolume = activeAmbienceSource.volume,
            targetVolume = 0f,
            currentTime = 0f,
            duration = fadeDuration,
            targetClip = clip,
            isCrossFade = true,
            useSecondarySource = (activeAmbienceSource == ambienceSourceSecondary)
        };
    }

    public void FadeOutMusic(float fadeDuration = 1f)
    {
        musicFadeState = new FadeState
        {
            isActive = true,
            startVolume = activeMusicSource.volume,
            targetVolume = 0f,
            currentTime = 0f,
            duration = fadeDuration,
            isCrossFade = false,
            useSecondarySource = (activeMusicSource == musicSourceSecondary)
        };
    }

    public void FadeOutAmbience(float fadeDuration = 2f)
    {
        ambienceFadeState = new FadeState
        {
            isActive = true,
            startVolume = activeAmbienceSource.volume,
            targetVolume = 0f,
            currentTime = 0f,
            duration = fadeDuration,
            isCrossFade = false,
            useSecondarySource = (activeAmbienceSource == ambienceSourceSecondary)
        };
    }

    public void FadeInMusic(float fadeDuration = 1f)
    {
        if (!activeMusicSource.isPlaying)
        {
            activeMusicSource.volume = 0f;
            activeMusicSource.Play();
        }

        musicFadeState = new FadeState
        {
            isActive = true,
            startVolume = activeMusicSource.volume,
            targetVolume = musicVolume,
            currentTime = 0f,
            duration = fadeDuration,
            isCrossFade = false,
            useSecondarySource = (activeMusicSource == musicSourceSecondary)
        };
    }

    public void FadeInAmbience(float fadeDuration = 2f)
    {
        if (!activeAmbienceSource.isPlaying)
        {
            activeAmbienceSource.volume = 0f;
            activeAmbienceSource.Play();
        }

        ambienceFadeState = new FadeState
        {
            isActive = true,
            startVolume = activeAmbienceSource.volume,
            targetVolume = ambienceVolume,
            currentTime = 0f,
            duration = fadeDuration,
            isCrossFade = false,
            useSecondarySource = (activeAmbienceSource == ambienceSourceSecondary)
        };
    }

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateMixerVolume("MasterVolume", masterVolume);
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        UpdateMixerVolume("MusicVolume", musicVolume);
        if (!musicFadeState.isActive) activeMusicSource.volume = musicVolume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        UpdateMixerVolume("SFXVolume", sfxVolume);
    }

    public void SetDialogueVolume(float volume)
    {
        dialogueVolume = Mathf.Clamp01(volume);
        UpdateMixerVolume("DialogueVolume", dialogueVolume);
    }

    public void SetAmbienceVolume(float volume)
    {
        ambienceVolume = Mathf.Clamp01(volume);
        UpdateMixerVolume("AmbienceVolume", ambienceVolume);
        if (!ambienceFadeState.isActive) activeAmbienceSource.volume = ambienceVolume;
    }

    private void UpdateMixerVolume(string parameterName, float volume)
    {
        masterMixer.SetFloat(parameterName, volume > 0.0001f ? Mathf.Log10(volume) * 20f : -80f);
    }

    public void StopLowPrioritySounds()
    {
        var sources = new List<AudioSource>(activeSources);
        foreach (AudioSource source in sources)
        {
            if (source.outputAudioMixerGroup != dialogueGroup)
            {
                source.Stop();
                ReturnAudioSourceToPool(source);
                activeSources.Remove(source);
            }
        }
    }

    public void StopAllSounds()
    {
        var sources = new List<AudioSource>(activeSources);
        foreach (AudioSource source in sources)
        {
            source.Stop();
            ReturnAudioSourceToPool(source);
            activeSources.Remove(source);
        }
    }

    // Performance monitoring
    public int ActiveSourceCount => activeSources.Count;
    public int PoolSize => audioSourcePool.Count;
    public bool IsMusicFading => musicFadeState.isActive;
    public bool IsAmbienceFading => ambienceFadeState.isActive;
}