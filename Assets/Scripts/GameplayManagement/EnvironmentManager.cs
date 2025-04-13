using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EnvironmentManager : GenericSingleton<EnvironmentManager>
{

    [SerializeField]
    private GameStateVariableSO gameStateVariable;

    void Start()
    {
        if(!gameStateVariable || !gameStateVariable.activeEncounter || gameStateVariable.activeEncounter.GetValue() == null) {
            Debug.Log("EnvironmentManager not in combat or shop");
            return;
        }
        Debug.Log("[EnvironmentManager] current encounter stage: " + gameStateVariable.activeEncounter.GetValue().GetEncounterStage());
        Debug.Log("[EnvironmentManager] current encounter type: " + gameStateVariable.activeEncounter.GetValue().getEncounterType());
    }
}