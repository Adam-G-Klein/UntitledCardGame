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
    public SaveState(SaveStateSerializable saveStateSerializable, 
        GameStateVariableSO emptyGameStateSO /*GAMESTATE IS EMPTY HERE*/
        ) {
        this.testText = saveStateSerializable.testText;
        this.gameStateVariableSO = emptyGameStateSO;
        switch (saveStateSerializable.activeEncounter.encounterType) {
            case EncounterType.Enemy:
                EnemyEncounterSerializable enemyEncounter = (EnemyEncounterSerializable)saveStateSerializable.activeEncounter;
                this.gameStateVariableSO.activeEncounter.SetValue(enemyEncounter.Deserialize());
                break;
            case EncounterType.Shop:
                ShopEncounterSerializable shopEncounter = (ShopEncounterSerializable)saveStateSerializable.activeEncounter;
                this.gameStateVariableSO.activeEncounter.SetValue(shopEncounter.Deserialize());
                break;
            default:
                Debug.LogError("Unknown encounter type when deserializing: " + saveStateSerializable.activeEncounter.encounterType);
                break;
        }
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
        Encounter encounter = saveState.gameStateVariableSO.activeEncounter.GetValue();
        switch (encounter.encounterType) {
            case EncounterType.Enemy:
                this.activeEncounter = new EnemyEncounterSerializable((EnemyEncounter)encounter);
                break;
            case EncounterType.Shop:
                this.activeEncounter = new ShopEncounterSerializable((ShopEncounter)encounter);
                break;
            default:
                Debug.LogError("Unknown encounter type when serializing: " + encounter.encounterType);
                break;
        }
    }
}