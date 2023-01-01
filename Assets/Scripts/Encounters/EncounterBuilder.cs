using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EncounterBuilder : MonoBehaviour
{
    public EncounterConstants encounterConstants;
    public EncounterVariableSO encounterVariable;

    void Awake() {
        encounterVariable.Value.build(encounterConstants);
    }
}
