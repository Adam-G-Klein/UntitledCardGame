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
        SaveStateSerializable saveStateSerializable = new SaveStateSerializable(saveState);
        SaveSystem.Save<SaveStateSerializable>(saveStateSerializable);
    }
    public void LoadHandler() {
        SaveStateSerializable load = SaveSystem.Load<SaveStateSerializable>();
        // instantiate the save state from the serializable data
        SaveState loadState = new SaveState(load, emptyGameStateSO: gameStateVariableSO);
        Debug.Log("[SaveManager] Loaded text: " + loadState.testText);
        textField.SetValueWithoutNotify(loadState.testText);
        LoadActiveEncounter(loadState);
    }

    
    // TODO: load the map itself. Changing encounters won't do anything til we do that
    private void LoadActiveEncounter(SaveState load) {
        // Load the correct encounter for the encounter type
        Encounter activeEncounter = load.gameStateVariableSO.activeEncounter.GetValue();
        gameStateVariableSO.LoadLocationForEncounter(activeEncounter);
    }
}
