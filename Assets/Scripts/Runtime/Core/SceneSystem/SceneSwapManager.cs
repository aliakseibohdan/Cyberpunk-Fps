using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwapManager : MonoBehaviour
{
    public static SceneSwapManager Instance { get; private set; }

    private Coroutine _activeSceneSwapCoroutine;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public static void SwapScene(SceneField sceneToLoad)
    {
        if (Instance._activeSceneSwapCoroutine != null)
        {
            Instance.StopCoroutine(Instance._activeSceneSwapCoroutine);
        }

        Instance._activeSceneSwapCoroutine = Instance.StartCoroutine(
            Instance.FadeOutThenChangeScene(sceneToLoad)
        );
    }

    private IEnumerator FadeOutThenChangeScene(SceneField sceneToLoad)
    {
        InputManager.Instance?.DisableAllInput();

        SceneFadeManager.Instance.FadeOut(SceneFadeManager.FadeType.PlainBlack);

        yield return new WaitUntil(() => !SceneFadeManager.Instance.IsFadingOut);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad.SceneName);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        asyncLoad.allowSceneActivation = true;

        _activeSceneSwapCoroutine = null;
    }

    // Called by the new scene after it's loaded
    public void OnNewSceneLoaded()
    {
        if (this != Instance) return;

        StartCoroutine(ActivatePlayerControlsAfterFadeIn());
        SceneFadeManager.Instance.FadeIn(SceneFadeManager.FadeType.PlainBlack);
    }

    private IEnumerator ActivatePlayerControlsAfterFadeIn()
    {
        yield return new WaitUntil(() => !SceneFadeManager.Instance.IsFadingIn);
        InputManager.Instance?.EnablePlayerInput();
    }
}