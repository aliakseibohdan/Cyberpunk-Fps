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
    [SerializeField] private AudioMixerGroup weaponGroup;
    [SerializeField] private AudioMixerGroup explosionGroup;
    [SerializeField] private AudioMixerGroup movementGroup;
    [SerializeField] private AudioMixerGroup enemyGroup;
    [SerializeField] private AudioMixerGroup uiGroup;
    [SerializeField] private AudioMixerGroup dialogueGroup;
    [SerializeField] private AudioMixerGroup ambienceGroup;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource ambienceSource;
    [SerializeField] private int initialPoolSize = 20;

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 1f)] public float dialogueVolume = 1f;
    [Range(0f, 1f)] public float ambienceVolume = 1f;

    private Dictionary<string, AudioClip> audioLibrary;
    private Queue<AudioSource> audioSourcePool;
    private List<AudioSource> activeSources;
    private Dictionary<AudioSource, float> sourceVolumes;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Initialize()
    {
        audioSourcePool = new Queue<AudioSource>();
        activeSources = new List<AudioSource>();
        sourceVolumes = new Dictionary<AudioSource, float>();
        audioLibrary = new Dictionary<string, AudioClip>();

        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewAudioSource();
        }

        LoadAudioClips();

        UpdateMixerVolumes();
    }

    private void CreateNewAudioSource()
    {
        GameObject sourceObject = new("PooledAudioSource");
        sourceObject.transform.SetParent(transform);
        AudioSource source = sourceObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        audioSourcePool.Enqueue(source);
    }

    private void LoadAudioClips()
    {
        AudioClip[] clips = Resources.LoadAll<AudioClip>("Audio");

        foreach (AudioClip clip in clips)
        {
            audioLibrary[clip.name] = clip;
        }
    }

    private void Update()
    {
        for (int i = activeSources.Count - 1; i >= 0; i--)
        {
            AudioSource source = activeSources[i];
            if (!source.isPlaying)
            {
                ReturnAudioSourceToPool(source);
                activeSources.RemoveAt(i);
            }
        }
    }

    private AudioSource GetAudioSource()
    {
        if (audioSourcePool.Count == 0)
        {
            CreateNewAudioSource();
        }

        AudioSource source = audioSourcePool.Dequeue();
        activeSources.Add(source);
        return source;
    }

    private void ReturnAudioSourceToPool(AudioSource source)
    {
        source.clip = null;
        source.outputAudioMixerGroup = null;
        audioSourcePool.Enqueue(source);
    }

    public void PlaySound(string clipName, AudioMixerGroup mixerGroup,
                         float volume = 1f, float pitch = 1f,
                         bool loop = false, Vector3 position = default)
    {
        if (!audioLibrary.ContainsKey(clipName))
        {
            Debug.LogWarning($"Audio clip {clipName} not found in library");
            return;
        }

        AudioSource source = GetAudioSource();
        source.transform.position = position;
        source.clip = audioLibrary[clipName];
        source.outputAudioMixerGroup = mixerGroup;
        source.volume = volume;
        source.pitch = pitch;
        source.loop = loop;
        source.Play();

        sourceVolumes[source] = volume;
    }

    public void PlayWeaponSound(string clipName, float volume = 1f, Vector3 position = default)
    {
        PlaySound(clipName, weaponGroup, volume, 1f, false, position);
    }

    public void PlayExplosionSound(string clipName, float volume = 1f, Vector3 position = default)
    {
        PlaySound(clipName, explosionGroup, volume, 1f, false, position);
    }

    public void PlayMovementSound(string clipName, float volume = 1f, Vector3 position = default)
    {
        PlaySound(clipName, movementGroup, volume, 1f, false, position);
    }

    public void PlayUISound(string clipName, float volume = 1f)
    {
        PlaySound(clipName, uiGroup, volume);
    }

    public void PlayDialogueSound(string clipName, float volume = 1f)
    {
        PlaySound(clipName, dialogueGroup, volume);
    }

    public void PlayMusic(string clipName, float fadeDuration = 1f)
    {
        if (!audioLibrary.ContainsKey(clipName)) return;

        StartCoroutine(CrossFadeMusic(audioLibrary[clipName], fadeDuration));
    }

    public void PlayAmbience(string clipName, float fadeDuration = 2f)
    {
        if (!audioLibrary.ContainsKey(clipName)) return;

        StartCoroutine(CrossFadeAmbience(audioLibrary[clipName], fadeDuration));
    }

    private System.Collections.IEnumerator CrossFadeMusic(AudioClip newClip, float duration)
    {
        float currentTime = 0;
        float startVolume = musicSource.volume;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, currentTime / duration);
            yield return null;
        }

        musicSource.Stop();
        musicSource.clip = newClip;
        musicSource.Play();

        currentTime = 0;
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, musicVolume, currentTime / duration);
            yield return null;
        }
    }

    private System.Collections.IEnumerator CrossFadeAmbience(AudioClip newClip, float duration)
    {
        float currentTime = 0;
        float startVolume = ambienceSource.volume;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            ambienceSource.volume = Mathf.Lerp(startVolume, 0f, currentTime / duration);
            yield return null;
        }

        ambienceSource.Stop();
        ambienceSource.clip = newClip;
        ambienceSource.Play();

        currentTime = 0;
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            ambienceSource.volume = Mathf.Lerp(0f, ambienceVolume, currentTime / duration);
            yield return null;
        }
    }

    public void SetMasterVolume(float volume)
    {
        masterVolume = volume;
        UpdateMixerVolumes();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = volume;
        UpdateMixerVolumes();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = volume;
        UpdateMixerVolumes();
    }

    public void SetDialogueVolume(float volume)
    {
        dialogueVolume = volume;
        UpdateMixerVolumes();
    }

    public void SetAmbienceVolume(float volume)
    {
        ambienceVolume = volume;
        UpdateMixerVolumes();
    }

    private void UpdateMixerVolumes()
    {
        masterMixer.SetFloat("MasterVolume", Mathf.Log10(masterVolume) * 20);
        masterMixer.SetFloat("MusicVolume", Mathf.Log10(musicVolume) * 20);
        masterMixer.SetFloat("SFXVolume", Mathf.Log10(sfxVolume) * 20);
        masterMixer.SetFloat("DialogueVolume", Mathf.Log10(dialogueVolume) * 20);
        masterMixer.SetFloat("AmbienceVolume", Mathf.Log10(ambienceVolume) * 20);

        foreach (var source in activeSources)
        {
            if (sourceVolumes.ContainsKey(source))
            {
                source.volume = sourceVolumes[source] * sfxVolume;
            }
        }

        musicSource.volume = musicVolume;
        ambienceSource.volume = ambienceVolume;
    }

    public void StopLowPrioritySounds()
    {
        foreach (AudioSource source in activeSources)
        {
            if (source.outputAudioMixerGroup != dialogueGroup &&
                source.outputAudioMixerGroup != weaponGroup)
            {
                source.Stop();
            }
        }
    }

    public void DuckAudio(AudioMixerGroup[] groupsToDuck, float duckAmount = 0.3f, float duration = 0.5f)
    {
        StartCoroutine(DuckAudioRoutine(groupsToDuck, duckAmount, duration));
    }

    private System.Collections.IEnumerator DuckAudioRoutine(AudioMixerGroup[] groupsToDuck, float duckAmount, float duration)
    {
        float[] originalVolumes = new float[groupsToDuck.Length];
        for (int i = 0; i < groupsToDuck.Length; i++)
        {
            groupsToDuck[i].audioMixer.GetFloat(groupsToDuck[i].name + "Volume", out originalVolumes[i]);
            groupsToDuck[i].audioMixer.SetFloat(groupsToDuck[i].name + "Volume", Mathf.Log10(duckAmount) * 20);
        }

        yield return new WaitForSeconds(duration);

        for (int i = 0; i < groupsToDuck.Length; i++)
        {
            groupsToDuck[i].audioMixer.SetFloat(groupsToDuck[i].name + "Volume", originalVolumes[i]);
        }
    }

    public void PlaySoundAtPoint(string clipName, Vector3 position, AudioMixerGroup mixerGroup,
                               float volume = 1f, float spatialBlend = 1f)
    {
        AudioSource source = GetAudioSource();
        source.transform.position = position;
        source.spatialBlend = spatialBlend;
        PlaySound(clipName, mixerGroup, volume, 1f, false, position);
    }
}