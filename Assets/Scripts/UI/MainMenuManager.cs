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
        Screen.SetResolution(1920, 1080, false);
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
        optionsButton = mainMenuUIDocument.rootVisualElement.Q<Button>("optionsButton");
        exitButton = mainMenuUIDocument.rootVisualElement.Q<Button>("exitButton");

        VisualElementUtils.RegisterSelected(startButton, startButtonHandler);
        VisualElementUtils.RegisterSelected(optionsButton, optionsButtonHandler);
        VisualElementUtils.RegisterSelected(exitButton, exitButtonHandler);
        

        FocusManager.Instance.RegisterFocusables(mainMenuUIDocument);
        FocusManager.Instance.SetFocus(startButton.AsFocusable());
    }

    public void startButtonHandler() {
        generateMap.generateMapAndChangeScenes();
        //SceneManager.LoadScene("GenerateMap");
    }

    public void test(string s) {
        Debug.Log(s);
    }

    public void optionsButtonHandler() {
        Debug.Log("OPTIONS MENU BUTTON HANDLER");
        if (optionsUIPrefab == null) {
            optionsUIPrefab = GameObject.FindGameObjectWithTag("OptionsViewCanvas");
        }
        FocusManager.Instance.DisableFocusables(mainMenuUIDocument);
        OptionsViewController optionsViewController = optionsUIPrefab.GetComponent<OptionsViewController>();
        optionsViewController.RegisterOnExitHandler(EnableFocusables);
        optionsViewController.ToggleVisibility(true);
    }

    private void EnableFocusables() {
        FocusManager.Instance.EnableFocusables(mainMenuUIDocument);
        FocusManager.Instance.SetFocusNextFrame(startButton.AsFocusable());
    }

    public void exitButtonHandler() {
        Application.Quit();
    }
}
