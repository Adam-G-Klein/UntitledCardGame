using System.Collections;
using System.Collections.Generic;
using System;
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

    public void EndEncounterHandler(EndEncounterEventInfo info) {
        activeEncounterVariable.GetValue().isCompleted = true;

        // Gold interest calculation
        int baseGoldEarnedPerBattle = 10;
        int increments = 10; // Increments of how much does the player need to have before earning 1 more

        int extraGold = Mathf.FloorToInt(activePlayerDataVariable.GetValue().gold / increments);
        activePlayerDataVariable.GetValue().gold += baseGoldEarnedPerBattle + extraGold;


        activeMapVariable.GetValue().loadMapScene();
    }
}
