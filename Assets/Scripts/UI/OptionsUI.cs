using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.UIElements;

public class OptionsUI : MonoBehaviour
{
    [SerializeField]
    private TMP_Dropdown fullscreenDropdown;
    [SerializeField]
    private TMP_Dropdown resolutionDropdown;

    [SerializeField]
    private UnityEngine.UI.Slider volumeSlider;
    [SerializeField]
    private UnityEngine.UI.Slider timescaleSlider;
    [SerializeField]
    private UIDocument compendiumUIDocument;
    [SerializeField]
    private CompanionPoolSO companionPool;
    [SerializeField]
    private CardPoolSO neutralCardPool;
    private CompendiumView compendiumView;

    [SerializeField] FMODUnity.EventReference fmodMixer;
    private FMOD.Studio.EventInstance mixerInstance;

    // Start is called before the first frame update
    void Start()
    {
        compendiumUIDocument.rootVisualElement.style.visibility = Visibility.Hidden;
        GetComponent<Canvas>().worldCamera = Camera.main;
        if (Screen.fullScreen) {
            // Fix just setting the index with a magic number
            fullscreenDropdown.value = 0;
            resolutionDropdown.interactable = false;
        } else {
            // Fix just setting the index with a magic number
            fullscreenDropdown.value = 1;
            resolutionDropdown.interactable = true;
        }

        mixerInstance = FMODUnity.RuntimeManager.CreateInstance(fmodMixer);
        mixerInstance.start();
        //volumeSlider.value = MusicController2.Instance.currentVolume;
    }

    public void onMainMenuButtonHandler() {
        // Load the main menu scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public void onExitGameHandler() {
        // Quit the game
        Application.Quit();
    }

    // public void onCompendiumButtonHandler() {
    //     Debug.LogError("Compendium button clicked");
    //     compendiumUIDocument.rootVisualElement.style.visibility = Visibility.Visible;
    //     compendiumView = null; // in the future we would ideally have some way of tracking if it had to be recreated based on change in gamestate
    //     // compendiumView = new CompendiumView(compendiumUIDocument, companionPool, neutralCardPool, tooltipPrefab);
    // }

    public void onVolumeSliderChangedHandler(float value) {
        //MusicController2.Instance.SetVolume(value);
        mixerInstance.setParameterByName("Master_Volume", value);
    }

    public void onFullscreenChangedHandler(int index) {
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
    }

    public void OnTimescaleSliderChange(float value) {
        Time.timeScale = 1 + (value * 4);
    }

    public void exitButtonHandler() {
        Destroy(this.gameObject);
    }
}