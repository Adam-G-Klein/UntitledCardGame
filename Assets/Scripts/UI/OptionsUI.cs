using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionsUI : MonoBehaviour
{
    [SerializeField]
    private TMP_Dropdown fullscreenDropdown;
    [SerializeField]
    private TMP_Dropdown resolutionDropdown;

    [SerializeField]
    private Slider volumeSlider;
    [SerializeField]
    private Slider timescaleSlider;

    // Start is called before the first frame update
    void Start()
    {
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
        volumeSlider.value = MusicController2.Instance.currentVolume;
    }

    public void onMainMenuButtonHandler() {
        // Load the main menu scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public void onExitGameHandler() {
        // Quit the game
        Application.Quit();
    }

    public void onVolumeSliderChangedHandler(float value) {
        MusicController2.Instance.SetVolume(value);
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
