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
    // Start is called before the first frame update
    private Camera mainCamera;

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
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        ToggleVisibility();
    }

    public void onExitGameHandler() {
        // Quit the game
        Application.Quit();
    }

    public void onCompendiumButtonHandler() {
        Debug.LogError("Compendium button clicked");
        compendiumUIDocument.rootVisualElement.style.visibility = Visibility.Visible;
        compendiumView = null; // in the future we would ideally have some way of tracking if it had to be recreated based on change in gamestate
        compendiumView = new CompendiumView(compendiumUIDocument, companionPool, neutralCardPool);
    }

    public void onVolumeSliderChangedHandler(float value) {
        MusicController2.Instance.SetVolume(value);
    }

    /*public void onFullscreenChangedHandler(int index) {
        string option = fullscreenDropdown.options[index].text;
        
        switch(option) {
            case "Enabled":
                Screen.fullScreen = true;
                resolutionDropdown.interactable = false;
            break;

            case "Disabled":
                Screen.fullScreen = false;
                resolutionDropdown.interactable = true;
            break;
        }
    }

    public void onResolutionChangedHandler(int index) {
        string option = resolutionDropdown.options[index].text;

        int width = Int32.Parse(option.Split("x")[0]);
        int height = Int32.Parse(option.Split("x")[1]);
        Screen.SetResolution(width, height, Screen.fullScreen);
    }*/
    
    public void OnTimescaleSliderChange(float value) {
        Time.timeScale = 1 + (value * 4);
    }

    private void BackButtonHandler() {
        ToggleVisibility();
    }

    public void ToggleVisibility(bool enable = false) {
        if (enable) {
            Debug.Log("toggling options visible");
            canvasGroup.blocksRaycasts = true;
            UIDocumentUtils.SetAllPickingMode(optionsUIDocument.rootVisualElement, PickingMode.Position);
            optionsUIDocument.rootVisualElement.style.visibility = Visibility.Visible;
            // get all UIDocuments in the scene
            List<UIDocument> documents = FindObjectsOfType<UIDocument>().Where(doc => doc != optionsUIDocument && doc != compendiumUIDocument).ToList();
            ToggleUIDocs(documents, true);
        } else {
            Debug.Log("toggling options hidden");
            canvasGroup.blocksRaycasts = false;
            UIDocumentUtils.SetAllPickingMode(optionsUIDocument.rootVisualElement, PickingMode.Ignore);
            optionsUIDocument.rootVisualElement.style.visibility = Visibility.Hidden;
            compendiumUIDocument.rootVisualElement.style.visibility = Visibility.Hidden;
            List<UIDocument> documents = FindObjectsOfType<UIDocument>().Where(doc => doc != optionsUIDocument && doc != compendiumUIDocument).ToList();
            ToggleUIDocs(documents, false);
        }
    }

    private void ToggleUIDocs(List<UIDocument> documents, bool inMenu) {
        String currentSceneName = SceneManager.GetActiveScene().name;
        if (currentSceneName == "CombatScene") {
            EnemyEncounterViewModel.Instance.SetInMenu(inMenu);
            foreach (UIDocument doc in documents) {
                if (doc != null && doc.rootVisualElement != null && (doc.name == "EndCombatScreen" || doc.name == "VictoryPopUp" || doc.name == "DefeatPopup")) {
                    UIDocumentUtils.SetAllPickingMode(doc.rootVisualElement, inMenu ? PickingMode.Ignore : PickingMode.Position);
            }
        }
            return;
        } 
        if (currentSceneName == "PlaceholderShopEncounter") {
            ShopManager.Instance.shopViewController.SetDisplayStyle(!inMenu); // super jank rn
            return;
        }
        foreach (UIDocument doc in documents) {
            if (doc != null && doc.rootVisualElement != null) {
                UIDocumentUtils.SetAllPickingMode(doc.rootVisualElement, inMenu ? PickingMode.Ignore : PickingMode.Position);
            }
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
}