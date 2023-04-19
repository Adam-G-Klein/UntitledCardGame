using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EncounterManager : MonoBehaviour
{
    public EncounterConstantsSO encounterConstants;
    public EncounterVariableSO activeEncounterVariable;
    public PlayerDataVariableSO activePlayerDataVariable;
    public  CompanionListVariableSO activeCompanionsVariable;
    public MapVariableSO activeMapVariable;
    private bool inPlayMode = false;
    public bool refreshCompanionsOnPlay = true;

    void Awake() {
        if (activeEncounterVariable.GetValue().getEncounterType() != EncounterType.Enemy) {
            Debug.LogError("Active encounter is not an enemy but an enemy was loaded!");
            return;
        }
        activeEncounterVariable.GetValue().build(activeCompanionsVariable.companionList, encounterConstants);
    }

    void Update() {
        
    }

    public void endEncounterHandler(EndEncounterEventInfo info) {
        activeEncounterVariable.GetValue().isCompleted = true;
        activePlayerDataVariable.GetValue().gold += 3;
        activeMapVariable.GetValue().loadMapScene();
    }
}
