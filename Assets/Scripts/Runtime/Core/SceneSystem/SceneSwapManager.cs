using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwapManager : MonoBehaviour
{
    public static SceneSwapManager Instance { get; private set; }

    [Header("Fade Settings")]
    [SerializeField] private float sceneFadeDuration = 1f;
    [SerializeField] private float audioFadeDuration = 0.8f; // Slightly shorter than scene fade

    private Coroutine activeSceneSwapCoroutine;
    private bool isLoadingScene = false;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public static void SwapScene(SceneField sceneToLoad)
    {
        if (Instance.isLoadingScene)
        {
            Debug.LogWarning("Scene swap already in progress!");
            return;
        }

        if (Instance.activeSceneSwapCoroutine != null)
        {
            Instance.StopCoroutine(Instance.activeSceneSwapCoroutine);
        }

        Instance.activeSceneSwapCoroutine = Instance.StartCoroutine(
            Instance.FadeOutThenChangeScene(sceneToLoad)
        );
    }

    private IEnumerator FadeOutThenChangeScene(SceneField sceneToLoad)
    {
        isLoadingScene = true;
        Debug.Log($"Starting scene transition to: {sceneToLoad.SceneName}");

        InputManager.Instance?.DisableAllInput();

        // Start audio fade out FIRST
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.FadeOutMusic(audioFadeDuration);
            AudioManager.Instance.FadeOutAmbience(audioFadeDuration);
            Debug.Log("Audio fade out started");
        }

        // Start scene fade out slightly after audio to ensure audio finishes first
        yield return new WaitForSeconds(0.1f);

        SceneFadeManager.Instance.FadeDuration = sceneFadeDuration;
        SceneFadeManager.Instance.FadeOut(SceneFadeManager.FadeType.PlainBlack);
        Debug.Log("Visual fade out started");

        // Wait for BOTH audio and visual fade outs to complete
        float fadeOutStartTime = Time.time;
        float maxFadeOutTime = Mathf.Max(sceneFadeDuration, audioFadeDuration) + 1f; // Safety margin

        while ((AudioManager.Instance != null &&
               (AudioManager.Instance.IsMusicFading || AudioManager.Instance.IsAmbienceFading)) ||
               SceneFadeManager.Instance.IsFadingOut)
        {
            // Safety timeout to prevent infinite loops
            if (Time.time - fadeOutStartTime > maxFadeOutTime)
            {
                Debug.LogWarning("Fade out timeout - forcing scene load");
                break;
            }
            yield return null;
        }

        Debug.Log("Fade out complete, loading scene...");

        // Load scene asynchronously
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad.SceneName);
        asyncLoad.allowSceneActivation = false;

        // Wait until the scene is loaded in the background
        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        // Small delay to ensure everything is ready
        yield return new WaitForSeconds(0.1f);

        asyncLoad.allowSceneActivation = true;

        // Wait for scene to be fully active
        yield return null;

        isLoadingScene = false;
        activeSceneSwapCoroutine = null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene loaded: {scene.name}");
        StartCoroutine(HandleNewSceneSetup());
    }

    private IEnumerator HandleNewSceneSetup()
    {
        // Wait for scene to stabilize
        yield return null;

        // Start visual fade in FIRST
        SceneFadeManager.Instance.FadeDuration = sceneFadeDuration;
        SceneFadeManager.Instance.FadeIn(SceneFadeManager.FadeType.PlainBlack);
        Debug.Log("Visual fade in started");

        yield return new WaitForSeconds(0.2f);

        if (AudioManager.Instance != null)
        {
            PlaySceneAudio();
            Debug.Log("Audio fade in started");
        }

        float fadeInStartTime = Time.time;
        float maxFadeInTime = sceneFadeDuration + 1f; // Safety margin

        while (SceneFadeManager.Instance.IsFadingIn)
        {
            if (Time.time - fadeInStartTime > maxFadeInTime)
            {
                Debug.LogWarning("Fade in timeout - forcing completion");
                break;
            }
            yield return null;
        }

        Debug.Log("Fade in complete, enabling input...");

        InputManager.Instance?.EnablePlayerInput();
    }

    private void PlaySceneAudio()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        // You can customize this based on scene names
        // For now, I'll just fade in the current audio
        AudioManager.Instance.FadeInMusic(audioFadeDuration);
        AudioManager.Instance.FadeInAmbience(audioFadeDuration);

        // Alternative: Scene-specific audio
        /*
        switch (sceneName)
        {
            case "MainMenu":
                AudioManager.Instance.PlayMusic("MenuMusic", audioFadeDuration);
                AudioManager.Instance.PlayAmbience("MenuAmbience", audioFadeDuration);
                break;
            case "GameScene":
                AudioManager.Instance.PlayMusic("GameplayMusic", audioFadeDuration);
                AudioManager.Instance.PlayAmbience("GameplayAmbience", audioFadeDuration);
                break;
            default:
                // Fallback: just fade in existing audio
                AudioManager.Instance.FadeInMusic(audioFadeDuration);
                AudioManager.Instance.FadeInAmbience(audioFadeDuration);
                break;
        }
        */
    }

    public bool IsTransitioning => isLoadingScene;
}