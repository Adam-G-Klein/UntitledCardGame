using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Linq;

public class OptionsViewController : MonoBehaviour
{
    private static OptionsViewController instance;
    private Slider volumeSlider;
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

    void Awake() {
        if (instance != null && instance != this) {
            Destroy(this.gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this.gameObject);
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponent<Canvas>();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        optionsUIDocument.rootVisualElement.style.visibility = Visibility.Hidden;
        compendiumUIDocument.rootVisualElement.style.visibility = Visibility.Hidden;
        volumeSlider = optionsUIDocument.rootVisualElement.Q<Slider>("volumeSlider");
        volumeSlider.RegisterValueChangedCallback((evt) => onVolumeSliderChangedHandler(evt.newValue));
        timescaleSlider = optionsUIDocument.rootVisualElement.Q<Slider>("gameSpeedSlider");
        timescaleSlider.RegisterValueChangedCallback((evt) => OnTimescaleSliderChange(evt.newValue));
        volumeSlider.value = MusicController2.Instance.currentVolume;
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
    }

    void Update() {
        // haha gross but lazy bool evaluation is a thing so bite me I guess
        if(Input.GetKeyDown(KeyCode.Escape)) {
            compendiumView?.ExitButtonHandler();
            ToggleVisibility(optionsUIDocument.rootVisualElement.style.visibility == Visibility.Hidden);
            // If the player just hits escape with it open, then it breaks focusing unless we do this
        }
    }

    private void RegisterFocusables() {
        VisualElementFocusable volumeSliderFocusable = new VisualElementFocusable(volumeSlider);
        volumeSliderFocusable.SetInputAction(GFGInputAction.LEFT, () => VisualElementUtils.ProcessSliderInput(volumeSlider, GFGInputAction.LEFT));
        volumeSliderFocusable.SetInputAction(GFGInputAction.RIGHT, () => VisualElementUtils.ProcessSliderInput(volumeSlider, GFGInputAction.RIGHT));
        VisualElementFocusable timescaleSliderFocusable = new VisualElementFocusable(timescaleSlider);
        timescaleSliderFocusable.SetInputAction(GFGInputAction.LEFT, () => VisualElementUtils.ProcessSliderInput(timescaleSlider, GFGInputAction.LEFT));
        timescaleSliderFocusable.SetInputAction(GFGInputAction.RIGHT, () => VisualElementUtils.ProcessSliderInput(timescaleSlider, GFGInputAction.RIGHT));
        FocusManager.Instance.RegisterFocusableTarget(volumeSliderFocusable);
        FocusManager.Instance.RegisterFocusableTarget(timescaleSliderFocusable);
        FocusManager.Instance.RegisterFocusables(optionsUIDocument);
    }

    public void onMainMenuButtonHandler() {
        // Load the main menu scene
        MusicController2.Instance.PrepareForGoingBackToMainMenu();
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        ToggleVisibility();
    }

    public void onExitGameHandler() {
        // Quit the game
        Application.Quit();
    }

    public void onCompendiumButtonHandler() {
        compendiumUIDocument.rootVisualElement.style.visibility = Visibility.Visible;
        compendiumView = null; // in the future we would ideally have some way of tracking if it had to be recreated based on change in gamestate
        compendiumView = new CompendiumView(compendiumUIDocument, companionPool, neutralCardPool, tooltipPrefab);
    }

    public void onVolumeSliderChangedHandler(float value) {
       MusicController2.Instance.SetVolume(value);
    }
    
    public void OnTimescaleSliderChange(float value) {
        Time.timeScale = 1 + (value * 4);
    }

    private void BackButtonHandler() {
        ToggleVisibility();
    }

    public void ToggleVisibility(bool enable = false) {
        if (enable) {
            autoUpgradeToggle.value = gameState.autoUpgrade; // this is updated elsewhere so we need to make sure it's consistent with the value in the game state
            canvasGroup.blocksRaycasts = true;
            UIDocumentUtils.SetAllPickingMode(optionsUIDocument.rootVisualElement, PickingMode.Position);
            optionsUIDocument.rootVisualElement.style.visibility = Visibility.Visible;
            // Have to do this each time due to how the options menu persists across scenes
            FocusManager.Instance.StashFocusables(this.GetType().Name);
            RegisterFocusables();
            FocusManager.Instance.SetFocusNextFrame(backButton.AsFocusable());
        } else {
            canvasGroup.blocksRaycasts = false;
            UIDocumentUtils.SetAllPickingMode(optionsUIDocument.rootVisualElement, PickingMode.Ignore);
            optionsUIDocument.rootVisualElement.style.visibility = Visibility.Hidden;
            compendiumUIDocument.rootVisualElement.style.visibility = Visibility.Hidden;
            FocusManager.Instance.UnregisterFocusables(optionsUIDocument);
            FocusManager.Instance.UnstashFocusables(this.GetType().Name);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        // RegisterFocusables();
        UpdateCameraReference();
        ToggleVisibility();
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
}