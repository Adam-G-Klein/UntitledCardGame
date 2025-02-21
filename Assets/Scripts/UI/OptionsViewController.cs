using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UIElements;

public class OptionsViewController : MonoBehaviour
{

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
    private CompendiumView compendiumView;
    private Button backButton;
    private Button quitButton;
    private Button mainMenuButton;
    private Button compendiumButton;
    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        optionsUIDocument.rootVisualElement.style.visibility = Visibility.Hidden;
        compendiumUIDocument.rootVisualElement.style.visibility = Visibility.Hidden;
        Debug.LogError("here");

        volumeSlider = optionsUIDocument.rootVisualElement.Q<Slider>("volumeSlider");
        volumeSlider.RegisterValueChangedCallback((evt) => onVolumeSliderChangedHandler(evt.newValue));
        timescaleSlider = optionsUIDocument.rootVisualElement.Q<Slider>("gameSpeedSlider");
        timescaleSlider.RegisterValueChangedCallback((evt) => OnTimescaleSliderChange(evt.newValue));
        volumeSlider.value = MusicController2.Instance.currentVolume;
        compendiumButton = optionsUIDocument.rootVisualElement.Q<Button>("compendiumButton");
        compendiumButton.clicked += onCompendiumButtonHandler;
        backButton = optionsUIDocument.rootVisualElement.Q<Button>("backButton");
        backButton.clicked += exitButtonHandler;
        mainMenuButton = optionsUIDocument.rootVisualElement.Q<Button>("exitButton");
        mainMenuButton.clicked += onMainMenuButtonHandler;
        quitButton = optionsUIDocument.rootVisualElement.Q<Button>("quitButton");
        quitButton.clicked += onExitGameHandler;
    }

    void Update() {
        // haha gross but lazy bool evaluation is a thing so bite me I guess
        if(Input.GetKeyDown(KeyCode.Escape)) {
            ToggleVisibility();
        }
    }

    public void onMainMenuButtonHandler() {
        // Load the main menu scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        exitButtonHandler();
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

    public void exitButtonHandler() {
        optionsUIDocument.rootVisualElement.style.visibility = Visibility.Hidden;
        compendiumUIDocument.rootVisualElement.style.visibility = Visibility.Hidden;
    }

    public void ToggleVisibility() {
        if (optionsUIDocument.rootVisualElement.style.visibility == Visibility.Hidden) {
            optionsUIDocument.rootVisualElement.style.visibility = Visibility.Visible;
        } else {
            exitButtonHandler();
        }
    }
}