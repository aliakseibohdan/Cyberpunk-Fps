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
    [SerializeField] private AudioSource ambienceSource;
    [SerializeField] private int initialPoolSize = 20;
    [SerializeField] private int maxPoolSize = 50;

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 1f)] public float dialogueVolume = 1f;
    [Range(0f, 1f)] public float ambienceVolume = 1f;

    [Header("Fade Optimization")]
    [SerializeField] private int fadeUpdatesPerSecond = 30;

    // Optimized data structures
    private Dictionary<string, AudioClip> audioLibrary;
    private Dictionary<AudioClip, bool> clipPreloaded;
    private Queue<AudioSource> audioSourcePool;
    private HashSet<AudioSource> activeSources;
    private List<AudioSource> sourcesToReturn;

    // Optimized fade system - no coroutines
    private FadeState musicFadeState;
    private FadeState ambienceFadeState;
    private float fadeUpdateInterval;
    private float lastFadeUpdateTime;

    private struct FadeState
    {
        public bool isActive;
        public float startVolume;
        public float targetVolume;
        public float currentTime;
        public float duration;
        public AudioClip targetClip;
        public bool isCrossFade;
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

        // Calculate fade update interval based on desired FPS
        fadeUpdateInterval = 1f / fadeUpdatesPerSecond;

        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewAudioSource();
        }

        LoadAudioClips();

        musicFadeState = new FadeState { isActive = false };
        ambienceFadeState = new FadeState { isActive = false };
    }

    private void CreateNewAudioSource()
    {
        if (audioSourcePool.Count >= maxPoolSize) return;

        GameObject sourceObject = new("PooledAudioSource", typeof(AudioSource));
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
        clipPreloaded = new Dictionary<AudioClip, bool>(clips.Length);

        foreach (AudioClip clip in clips)
        {
            audioLibrary[clip.name] = clip;
            // Preload audio data to avoid runtime hitches
            if (clip.loadState == AudioDataLoadState.Unloaded)
            {
                clip.LoadAudioData();
            }
            clipPreloaded[clip] = true;
        }
    }

    private void Update()
    {
        float currentTime = Time.time;

        // Only update fades if they're active (early exit)
        if (musicFadeState.isActive || ambienceFadeState.isActive)
        {
            if (currentTime - lastFadeUpdateTime >= fadeUpdateInterval)
            {
                UpdateFades();
                lastFadeUpdateTime = currentTime;
            }
        }

        // Optimize source cleanup - only check every 10 frames
        if (activeSources.Count > 0 && Time.frameCount % 10 == 0)
        {
            sourcesToReturn.Clear();

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
    }

    private void UpdateFades()
    {
        // Budget system: don't spend more than 1ms on audio per frame
        float startTime = Time.realtimeSinceStartup;

        if (musicFadeState.isActive)
        {
            UpdateFadeState(ref musicFadeState, musicSource);
            if (Time.realtimeSinceStartup - startTime > 0.001f) return; // Budget exceeded
        }

        if (ambienceFadeState.isActive)
        {
            UpdateFadeState(ref ambienceFadeState, ambienceSource);
        }
    }

    private void UpdateFadeState(ref FadeState fadeState, AudioSource audioSource)
    {
        fadeState.currentTime += fadeUpdateInterval;

        float progress = Mathf.Clamp01(fadeState.currentTime / fadeState.duration);
        float volume = SmoothLerp(fadeState.startVolume, fadeState.targetVolume, progress);
        audioSource.volume = volume;

        if (progress >= 1f)
        {
            if (fadeState.isCrossFade && fadeState.targetClip != null)
            {
                // OPTIMIZATION: Instead of calling Stop() and Play() - just swap the clip and let it continue
                // This avoids the expensive AudioSource.Play() call
                bool wasPlaying = audioSource.isPlaying;
                audioSource.clip = fadeState.targetClip;

                // Only start playing if it wasn't already playing or if it stopped
                if (!wasPlaying)
                {
                    audioSource.Play();
                }
                // If it was playing, the clip swap happens seamlessly

                // Continue fading in the new clip
                fadeState.startVolume = 0f;
                fadeState.targetVolume = fadeState.targetClip == musicSource.clip ? musicVolume : ambienceVolume;
                fadeState.currentTime = 0f;
                fadeState.isCrossFade = false;
                fadeState.targetClip = null;
            }
            else
            {
                fadeState.isActive = false;
                audioSource.volume = fadeState.targetVolume;

                // OPTIMIZATION: Only stop if target volume is 0
                if (fadeState.targetVolume <= 0.01f)
                {
                    audioSource.Stop();
                }
            }
        }
    }

    // Even faster interpolation - removed expensive operations
    private float SmoothLerp(float a, float b, float t)
    {
        // Uses linear interpolation for fade - it's faster and good enough for audio
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

        CreateNewAudioSource();
        return audioSourcePool.Dequeue();
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
            startVolume = musicSource.volume,
            targetVolume = 0f,
            currentTime = 0f,
            duration = fadeDuration,
            targetClip = clip,
            isCrossFade = true
        };
    }

    public void PlayAmbience(string clipName, float fadeDuration = 2f)
    {
        if (!audioLibrary.TryGetValue(clipName, out AudioClip clip)) return;

        ambienceFadeState.isActive = false;

        ambienceFadeState = new FadeState
        {
            isActive = true,
            startVolume = ambienceSource.volume,
            targetVolume = 0f,
            currentTime = 0f,
            duration = fadeDuration,
            targetClip = clip,
            isCrossFade = true
        };
    }

    // Simple fade methods
    public void FadeOutMusic(float fadeDuration = 1f)
    {
        musicFadeState = new FadeState
        {
            isActive = true,
            startVolume = musicSource.volume,
            targetVolume = 0f,
            currentTime = 0f,
            duration = fadeDuration,
            isCrossFade = false
        };
    }

    public void FadeOutAmbience(float fadeDuration = 2f)
    {
        ambienceFadeState = new FadeState
        {
            isActive = true,
            startVolume = ambienceSource.volume,
            targetVolume = 0f,
            currentTime = 0f,
            duration = fadeDuration,
            isCrossFade = false
        };
    }

    public void FadeInMusic(float fadeDuration = 1f)
    {
        if (!musicSource.isPlaying) musicSource.Play();

        musicFadeState = new FadeState
        {
            isActive = true,
            startVolume = 0f,
            targetVolume = musicVolume,
            currentTime = 0f,
            duration = fadeDuration,
            isCrossFade = false
        };
    }

    public void FadeInAmbience(float fadeDuration = 2f)
    {
        if (!ambienceSource.isPlaying) ambienceSource.Play();

        ambienceFadeState = new FadeState
        {
            isActive = true,
            startVolume = 0f,
            targetVolume = ambienceVolume,
            currentTime = 0f,
            duration = fadeDuration,
            isCrossFade = false
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
        if (!musicFadeState.isActive) musicSource.volume = musicVolume;
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
        if (!ambienceFadeState.isActive) ambienceSource.volume = ambienceVolume;
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