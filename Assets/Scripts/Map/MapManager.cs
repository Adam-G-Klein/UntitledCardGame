using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public MapVariableSO activeMapVariable;
    public EncounterVariableSO activeEncounterVariable;
    public PlayerDataVariableSO playerDataVariable;
    public IntGameEvent setGoldUIEvent;

    void Start() {
        setGoldUIEvent.Raise(playerDataVariable.GetValue().gold);
    }

    public void encounterInitiateEventHandler(string encounterId) {
        activeMapVariable.GetValue().loadEncounterById(encounterId, activeEncounterVariable);
    }

    public void exitGame() {
        Application.Quit();
    }
}
