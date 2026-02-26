using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.Rendering.Universal;

public class OptionsViewController : GenericSingleton<OptionsViewController>, IControlsReceiver
{
    private Slider musicVolumeSlider;
    private Slider sfxVolumeSlider;
    private Slider timescaleSlider;
    [SerializeField]
    private UniversalRenderPipelineAsset pipelineAsset;
    [SerializeField]
    private UIDocument optionsUIDocument;
    [SerializeField]
    private UIDocument compendiumUIDocument;
    [SerializeField]
    private CompanionPoolSO companionPool;
    [SerializeField]
    private CardPoolSO neutralCardPool;
    [SerializeField]
    private CanvasGroup canvasGroup;
    [SerializeField]
    private List<PackSO> packSOs;
    [SerializeField]
    private GameObject creditsPrefab;
    private Canvas canvas;
    private CompendiumView compendiumView;
    private CreditsView creditsView = null;
    private Button backButton;
    private Button quitButton;
    private Button mainMenuButton;
    private Button compendiumButton;
    private Button creditsButton;
    private Toggle fullscreenToggle;
    private Toggle autoUpgradeToggle;
    private Toggle dataConsentToggle;
    private DropdownField renderScaleDropdown;
    private DropdownField antiAliasingDropdown;
    [SerializeField]
    private GameStateVariableSO gameState;
    // Start is called before the first frame update
    private Camera mainCamera;
    [SerializeField]
    private GameObject tooltipPrefab;
    public delegate void EnterExitVoidHandler();
    private event EnterExitVoidHandler onViewEnterHandler;
    private event EnterExitVoidHandler onViewExitHandler;

    public float getMusicVolumeSliderValue() {
        return musicVolumeSlider.value;
    }
    public void setMusicVolumeSliderValue(float value) {
        musicVolumeSlider.value = value;
    }

    public float getSFXVolumeSliderValue() {
        return sfxVolumeSlider.value;
    }
    public void setSFXVolumeSliderValue(float value)
    {
        sfxVolumeSlider.value = value;
    }
    public float getGameSpeedSliderValue() {
        return timescaleSlider.value;
    }
    public void setGameSpeedSliderValue(float value) {
        timescaleSlider.value = value;
    }
    public bool getIsFullScreened() {
        return gameState.fullscreenEnabled;
    }
    public void setIsFullScreened(bool value) {
        gameState.fullscreenEnabled = value;
    }
    public bool getIsAutoUpgradeEnabled() {
        return gameState.autoUpgrade;
    }
    public void setIsAutoUpgradeEnabled(bool value) {
        gameState.autoUpgrade = value;
    }

    public bool getIsDataCollectionEnabled() {
        return gameState.consentToDataCollection;
    }
    public void setIsDataCollectionEnabled(bool value) {
        gameState.consentToDataCollection = value;
    }

    public int getRenderScaleDropdown() {
        return renderScaleDropdown.index;
    }

    public void setRenderScaleDropdown(int index) {
        renderScaleDropdown.index = index;
    }

    void Awake() {
        // DontDestroyOnLoad(this.gameObject);
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponent<Canvas>();
        optionsUIDocument.enabled = true;
        compendiumUIDocument.enabled = true;
    }

    void Start()
    {
        optionsUIDocument.rootVisualElement.style.visibility = Visibility.Hidden;
        compendiumUIDocument.rootVisualElement.style.visibility = Visibility.Hidden;
        musicVolumeSlider = optionsUIDocument.rootVisualElement.Q<Slider>("musicVolumeSlider");
        musicVolumeSlider.RegisterValueChangedCallback((evt) => onVolumeSliderChangedHandler(evt.newValue, VolumeType.MUSIC));
        sfxVolumeSlider = optionsUIDocument.rootVisualElement.Q<Slider>("sfxVolumeSlider");
        sfxVolumeSlider.RegisterValueChangedCallback((evt) => onVolumeSliderChangedHandler(evt.newValue, VolumeType.SFX));
        timescaleSlider = optionsUIDocument.rootVisualElement.Q<Slider>("gameSpeedSlider");
        timescaleSlider.RegisterValueChangedCallback((evt) => OnTimescaleSliderChange(evt.newValue));
        musicVolumeSlider.value = MusicController.Instance.currentMusicVolume;
        sfxVolumeSlider.value = MusicController.Instance.currentSFXVolume;
        compendiumButton = optionsUIDocument.rootVisualElement.Q<Button>("compendiumButton");
        compendiumButton.RegisterOnSelected(onCompendiumButtonHandler);
        backButton = optionsUIDocument.rootVisualElement.Q<Button>("backButton");
        backButton.RegisterOnSelected(BackButtonHandler);
        mainMenuButton = optionsUIDocument.rootVisualElement.Q<Button>("exitButton");
        mainMenuButton.RegisterOnSelected(onMainMenuButtonHandler);
        quitButton = optionsUIDocument.rootVisualElement.Q<Button>("quitButton");
        quitButton.RegisterOnSelected(onExitGameHandler);
        creditsButton = optionsUIDocument.rootVisualElement.Q<Button>("creditsButton");
        creditsButton.RegisterOnSelected(onCreditsHandler);
        fullscreenToggle = optionsUIDocument.rootVisualElement.Q<Toggle>("fullscreenToggle");
        fullscreenToggle.value = gameState.fullscreenEnabled;
        fullscreenToggle.RegisterValueChangedCallback(FullScreenToggleEvent);
        autoUpgradeToggle = optionsUIDocument.rootVisualElement.Q<Toggle>("auto-upgrade-toggle");
        autoUpgradeToggle.RegisterValueChangedCallback(AutoUpgradeToggleEvent);
        autoUpgradeToggle.value = gameState.autoUpgrade;
        dataConsentToggle = optionsUIDocument.rootVisualElement.Q<Toggle>("data-consent-toggle");
        dataConsentToggle.RegisterValueChangedCallback(DataConsentToggleEvent);
        dataConsentToggle.value = gameState.consentToDataCollection;
        renderScaleDropdown = optionsUIDocument.rootVisualElement.Q<DropdownField>("render-scale-dropdown");
        antiAliasingDropdown = optionsUIDocument.rootVisualElement.Q<DropdownField>("anti-aliasing-dropdown");
        renderScaleDropdown.RegisterValueChangedCallback(RenderScaleDropdownChanged);
        antiAliasingDropdown.RegisterValueChangedCallback(AntiAliasingDropdownChanged);

        canvasGroup.blocksRaycasts = false;

        ControlsManager.Instance.RegisterControlsReceiver(this);
        MusicController.Instance.RegisterButtonClickSFX(optionsUIDocument);
    }

    private void OptionsMenuButton()
    {
        if (compendiumView != null) compendiumView.ExitButtonHandler();
        else if (creditsView != null) creditsView.OnCompleted();
        else ToggleVisibility(optionsUIDocument.rootVisualElement.style.visibility == Visibility.Hidden);
    }

    private void RegisterFocusables() {
        VisualElementFocusable musicVolumeSliderFocusable = musicVolumeSlider.AsFocusable();
        musicVolumeSliderFocusable.SetInputAction(GFGInputAction.LEFT, () => VisualElementUtils.ProcessSliderInput(musicVolumeSlider, GFGInputAction.LEFT));
        musicVolumeSliderFocusable.SetInputAction(GFGInputAction.RIGHT, () => VisualElementUtils.ProcessSliderInput(musicVolumeSlider, GFGInputAction.RIGHT));
        VisualElementFocusable sfxVolumeSliderFocusable = sfxVolumeSlider.AsFocusable();
        sfxVolumeSliderFocusable.SetInputAction(GFGInputAction.LEFT, () => VisualElementUtils.ProcessSliderInput(sfxVolumeSlider, GFGInputAction.LEFT));
        sfxVolumeSliderFocusable.SetInputAction(GFGInputAction.RIGHT, () => VisualElementUtils.ProcessSliderInput(sfxVolumeSlider, GFGInputAction.RIGHT));
        VisualElementFocusable timescaleSliderFocusable = timescaleSlider.AsFocusable();
        timescaleSliderFocusable.SetInputAction(GFGInputAction.LEFT, () => VisualElementUtils.ProcessSliderInput(timescaleSlider, GFGInputAction.LEFT));
        timescaleSliderFocusable.SetInputAction(GFGInputAction.RIGHT, () => VisualElementUtils.ProcessSliderInput(timescaleSlider, GFGInputAction.RIGHT));
        FocusManager.Instance.RegisterFocusables(optionsUIDocument);
    }

    public void onMainMenuButtonHandler() {
        // Load the main menu scene
        MusicController.Instance.PrepareForGoingBackToMainMenu();
        // UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        SceneTransitionManager.LoadScene("MainMenu");
        ToggleVisibility();
    }

    public void onExitGameHandler()
    {
        SaveManager.Instance.SavePlayerSettings();
        if (gameState.BuildTypeDemoOrConvention()) {
            SceneTransitionManager.LoadScene("DemoExitGameMenu");
            return;
        }
        Application.Quit();
    }

    public void onCreditsHandler() {
        creditsView = null;
        FocusManager.Instance.StashLockedFocusables(this.GetType().Name);
        GameObject creditsGO = Instantiate(creditsPrefab, Vector3.zero, Quaternion.identity);
        creditsView = creditsGO.GetComponent<CreditsView>();
        creditsView.Init(creditsFinishedHandler);
    }

    public void creditsFinishedHandler() {
        creditsView = null;
        SetFocusOnBackButton();
    }

    public void SetFocusOnBackButton() {
        if (ControlsManager.Instance.GetControlMethod() == ControlsManager.ControlMethod.KeyboardController) {
            FocusManager.Instance.SetFocus(backButton.AsFocusable());
        }
    }

    public void onCompendiumButtonHandler()
    {
        compendiumView = null;
        compendiumUIDocument.rootVisualElement.style.display = DisplayStyle.Flex;
        compendiumUIDocument.rootVisualElement.style.visibility = Visibility.Visible;
        FocusManager.Instance.StashLockedFocusables(this.GetType().Name);
        FocusManager.Instance.UnlockFocusables();
        compendiumView = new CompendiumView(compendiumUIDocument, companionPool, neutralCardPool, packSOs, tooltipPrefab, ProgressManager.Instance.progressSO.unlockedCards);
    }
    
    public void onCloseCompenidum()
    {
        compendiumView = null;
        SetFocusOnBackButton();
    }

    public void onVolumeSliderChangedHandler(float value, VolumeType volumeType) {
       MusicController.Instance.SetVolume(value, volumeType);
    }

    public void OnTimescaleSliderChange(float value) {
        Time.timeScale = 1 + (value * 4);
    }

    private void BackButtonHandler() {
        ToggleVisibility();
    }

    public void ToggleVisibility(bool enable = false) {
        if (enable) {
            UpdateCameraReference();
            autoUpgradeToggle.value = gameState.autoUpgrade; // this is updated elsewhere so we need to make sure it's consistent with the value in the game state
            canvasGroup.blocksRaycasts = true;
            UIDocumentUtils.SetAllPickingMode(optionsUIDocument.rootVisualElement, PickingMode.Position);
            optionsUIDocument.rootVisualElement.style.visibility = Visibility.Visible;
            FocusManager.Instance.StashFocusables(this.GetType().Name);
            RegisterFocusables();
            FocusManager.Instance.LockFocusables(); // This has to go after we register the options view focusables
            if (ControlsManager.Instance.GetControlMethod() == ControlsManager.ControlMethod.KeyboardController)
                FocusManager.Instance.SetFocusNextFrame(backButton.AsFocusable());
            onViewEnterHandler?.Invoke();
        } else {
            SaveManager.Instance.SavePlayerSettings();
            canvasGroup.blocksRaycasts = false;
            UIDocumentUtils.SetAllPickingMode(optionsUIDocument.rootVisualElement, PickingMode.Ignore);
            optionsUIDocument.rootVisualElement.style.visibility = Visibility.Hidden;
            compendiumUIDocument.rootVisualElement.style.visibility = Visibility.Hidden;
            FocusManager.Instance.UnregisterFocusables(optionsUIDocument);
            FocusManager.Instance.UnstashFocusables(this.GetType().Name);
            FocusManager.Instance.UnlockFocusables();
            onViewExitHandler?.Invoke();
        }
    }

    private void UpdateCameraReference() {
        mainCamera = Camera.main;
        if (mainCamera != null) {
            Debug.Log("Main camera updated: " + mainCamera.name);
        } else {
            Debug.LogWarning("Main camera not found");
        }
        canvas.worldCamera = mainCamera;
    }

    private void FullScreenToggleEvent(ChangeEvent<bool> evt) {
        gameState.fullscreenEnabled = evt.newValue;
        Screen.SetResolution(1920, 1080, gameState.fullscreenEnabled);
    }

    private void AutoUpgradeToggleEvent(ChangeEvent<bool> evt) {
        gameState.autoUpgrade = evt.newValue;
    }

    private void DataConsentToggleEvent(ChangeEvent<bool> evt)
    {
        gameState.consentToDataCollection = evt.newValue;
        AnalyticsManager.Instance.ConfigureDataCollection();
    }

    public void ProcessGFGInputAction(GFGInputAction action)
    {
        if (action == GFGInputAction.OPTIONS)
        {
            OptionsMenuButton();
        }
    }

    public void SwappedControlMethod(ControlsManager.ControlMethod controlMethod)
    {
        return;
    }

    public void SetEnterHandler(EnterExitVoidHandler handler) {
        onViewEnterHandler += handler;
    }

    public void SetExitHandler(EnterExitVoidHandler handler) {
        onViewExitHandler += handler;
    }

    public void RenderScaleDropdownChanged(ChangeEvent<string> evt) {
        switch (evt.newValue) {
            case "1.0":
                pipelineAsset.renderScale = 1.0f;
            break;

            case "1.5":
                pipelineAsset.renderScale = 1.5f;
            break;

            case "2.0":
                pipelineAsset.renderScale = 2f;
            break;
        }
    }

    public void AntiAliasingDropdownChanged(ChangeEvent<string> evt) {
        switch (evt.newValue) {
            case "Disabled":
                pipelineAsset.msaaSampleCount = 1;
            break;

            case "2x":
                pipelineAsset.msaaSampleCount = 2;
            break;

            case "4x":
                pipelineAsset.msaaSampleCount = 4;
            break;

            case "8x":
                pipelineAsset.msaaSampleCount = 8;
            break;
        }
    }
}