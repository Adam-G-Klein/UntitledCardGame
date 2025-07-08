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

    // Start is called before the first frame update
    void Start() { }

    public void SaveHandler() {
        SaveState saveState = new SaveState("save game", gameStateVariableSO);
        SaveSystem.Save<SaveState>(saveState);
    }

    public void LoadHandler()
    {
        Debug.Log("HELLO THE LOAD HANDLER BUTTON WAS PRESSED FOR SURE");
        SaveState loadState = SaveSystem.Load<SaveState>();
        // instantiate the save state from the serializable data
        loadState.LoadToGameState(gameStateVariableSO, soRegistry);
        // print out a buncha companion information
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

    public bool DoesSaveGameExist() {
        return SaveSystem.SaveExists();
    }

    [ContextMenu("Delete Save Data")]
    public void DeleteSaveData() {
        SaveSystem.DeleteSave();
    }
}