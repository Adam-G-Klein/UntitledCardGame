using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EncounterBuilder : MonoBehaviour
{
    public EncounterConstants encounterConstants;
    public EncounterVariableSO encounterVariable;

    void Awake() {
        if (encounterVariable.Value.getEncounterType() != EncounterType.Enemy) {
            Debug.LogError("Active encounter is not an enemy but an enemy was loaded!");
            return;
        }
        encounterVariable.Value.build(encounterConstants);
    }
}
