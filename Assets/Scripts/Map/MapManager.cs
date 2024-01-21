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
        gameState.map.GetValue().loadEncounterById(encounterId, gameState.activeEncounter);
    }

    public void exitGame() {
        Application.Quit();
    }
}
