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
        fullscreenToggle.value = false;
        fullscreenToggle.RegisterValueChangedCallback(FullScreenToggleEvent);

        canvasGroup.blocksRaycasts = false;
    }

    void Update() {
        // haha gross but lazy bool evaluation is a thing so bite me I guess
        if(Input.GetKeyDown(KeyCode.Escape)) {
            ToggleVisibility(optionsUIDocument.rootVisualElement.style.visibility == Visibility.Hidden);
        }
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
            canvasGroup.blocksRaycasts = true;
            UIDocumentUtils.SetAllPickingMode(optionsUIDocument.rootVisualElement, PickingMode.Position);
            optionsUIDocument.rootVisualElement.style.visibility = Visibility.Visible;
        } else {
            canvasGroup.blocksRaycasts = false;
            UIDocumentUtils.SetAllPickingMode(optionsUIDocument.rootVisualElement, PickingMode.Ignore);
            optionsUIDocument.rootVisualElement.style.visibility = Visibility.Hidden;
            compendiumUIDocument.rootVisualElement.style.visibility = Visibility.Hidden;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
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
        Screen.fullScreen = evt.newValue;
    }
}