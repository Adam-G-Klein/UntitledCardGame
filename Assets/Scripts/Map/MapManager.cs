using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public MapReference mapReference;
    public EncounterVariableSO activeEncounterVariable;
    public PlayerDataReference playerDataReference;
    public IntGameEvent setGoldUIEvent;

    void Start() {
        setGoldUIEvent.Raise(playerDataReference.Value.gold);
    }

    public void encounterInitiateEventHandler(string encounterId) {
        mapReference.Value.loadEncounterById(encounterId, activeEncounterVariable);
    }
}
