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
    public void Awake()
    {
        // there has to be somewhere to call this where it actuallys only runs at start up not every time the player goes back to the main menu
        Screen.SetResolution(1920, 1080, gameState.fullscreenEnabled);
    }

    public void Start()
    {
        gameState.currentLocation = Location.MAIN_MENU;
        StartCoroutine(SetupWhenReady());

    }

    private IEnumerator SetupWhenReady()
    {
        while (!UIDocumentUtils.ElementIsReady(mainMenuUIDocument.rootVisualElement))
        {
            Debug.Log("[MainMenuManager] Main menu UI document is not ready yet, waiting...");
            yield return null;
        }
        Setup();
    }

    public void Setup() {
        startButton = mainMenuUIDocument.rootVisualElement.Q<Button>("startButton");
        startButton.RegisterCallback<ClickEvent>(ev => startButtonHandler());
        UIDocumentHoverableInstantiator.Instance.InstantiateHoverableWhenUIElementReady(startButton, startButtonHandler);
        optionsButton = mainMenuUIDocument.rootVisualElement.Q<Button>("optionsButton");
        optionsButton.RegisterCallback<ClickEvent>(ev => optionsButtonHandler());
        exitButton = mainMenuUIDocument.rootVisualElement.Q<Button>("exitButton");
        exitButton.RegisterCallback<ClickEvent>(ev => exitButtonHandler());
        UIDocumentHoverableInstantiator.Instance.InstantiateHoverableWhenUIElementReady(exitButton, exitButtonHandler);

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
