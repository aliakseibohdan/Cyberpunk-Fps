using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwapManager : MonoBehaviour
{
    public static SceneSwapManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
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
        Instance.StartCoroutine(Instance.FadeOutThenChangeScene(sceneToLoad));
    }

    private IEnumerator FadeOutThenChangeScene(SceneField sceneToLoad)
    {
        InputManager.Instance.DisableAllInput();
        SceneFadeManager.instance.FadeOut(SceneFadeManager.FadeType.PlainBlack);

        while (SceneFadeManager.instance.IsFadingOut)
        {
            yield return null;
        }

        SceneManager.LoadScene(sceneToLoad);
    }

    private IEnumerator ActivatePlayerControlsAfterFadeIn()
    {
        while(SceneFadeManager.instance.IsFadingIn)
        {
            yield return null;
        }

        InputManager.Instance.EnablePlayerInput();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(ActivatePlayerControlsAfterFadeIn());

        SceneFadeManager.instance.FadeIn(SceneFadeManager.FadeType.PlainBlack);
    }
}
