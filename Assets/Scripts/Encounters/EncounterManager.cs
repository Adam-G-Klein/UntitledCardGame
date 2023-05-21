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
    public bool refreshCombatEntitiesOnPlay = true;

    void Awake() {
        if (activeEncounterVariable.GetValue().getEncounterType() != EncounterType.Enemy) {
            Debug.LogError("Active encounter is not an enemy but an enemy was loaded!");
            return;
        }
        activeEncounterVariable.GetValue().build(activeCompanionsVariable.companionList, encounterConstants);
    }

    void Update() {
        if(Input.GetKeyDown(KeyCode.Escape)) {
            Application.Quit();
        }
        
    }

    public void endEncounterHandler(EndEncounterEventInfo info) {
        activeEncounterVariable.GetValue().isCompleted = true;
        activePlayerDataVariable.GetValue().gold += 5;
        activeMapVariable.GetValue().loadMapScene();
    }
}
