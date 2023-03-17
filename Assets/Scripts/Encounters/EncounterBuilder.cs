using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EncounterBuilder : MonoBehaviour
{
    public EncounterConstants encounterConstants;
    public EncounterVariableSO activeEncounter;
    public CompanionListVariableSO activeCompanions;
    public MapReference map;

    void Awake() {
        if (activeEncounter.Value.getEncounterType() != EncounterType.Enemy) {
            Debug.LogError("Active encounter is not an enemy but an enemy was loaded!");
            return;
        }
        activeEncounter.Value.build(activeCompanions.companionList, encounterConstants);
    }

    public void endEncounterHandler(EndEncounterEventInfo info) {
        activeEncounter.Value.isCompleted = true;
        map.Value.loadMapScene();
    }
}
