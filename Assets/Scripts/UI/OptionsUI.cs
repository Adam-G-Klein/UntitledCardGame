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

    public void exitButtonHandler() {
        Destroy(this.gameObject);
    }
}
