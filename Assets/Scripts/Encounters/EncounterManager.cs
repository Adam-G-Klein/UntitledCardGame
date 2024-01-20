using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;

[RequireComponent(typeof(EndEncounterEventListener))]
public class EncounterManager : MonoBehaviour, IEncounterBuilder
{
    public EncounterConstantsSO encounterConstants;
    public EncounterVariableSO activeEncounterVariable;
    public PlayerDataVariableSO activePlayerDataVariable;
    public  CompanionListVariableSO activeCompanionsVariable;
    public MapVariableSO activeMapVariable;
    private bool inPlayMode = false;
    public bool refreshCombatEntitiesOnPlay = true;

    void Awake() {
        // This ends up calling BuildEnemyEncounter below
        activeEncounterVariable.GetValue().BuildWithEncounterBuilder(this);
        ManaManager.Instance.SetManaPerTurn(activePlayerDataVariable.GetValue().manaPerTurn);
    }

    public void BuildEnemyEncounter(EnemyEncounter encounter) {
        encounter.Build(activeCompanionsVariable.companionList, encounterConstants);
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

    // This exists to satisfy the IEncounterBuilder interface.
    // The IEncounterBuilder interface exists to avoid type casting at runtime
    public void BuildShopEncounter(ShopEncounter encounter) {
        Debug.LogError("The enemy encounter scene was loaded but the active encounter is a shop!");
        return;
    }
}
