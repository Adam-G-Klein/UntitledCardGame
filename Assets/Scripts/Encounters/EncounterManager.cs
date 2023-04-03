using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EncounterManager : MonoBehaviour
{
    public EncounterConstantsSO encounterConstants;
    public EncounterVariableSO activeEncounterVariable;
    public PlayerDataVariableSO activePlayerDataVariable;
    public  CompanionListVariableSO activeCompanionsVariable;
    public MapVariableSO activeMapVariable;

    void Awake() {
        if (activeEncounterVariable.GetValue().getEncounterType() != EncounterType.Enemy) {
            Debug.LogError("Active encounter is not an enemy but an enemy was loaded!");
            return;
        }
        activeEncounterVariable.GetValue().build(activeCompanionsVariable.companionList, encounterConstants);
    }

    public void endEncounterHandler(EndEncounterEventInfo info) {
        activeEncounterVariable.GetValue().isCompleted = true;
        activePlayerDataVariable.GetValue().gold += 3;
        activeMapVariable.GetValue().loadMapScene();
    }
}
