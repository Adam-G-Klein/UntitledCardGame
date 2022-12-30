using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EncounterBuilder : MonoBehaviour
{
    public EncounterVariableSO encounterVariable;

    void Awake() {
        encounterVariable.encounter.build();
    }
}
