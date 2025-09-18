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
    private Button continueButton;
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
        MusicController.Instance.RegisterButtonClickSFX(mainMenuUIDocument);
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
        continueButton = mainMenuUIDocument.rootVisualElement.Q<Button>("continueButton");
        optionsButton = mainMenuUIDocument.rootVisualElement.Q<Button>("optionsButton");
        exitButton = mainMenuUIDocument.rootVisualElement.Q<Button>("exitButton");

        startButton.RegisterOnSelected(startButtonHandler);
        continueButton.RegisterOnSelected(ContinueButtonHandler);
        optionsButton.RegisterOnSelected(optionsButtonHandler);
        exitButton.RegisterOnSelected(exitButtonHandler);

        FocusManager.Instance.RegisterFocusables(mainMenuUIDocument);
        
        if (!SaveManager.Instance.DoesSaveGameExist()) {
            continueButton.SetEnabled(false);
            FocusManager.Instance.UnregisterFocusableTarget(continueButton.AsFocusable());
        }
    }

    public void startButtonHandler() {
        if (gameState.demoMode) {
            SaveManager.Instance.DeletePlayerProgressData();
        }
        generateMap.generateMapAndChangeScenes();
        MusicController.Instance.PlayStartSFX();
    }

    private void ContinueButtonHandler()
    {
        SaveManager.Instance.LoadHandler();
    }

    public void test(string s) {
        Debug.Log(s);
    }

    public void optionsButtonHandler() {
        if (optionsUIPrefab == null)
        {
            optionsUIPrefab = GameObject.FindGameObjectWithTag("OptionsViewCanvas");
        }
        OptionsViewController optionsViewController = optionsUIPrefab.GetComponent<OptionsViewController>();
        optionsViewController.ToggleVisibility(true);
    }

    public void exitButtonHandler() {
        Application.Quit();
    }
}
