using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
The non serialized game state
*/
public class SaveState 
{
    public string testText;
    public GameStateVariableSO gameStateVariableSO;

    public SaveState(string testText, GameStateVariableSO gameStateVariableSO) {
        this.testText = testText;
        this.gameStateVariableSO = gameStateVariableSO;
    }

    // The constructor used for LOADING from serialized data on disk
    public SaveState(SaveStateSerializable saveStateSerializable) {
        this.testText = saveStateSerializable.testText;
        this.gameStateVariableSO.
    }
}

[System.Serializable]
public class SaveStateSerializable
{
    public string testText;
    public EncounterSerializable activeEncounter;

    // The constructor used for SAVING to serialized data on disk
    public SaveStateSerializable(SaveState saveState) {
        this.testText = saveState.testText;
        this.activeEncounter = saveState.gameStateVariableSO.activeEncounter.GetValue().GetSerializableData();
    }
}