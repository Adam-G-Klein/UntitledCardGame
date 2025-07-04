using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/*
At the end of this, we'll want to be saving and loading one "gameSave" object, which will contain all the data we want to save.

*/
public class SaveManager : GenericSingleton<SaveManager>
{

    public GameStateVariableSO gameStateVariableSO;
    public SORegistry soRegistry;
    // Options ui doc just for testing
    // Will obviously have other hooks in the future
    public UIDocument uidoc;
    private TextField textField;
    private Button saveButton;
    private Button loadButton;

    // Start is called before the first frame update
    void Start()
    {
        GameObject optionsGO = GameObject.FindGameObjectWithTag("OptionsUIDoc");
        if (optionsGO == null) {
            Debug.LogError("[SaveManager] optionsGO is null!");
        } else {
            Debug.Log("[SaveManager] optionsGO found: " + optionsGO.name);
        }
        uidoc = optionsGO.GetComponent<UIDocument>();
        if (uidoc == null) {
            Debug.LogError("[SaveManager] uidoc is null!");
        } else {
            Debug.Log("[SaveManager] uidoc found: " + uidoc.name);
        }
        textField = uidoc.rootVisualElement.Q<TextField>("SaveSystemTextField");
        if (textField == null) {
            Debug.LogError("[SaveManager] textField is null!");
        } else {
            Debug.Log("[SaveManager] textField found: " + textField.name);
        }
        saveButton = uidoc.rootVisualElement.Q<Button>("Save");
        loadButton = uidoc.rootVisualElement.Q<Button>("Load");

        saveButton.RegisterCallback<ClickEvent>(ev => SaveHandler());
        loadButton.RegisterCallback<ClickEvent>(ev => LoadHandler());
        
    }

    public void SaveHandler() {
        SaveState saveState = new SaveState(textField.text, gameStateVariableSO);
        SaveSystem.Save<SaveState>(saveState);
    }

    public void LoadHandler()
    {
        Debug.Log("HELLO THE LOAD HANDLER BUTTON WAS PRESSED FOR SURE");
        SaveState loadState = SaveSystem.Load<SaveState>();
        // instantiate the save state from the serializable data
        loadState.LoadToGameState(gameStateVariableSO, soRegistry);
        // print out a buncha companion information
        textField.value = loadState.SaveName;
        foreach (Companion companion in gameStateVariableSO.companions.activeCompanions)
        {
            Debug.Log("CompanionType: " + companion.companionType.name + " loaded");
        }

        foreach (Encounter encounter in gameStateVariableSO.map.GetValue().encounters)
        {
            Debug.Log("EncounterType: " + encounter.getEncounterType() + " loaded");
        }

        gameStateVariableSO.LoadNextLocationFromLoadingSave();
    }
}