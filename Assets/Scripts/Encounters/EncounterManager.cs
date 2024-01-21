using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;

[RequireComponent(typeof(EndEncounterEventListener))]
public class EncounterManager : MonoBehaviour, IEncounterBuilder
{
    public GameStateVariableSO gameState;
    public EncounterConstantsSO encounterConstants;

    void Awake() {
        // This ends up calling BuildEnemyEncounter below
        gameState.activeEncounter.GetValue().BuildWithEncounterBuilder(this);
        ManaManager.Instance.SetManaPerTurn(gameState.playerData.GetValue().manaPerTurn);
    }

    public void BuildEnemyEncounter(EnemyEncounter encounter) {
        encounter.Build(gameState.companions.companionList, encounterConstants);
    }

    void Update() {
        if(Input.GetKeyDown(KeyCode.Escape)) {
            Application.Quit();
        }
    }

    public void EndEncounterHandler(EndEncounterEventInfo info) {
        gameState.activeEncounter.GetValue().isCompleted = true;

        // Gold interest calculation
        int baseGoldEarnedPerBattle = 10;
        int increments = 10; // Increments of how much does the player need to have before earning 1 more

        int extraGold = Mathf.FloorToInt(gameState.playerData.GetValue().gold / increments);
        gameState.playerData.GetValue().gold += baseGoldEarnedPerBattle + extraGold;

        gameState.map.GetValue().loadMapScene();
    }

    // This exists to satisfy the IEncounterBuilder interface.
    // The IEncounterBuilder interface exists to avoid type casting at runtime
    public void BuildShopEncounter(ShopEncounter encounter) {
        Debug.LogError("The enemy encounter scene was loaded but the active encounter is a shop!");
        return;
    }
}
