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
    [SerializeField]
    private UIDocument mainMenuUIDocument;
    private Button startButton;
    private Button optionsButton;
    private Button exitButton;
    [SerializeField]
    private GenerateMap generateMap;

    public void Start()
    {
        startButton = mainMenuUIDocument.rootVisualElement.Q<Button>("startButton");
        startButton.RegisterCallback<ClickEvent>(ev => startButtonHandler());
        optionsButton = mainMenuUIDocument.rootVisualElement.Q<Button>("optionsButton");
        optionsButton.RegisterCallback<ClickEvent>(ev => optionsButtonHandler());
        exitButton = mainMenuUIDocument.rootVisualElement.Q<Button>("exitButton");
        exitButton.RegisterCallback<ClickEvent>(ev => exitButtonHandler());
    }

    public void startButtonHandler() {
        generateMap.generateMapAndChangeScenes();
        //SceneManager.LoadScene("GenerateMap");
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
