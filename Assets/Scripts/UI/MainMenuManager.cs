using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField]
    private GameObject optionsUIPrefab;
    [SerializeField]
    private GameStateVariableSO gameState;

    public void startButtonHandler() {
        SceneManager.LoadScene("GenerateMap");
    }

    public void optionsButtonHandler() {
        if (optionsUIPrefab == null) {
            optionsUIPrefab = GameObject.FindGameObjectWithTag("OptionsViewCanvas");
        }
        optionsUIPrefab.GetComponent<OptionsViewController>().ToggleVisibility(true);
    }

    public void exitButtonHandler() {
        Application.Quit();
    }
}
