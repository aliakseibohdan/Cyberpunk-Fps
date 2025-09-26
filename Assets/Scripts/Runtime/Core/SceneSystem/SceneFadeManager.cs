using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SceneFadeManager : MonoBehaviour
{
    public static SceneFadeManager Instance { get; private set; }

    [System.Serializable]
    public enum FadeType
    {
        Shutters,
        RadialWipe,
        PlainBlack,
        Goop
    }

    public FadeType CurrentFadeType { get; private set; }
    public bool IsFadingOut { get; private set; }
    public bool IsFadingIn { get; private set; }
    public float FadeDuration { get; set; } = 1f;

    // Shader property IDs - cached for performance
    private static readonly int FadeAmountID = Shader.PropertyToID("_FadeAmount");
    private static readonly int UseShuttersID = Shader.PropertyToID("_UseShutters");
    private static readonly int UseRadialWipeID = Shader.PropertyToID("_UseRadialWipe");
    private static readonly int UsePlainBlackID = Shader.PropertyToID("_UsePlainBlack");
    private static readonly int UseGoopID = Shader.PropertyToID("_UseGoop");

    private Image _image;
    private Material _material;
    private Coroutine _currentFadeCoroutine;

    // Pre-cached property setters for each fade type
    private System.Action[] _fadeTypeSetters;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        _image = GetComponent<Image>();
        _image.enabled = false;

        _material = new Material(_image.material);
        _image.material = _material;

        InitializeFadeTypeSetters();
    }

    private void InitializeFadeTypeSetters()
    {
        _fadeTypeSetters = new System.Action[]
        {
            () => SetFadeEffect(UseShuttersID),
            () => SetFadeEffect(UseRadialWipeID),
            () => SetFadeEffect(UsePlainBlackID),
            () => SetFadeEffect(UseGoopID)
        };
    }

    private void SetFadeEffect(int effectID)
    {
        _material.SetFloat(UseShuttersID, 0f);
        _material.SetFloat(UseRadialWipeID, 0f);
        _material.SetFloat(UsePlainBlackID, 0f);
        _material.SetFloat(UseGoopID, 0f);

        _material.SetFloat(effectID, 1f);
    }

    public void FadeOut(FadeType fadeType)
    {
        ChangeFadeEffect(fadeType);
        StartFadeOut();
    }

    public void FadeIn(FadeType fadeType)
    {
        ChangeFadeEffect(fadeType);
        StartFadeIn();
    }

    private void ChangeFadeEffect(FadeType fadeType)
    {
        CurrentFadeType = fadeType;
        _fadeTypeSetters[(int)fadeType]?.Invoke();
    }

    private void StartFadeOut()
    {
        if (IsFadingOut || IsFadingIn) return;

        StopActiveFade();

        IsFadingOut = true;
        _image.enabled = true;
        _material.SetFloat(FadeAmountID, 0f);

        _currentFadeCoroutine = StartCoroutine(FadeCoroutine(1f));
    }

    private void StartFadeIn()
    {
        if (IsFadingIn || IsFadingOut) return;

        StopActiveFade();

        IsFadingIn = true;
        _image.enabled = true;
        _material.SetFloat(FadeAmountID, 1f);

        _currentFadeCoroutine = StartCoroutine(FadeCoroutine(0f));
    }

    private void StopActiveFade()
    {
        if (_currentFadeCoroutine != null)
        {
            StopCoroutine(_currentFadeCoroutine);
            _currentFadeCoroutine = null;
        }

        IsFadingOut = false;
        IsFadingIn = false;
    }

    private IEnumerator FadeCoroutine(float targetAmount)
    {
        float startAmount = _material.GetFloat(FadeAmountID);
        float elapsedTime = 0f;
        float inverseDuration = 1f / FadeDuration;

        while (elapsedTime < FadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime * inverseDuration;
            _material.SetFloat(FadeAmountID, Mathf.Lerp(startAmount, targetAmount, t));
            yield return null;
        }

        _material.SetFloat(FadeAmountID, targetAmount);

        if (targetAmount == 0f)
        {
            _image.enabled = false;
        }

        IsFadingOut = false;
        IsFadingIn = false;
        _currentFadeCoroutine = null;
    }
}