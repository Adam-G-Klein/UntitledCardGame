using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public GameStateVariableSO gameState;
    public IntGameEvent setGoldUIEvent;

    void Start() {
        setGoldUIEvent.Raise(gameState.playerData.GetValue().gold);
    }

    public void encounterInitiateEventHandler(string encounterId) {
        Encounter nextEncounter = gameState.map
            .GetValue().getEncounterById(encounterId);
        gameState.LoadNextLocation(nextEncounter.ToLocation());
    }

    public void exitGame() {
        Application.Quit();
    }
}
