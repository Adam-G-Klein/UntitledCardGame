using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EncounterVariableBuilder : MonoBehaviour 
{
    [Tooltip("Generate the encounter from the Active Encounter variable")]
    public EncounterVariable activeEncounter;

    // Start is called before the first frame update
    void Start() 
    {
        if (activeEncounter != null && activeEncounter.encounter != null) {
            activeEncounter.encounter.Build();
        }
    }
}
