using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Linq;

public class OptionsViewController : GenericSingleton<OptionsViewController>, IControlsReceiver
{
    private Slider musicVolumeSlider;
    private Slider sfxVolumeSlider;
    private Slider timescaleSlider;
    [SerializeField]
    private UIDocument optionsUIDocument;
    [SerializeField]
    private UIDocument compendiumUIDocument;
    [SerializeField]
    private CompanionPoolSO companionPool;
    [SerializeField]
    private CardPoolSO neutralCardPool;
    private CanvasGroup canvasGroup;
    private Canvas canvas;
    private CompendiumView compendiumView;
    private Button backButton;
    private Button quitButton;
    private Button mainMenuButton;
    private Button compendiumButton;
    private Toggle fullscreenToggle;
    private Toggle autoUpgradeToggle;
    [SerializeField]
    private GameStateVariableSO gameState;
    // Start is called before the first frame update
    private Camera mainCamera;
    [SerializeField]
    private GameObject tooltipPrefab;

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
        compendiumButton.clicked += onCompendiumButtonHandler;
        backButton = optionsUIDocument.rootVisualElement.Q<Button>("backButton");
        backButton.clicked += BackButtonHandler;
        mainMenuButton = optionsUIDocument.rootVisualElement.Q<Button>("exitButton");
        mainMenuButton.clicked += onMainMenuButtonHandler;
        quitButton = optionsUIDocument.rootVisualElement.Q<Button>("quitButton");
        quitButton.clicked += onExitGameHandler;
        fullscreenToggle = optionsUIDocument.rootVisualElement.Q<Toggle>("fullscreenToggle");
        fullscreenToggle.value = gameState.fullscreenEnabled;
        fullscreenToggle.RegisterValueChangedCallback(FullScreenToggleEvent);
        autoUpgradeToggle = optionsUIDocument.rootVisualElement.Q<Toggle>("auto-upgrade-toggle");
        autoUpgradeToggle.RegisterValueChangedCallback(AutoUpgradeToggleEvent);
        autoUpgradeToggle.value = gameState.autoUpgrade;

        canvasGroup.blocksRaycasts = false;

        ControlsManager.Instance.RegisterControlsReceiver(this);
    }

    private void OptionsMenuButton() {
        compendiumView?.ExitButtonHandler();
        ToggleVisibility(optionsUIDocument.rootVisualElement.style.visibility == Visibility.Hidden);
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
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        ToggleVisibility();
    }

    public void onExitGameHandler()
    {
        // Quit the game
        SaveManager.Instance.SavePlayerSettings();
        Application.Quit();
    }

    public void onCompendiumButtonHandler() {
        compendiumUIDocument.rootVisualElement.style.visibility = Visibility.Visible;
        compendiumView = null; // in the future we would ideally have some way of tracking if it had to be recreated based on change in gamestate
        compendiumView = new CompendiumView(compendiumUIDocument, companionPool, neutralCardPool, tooltipPrefab);
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
            // Have to do this each time due to how the options menu persists across scenes
            FocusManager.Instance.StashFocusables(this.GetType().Name);
            RegisterFocusables();
            if (ControlsManager.Instance.GetControlMethod() == ControlsManager.ControlMethod.KeyboardController)
                FocusManager.Instance.SetFocusNextFrame(backButton.AsFocusable());
        } else {
            SaveManager.Instance.SavePlayerSettings();
            canvasGroup.blocksRaycasts = false;
            UIDocumentUtils.SetAllPickingMode(optionsUIDocument.rootVisualElement, PickingMode.Ignore);
            optionsUIDocument.rootVisualElement.style.visibility = Visibility.Hidden;
            compendiumUIDocument.rootVisualElement.style.visibility = Visibility.Hidden;
            FocusManager.Instance.UnregisterFocusables(optionsUIDocument);
            FocusManager.Instance.UnstashFocusables(this.GetType().Name);
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

    public void ProcessGFGInputAction(GFGInputAction action)
    {
        if (action == GFGInputAction.OPTIONS) {
            OptionsMenuButton();
        }
    }

    public void SwappedControlMethod(ControlsManager.ControlMethod controlMethod)
    {
        return;
    }
}