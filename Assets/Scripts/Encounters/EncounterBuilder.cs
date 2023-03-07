using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EncounterBuilder : MonoBehaviour
{
    public EncounterConstants encounterConstants;
    public EncounterVariableSO encounterVariable;
    public MapReference map;

    void Awake() {
        if (encounterVariable.Value.getEncounterType() != EncounterType.Enemy) {
            Debug.LogError("Active encounter is not an enemy but an enemy was loaded!");
            return;
        }
        encounterVariable.Value.build(encounterConstants);
    }

    public void endEncounterHandler(EndEncounterEventInfo info) {
        encounterVariable.Value.isCompleted = true;
        map.Value.loadMapScene();
    }
}
