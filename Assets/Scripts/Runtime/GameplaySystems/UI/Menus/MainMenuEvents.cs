using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Audio;

public class MainMenuEvents : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private float _fadeDuration = 0.5f;

    [Header("Audio Settings")]
    private AudioSource _audioSource;
    [SerializeField] private AudioMixer _audioMixer;

    [Header("Scene Management")]
    [SerializeField] private SceneField _sceneToLoadFromNewGame;
    [SerializeField] private SceneField _sceneToLoadFromContinueGame;

    // Containers
    private UIDocument _document;
    private readonly Dictionary<string, VisualElement> _containers = new();

    // Main menu buttons
    private readonly Dictionary<MenuButton, Button> _buttons = new();

    // Settings UI elements - Replaced dropdowns with arrow navigation
    private SettingControl _qualitySetting;
    private SettingControl _resolutionSetting;
    private SettingControl _fullscreenModeSetting;

    private Slider _masterVolumeSlider;
    private Slider _musicVolumeSlider;
    private Slider _sfxVolumeSlider;
    private Slider _uiVolumeSlider;
    private Button _settingsBackButton;

    // Resolution data
    private Resolution[] _availableResolutions;
    private List<Resolution> _uniqueResolutions;
    private int _currentResolutionIndex;

    // Fullscreen mode options
    private readonly List<string> _fullscreenModes = new()
    {
        "Fullscreen Window",
        "Exclusive Fullscreen",
        "Maximized Window",
        "Windowed"
    };

    private enum MenuButton
    {
        Consent,
        NewGame,
        ContinueGame,
        Settings,
        Credits,
        Exit,
        CreditsBack,
        SettingsBack
    }

    // Class to handle the new arrow-button navigation system
    private class SettingControl
    {
        public Button LeftButton { get; set; }
        public Button RightButton { get; set; }
        public Label ValueLabel { get; set; }
        public List<string> Options { get; set; }
        public int CurrentIndex { get; set; }

        public void UpdateDisplay()
        {
            if (ValueLabel != null && Options != null && Options.Count > 0)
            {
                ValueLabel.text = Options[CurrentIndex];
            }

            // Update button states
            LeftButton?.SetEnabled(CurrentIndex > 0);
            RightButton?.SetEnabled(CurrentIndex < Options.Count - 1);
        }
    }

    private void Awake()
    {
        _document = GetComponent<UIDocument>();
        CacheContainers();
        CacheMainMenuButtons();
        CacheSettingsElements();
    }

    private void Start()
    {
        InitializeAudio();
        InitializeResolutionSettings();
        InitializeQualitySettings();
        InitializeVolumeSettings();
        InitializeFullscreenModeSettings();
    }

    private void OnEnable()
    {
        RegisterMainMenuCallbacks();
        RegisterSettingsCallbacks();
        ShowContainer("DisclaimerContainer", true, true);
    }

    private void OnDisable()
    {
        UnregisterMainMenuCallbacks();
        UnregisterSettingsCallbacks();
    }

    private void CacheContainers()
    {
        var containers = _document.rootVisualElement.Query<VisualElement>(className: "menu-container").ToList();
        foreach (var container in containers)
        {
            _containers[container.name] = container;

            // Hide all containers by default
            container.style.display = DisplayStyle.None;
            container.style.opacity = 0;
        }
    }

    private void CacheMainMenuButtons()
    {
        _buttons[MenuButton.Consent] = _document.rootVisualElement.Q<Button>("ConsentButton");
        _buttons[MenuButton.NewGame] = _document.rootVisualElement.Q<Button>("NewGameButton");
        _buttons[MenuButton.ContinueGame] = _document.rootVisualElement.Q<Button>("ContinueGameButton");
        _buttons[MenuButton.Settings] = _document.rootVisualElement.Q<Button>("SettingsButton");
        _buttons[MenuButton.Credits] = _document.rootVisualElement.Q<Button>("CreditsButton");
        _buttons[MenuButton.Exit] = _document.rootVisualElement.Q<Button>("ExitButton");
        _buttons[MenuButton.CreditsBack] = _document.rootVisualElement.Q<Button>("CreditsBackButton");

        if (string.IsNullOrEmpty(_sceneToLoadFromContinueGame.SceneName)) _buttons[MenuButton.ContinueGame].SetEnabled(false);
    }

    private void CacheSettingsElements()
    {
        VisualElement settingsContainer = _document.rootVisualElement.Q("SettingsContainer");

        // Initialize setting controls
        _qualitySetting = new SettingControl();
        _resolutionSetting = new SettingControl();
        _fullscreenModeSetting = new SettingControl();

        // Quality setting elements
        _qualitySetting.LeftButton = settingsContainer.Q<Button>("QualityLeftButton");
        _qualitySetting.RightButton = settingsContainer.Q<Button>("QualityRightButton");
        _qualitySetting.ValueLabel = settingsContainer.Q<Label>("QualityValueLabel");

        // Resolution setting elements
        _resolutionSetting.LeftButton = settingsContainer.Q<Button>("ResolutionLeftButton");
        _resolutionSetting.RightButton = settingsContainer.Q<Button>("ResolutionRightButton");
        _resolutionSetting.ValueLabel = settingsContainer.Q<Label>("ResolutionValueLabel");

        // Fullscreen mode setting elements
        _fullscreenModeSetting.LeftButton = settingsContainer.Q<Button>("FullscreenLeftButton");
        _fullscreenModeSetting.RightButton = settingsContainer.Q<Button>("FullscreenRightButton");
        _fullscreenModeSetting.ValueLabel = settingsContainer.Q<Label>("FullscreenValueLabel");

        // Audio settings
        _masterVolumeSlider = settingsContainer.Q<Slider>("MasterVolumeSlider");
        _musicVolumeSlider = settingsContainer.Q<Slider>("MusicVolumeSlider");
        _sfxVolumeSlider = settingsContainer.Q<Slider>("SFXVolumeSlider");
        _uiVolumeSlider = settingsContainer.Q<Slider>("UIVolumeSlider");

        _settingsBackButton = settingsContainer.Q<Button>("SettingsBackButton");
        _buttons[MenuButton.SettingsBack] = _settingsBackButton;
    }

    private void InitializeAudio()
    {
        if (TryGetComponent(out AudioSource source))
        {
            _audioSource = source;
        }
        else
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void InitializeResolutionSettings()
    {
        // Get available resolutions
        _availableResolutions = Screen.resolutions;
        _uniqueResolutions = new List<Resolution>();

        // Filter duplicates
        foreach (var resolution in _availableResolutions)
        {
            if (_uniqueResolutions.FindIndex(r =>
                r.width == resolution.width && r.height == resolution.height) == -1)
            {
                _uniqueResolutions.Add(resolution);
            }
        }

        // Populate resolution options
        _resolutionSetting.Options = new List<string>();
        for (int i = 0; i < _uniqueResolutions.Count; i++)
        {
            _resolutionSetting.Options.Add($"{_uniqueResolutions[i].width} x {_uniqueResolutions[i].height}");

            if (_uniqueResolutions[i].width == Screen.currentResolution.width &&
                _uniqueResolutions[i].height == Screen.currentResolution.height)
            {
                _currentResolutionIndex = i;
                _resolutionSetting.CurrentIndex = i;
            }
        }

        _resolutionSetting.UpdateDisplay();
    }

    private void InitializeQualitySettings()
    {
        _qualitySetting.Options = new List<string>(QualitySettings.names);
        _qualitySetting.CurrentIndex = QualitySettings.GetQualityLevel();
        _qualitySetting.UpdateDisplay();
    }

    private void InitializeVolumeSettings()
    {
        _masterVolumeSlider.value = 80;
        _musicVolumeSlider.value = 80;
        _sfxVolumeSlider.value = 80;
        _uiVolumeSlider.value = 80;

        SetVolume("MasterVolume", _masterVolumeSlider.value);
        SetVolume("MusicVolume", _musicVolumeSlider.value);
        SetVolume("SFXVolume", _sfxVolumeSlider.value);
        SetVolume("UIVolume", _uiVolumeSlider.value);
    }

    private void InitializeFullscreenModeSettings()
    {
        _fullscreenModeSetting.Options = _fullscreenModes;
        _fullscreenModeSetting.CurrentIndex = GetCurrentFullscreenModeIndex();
        _fullscreenModeSetting.UpdateDisplay();
    }

    private int GetCurrentFullscreenModeIndex()
    {
        switch (Screen.fullScreenMode)
        {
            case FullScreenMode.FullScreenWindow:
                return 0;
            case FullScreenMode.ExclusiveFullScreen:
                return 1;
            case FullScreenMode.MaximizedWindow:
                return 2;
            case FullScreenMode.Windowed:
                return 3;
            default:
                return 0;
        }
    }

    private void RegisterMainMenuCallbacks()
    {
        foreach (var kvp in _buttons)
        {
            kvp.Value.RegisterCallback<ClickEvent>(HandleMainMenuClick);
            kvp.Value.RegisterCallback<ClickEvent>(PlayButtonSound);
        }
    }

    private void UnregisterMainMenuCallbacks()
    {
        foreach (var kvp in _buttons)
        {
            kvp.Value.UnregisterCallback<ClickEvent>(HandleMainMenuClick);
            kvp.Value.UnregisterCallback<ClickEvent>(PlayButtonSound);
        }
    }

    private void RegisterSettingsCallbacks()
    {
        // Quality setting navigation
        _qualitySetting.LeftButton.RegisterCallback<ClickEvent>(evt => NavigateSetting(_qualitySetting, -1, OnQualityChanged));
        _qualitySetting.RightButton.RegisterCallback<ClickEvent>(evt => NavigateSetting(_qualitySetting, 1, OnQualityChanged));

        // Resolution setting navigation
        _resolutionSetting.LeftButton.RegisterCallback<ClickEvent>(evt => NavigateSetting(_resolutionSetting, -1, OnResolutionChanged));
        _resolutionSetting.RightButton.RegisterCallback<ClickEvent>(evt => NavigateSetting(_resolutionSetting, 1, OnResolutionChanged));

        // Fullscreen mode setting navigation
        _fullscreenModeSetting.LeftButton.RegisterCallback<ClickEvent>(evt => NavigateSetting(_fullscreenModeSetting, -1, OnFullscreenModeChanged));
        _fullscreenModeSetting.RightButton.RegisterCallback<ClickEvent>(evt => NavigateSetting(_fullscreenModeSetting, 1, OnFullscreenModeChanged));

        // Audio settings
        _masterVolumeSlider.RegisterValueChangedCallback(evt => SetVolume("MasterVolume", evt.newValue));
        _musicVolumeSlider.RegisterValueChangedCallback(evt => SetVolume("MusicVolume", evt.newValue));
        _sfxVolumeSlider.RegisterValueChangedCallback(evt => SetVolume("SFXVolume", evt.newValue));
        _uiVolumeSlider.RegisterValueChangedCallback(evt => SetVolume("UIVolume", evt.newValue));

        // Navigation
        _settingsBackButton.RegisterCallback<ClickEvent>(HandleMainMenuClick);
    }

    private void UnregisterSettingsCallbacks()
    {
        // Quality setting
        _qualitySetting.LeftButton.UnregisterCallback<ClickEvent>(evt => NavigateSetting(_qualitySetting, -1, OnQualityChanged));
        _qualitySetting.RightButton.UnregisterCallback<ClickEvent>(evt => NavigateSetting(_qualitySetting, 1, OnQualityChanged));

        // Resolution setting
        _resolutionSetting.LeftButton.UnregisterCallback<ClickEvent>(evt => NavigateSetting(_resolutionSetting, -1, OnResolutionChanged));
        _resolutionSetting.RightButton.UnregisterCallback<ClickEvent>(evt => NavigateSetting(_resolutionSetting, 1, OnResolutionChanged));

        // Fullscreen mode setting
        _fullscreenModeSetting.LeftButton.UnregisterCallback<ClickEvent>(evt => NavigateSetting(_fullscreenModeSetting, -1, OnFullscreenModeChanged));
        _fullscreenModeSetting.RightButton.UnregisterCallback<ClickEvent>(evt => NavigateSetting(_fullscreenModeSetting, 1, OnFullscreenModeChanged));

        // Audio settings
        _masterVolumeSlider.UnregisterValueChangedCallback(evt => SetVolume("MasterVolume", evt.newValue));
        _musicVolumeSlider.UnregisterValueChangedCallback(evt => SetVolume("MusicVolume", evt.newValue));
        _sfxVolumeSlider.UnregisterValueChangedCallback(evt => SetVolume("SFXVolume", evt.newValue));
        _uiVolumeSlider.UnregisterValueChangedCallback(evt => SetVolume("UIVolume", evt.newValue));

        _settingsBackButton.UnregisterCallback<ClickEvent>(HandleMainMenuClick);
    }

    private void NavigateSetting(SettingControl setting, int direction, Action changeHandler)
    {
        int newIndex = Mathf.Clamp(setting.CurrentIndex + direction, 0, setting.Options.Count - 1);

        if (newIndex != setting.CurrentIndex)
        {
            setting.CurrentIndex = newIndex;
            setting.UpdateDisplay();
            changeHandler?.Invoke();
            PlayButtonSound(null);
        }
    }

    private void PlayButtonSound(ClickEvent evt)
    {
        _audioSource.Play();
    }

    private void HandleMainMenuClick(ClickEvent evt)
    {
        var button = evt.currentTarget as Button;

        foreach (var kvp in _buttons)
        {
            if (kvp.Value == button)
            {
                switch (kvp.Key)
                {
                    case MenuButton.Consent:

                        ShowContainer("DisclaimerContainer", false, true, .35f);
                        ShowContainer("MainMenuContainer", true, true, 1f);
                        break;

                    case MenuButton.NewGame:

                        SceneFadeManager.instance.FadeDuration = 3f;
                        SceneSwapManager.SwapScene(_sceneToLoadFromNewGame);
                        break;

                    case MenuButton.ContinueGame:

                        SceneFadeManager.instance.FadeDuration = 3f;
                        SceneSwapManager.SwapScene(_sceneToLoadFromContinueGame);
                        break;

                    case MenuButton.Settings:

                        ShowContainer("MainMenuContainer", false, true, .35f);
                        ShowContainer("SettingsContainer", true, true, .85f);
                        break;

                    case MenuButton.Credits:

                        ShowContainer("MainMenuContainer", false, true, .35f);
                        ShowContainer("CreditsContainer", true, true, .85f);
                        break;

                    case MenuButton.CreditsBack:

                        ShowContainer("CreditsContainer", false, true, .35f);
                        ShowContainer("MainMenuContainer", true, true, .85f);
                        break;

                    case MenuButton.SettingsBack:

                        ShowContainer("SettingsContainer", false, true, .35f);
                        ShowContainer("MainMenuContainer", true, true, .85f);
                        break;

                    case MenuButton.Exit:

                        StartCoroutine(QuitAfterDelay(2f));
                        break;

                    default:

                        break;
                }
                return;
            }
        }
    }

    private IEnumerator QuitAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    #region Settings Handlers
    private void OnQualityChanged()
    {
        QualitySettings.SetQualityLevel(_qualitySetting.CurrentIndex);
    }

    private void OnResolutionChanged()
    {
        Resolution resolution = _uniqueResolutions[_resolutionSetting.CurrentIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode);
    }

    private void OnFullscreenModeChanged()
    {
        FullScreenMode mode = FullScreenMode.FullScreenWindow;

        switch (_fullscreenModeSetting.CurrentIndex)
        {
            case 0:
                mode = FullScreenMode.FullScreenWindow;
                break;
            case 1:
                mode = FullScreenMode.ExclusiveFullScreen;
                break;
            case 2:
                mode = FullScreenMode.MaximizedWindow;
                break;
            case 3:
                mode = FullScreenMode.Windowed;
                break;
        }

        Screen.fullScreenMode = mode;
    }

    private void SetVolume(string parameterName, float value)
    {
        if (_audioMixer == null) return;

        // Convert 0-100 slider to -80dB to 0dB
        float dB = value > 0 ?
            Mathf.Log10(value / 100) * 20 :
            -80;

        _audioMixer.SetFloat(parameterName, dB);
    }
    #endregion

    #region Container Management
    public void ShowContainer(string containerName, bool show, bool fade = true,
                             float delay = 0f, float duration = -1f)
    {
        if (!_containers.TryGetValue(containerName, out var container))
        {
            Debug.LogWarning($"Container not found: {containerName}");
            return;
        }

        if (duration < 0) duration = _fadeDuration;

        if (fade)
        {
            StartCoroutine(SetContainerVisibilityWithFade(container, show, delay, duration));
        }
        else
        {
            container.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
            container.style.opacity = show ? 1f : 0f;
        }
    }

    private IEnumerator SetContainerVisibilityWithFade(VisualElement container, bool show,
                                                      float delay, float duration)
    {
        yield return new WaitForSeconds(delay);

        if (show)
        {
            container.style.display = DisplayStyle.Flex;
        }

        float startOpacity = container.style.opacity.value;
        float targetOpacity = show ? 1f : 0f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            container.style.opacity = Mathf.Lerp(startOpacity, targetOpacity, elapsed / duration);
            yield return null;
        }

        container.style.opacity = targetOpacity;

        if (!show)
        {
            container.style.display = DisplayStyle.None;
        }
    }
    #endregion

    #region Button Management
    public void ShowAllButtons()
    {
        foreach (var kvp in _buttons)
        {
            StartCoroutine(FadeElement(kvp.Value, true));
        }
    }

    private IEnumerator FadeElement(VisualElement element, bool fadeIn, float duration = -1f)
    {
        if (duration < 0) duration = _fadeDuration;

        if (fadeIn)
        {
            element.visible = true;
            element.style.opacity = 0;
        }

        float startOpacity = element.style.opacity.value;
        float targetOpacity = fadeIn ? 1f : 0f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            element.style.opacity = Mathf.Lerp(startOpacity, targetOpacity, elapsed / duration);
            yield return null;
        }

        element.style.opacity = targetOpacity;

        if (!fadeIn)
        {
            element.style.display = DisplayStyle.None;
        }
    }
    #endregion
}