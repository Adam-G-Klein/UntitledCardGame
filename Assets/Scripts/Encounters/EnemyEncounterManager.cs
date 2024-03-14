using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;

[RequireComponent(typeof(EndEncounterEventListener))]
public class EnemyEncounterManager : GenericSingleton<EnemyEncounterManager>, IEncounterBuilder
{
    public GameStateVariableSO gameState;
    public EncounterConstantsSO encounterConstants;
    public CombatEncounterState combatEncounterState;
    public delegate void OnEncounterEndHandler();
    public event OnEncounterEndHandler onEncounterEndHandler;
    public CharacterPortraitController characterPortraitController;
    public EnemyPortraitController enemyPortraitController;
    [SerializeField]
    // There's so many ways we could do this
    // choosing the simplest one for now
    private GameObject postCombatUI;

    void Start() {
        // This ends up calling BuildEnemyEncounter below
        gameState.activeEncounter.GetValue().BuildWithEncounterBuilder(this);
        ManaManager.Instance.SetManaPerTurn(gameState.playerData.GetValue().manaPerTurn);
        RegisterCombatEncounterStateActions();
    }

    public void BuildEnemyEncounter(EnemyEncounter encounter) {
        List<CompanionInstance> createdCompanions = new List<CompanionInstance>();
        List<EnemyInstance> createdEnemies = new List<EnemyInstance>();
        encounter.Build(gameState.companions.activeCompanions, encounterConstants, createdCompanions, createdEnemies);
        characterPortraitController.SetupCharacterPortraits(createdCompanions);
        enemyPortraitController.SetupEnemyPortraits(createdEnemies);
    }

    void Update() {
        if(Input.GetKeyDown(KeyCode.Escape)) {
            Application.Quit();
        }
    }

    public void EndEncounterHandler(EndEncounterEventInfo info) {
        gameState.activeEncounter.GetValue().isCompleted = true;
        Debug.Log("EndEncounterHandler called, activeEncounter is " + gameState.activeEncounter.GetValue().id + " isCompleted is " + gameState.activeEncounter.GetValue().isCompleted);

        // Gold interest calculation
        int baseGoldEarnedPerBattle = 10;
        int increments = 10; // Increments of how much does the player need to have before earning 1 more

        int extraGold = Mathf.FloorToInt(gameState.playerData.GetValue().gold / increments);
        gameState.playerData.GetValue().gold += baseGoldEarnedPerBattle + extraGold;

        if (onEncounterEndHandler != null) {
            foreach (OnEncounterEndHandler handler in onEncounterEndHandler.GetInvocationList()) {
                handler.Invoke();
            }
        }

        gameState.LoadNextLocation();
        postCombatUI.SetActive(true);
        DialogueManager.Instance.SetDialogueLocation(gameState);
        DialogueManager.Instance.StartAnyDialogueSequence();
    }

    // This exists to satisfy the IEncounterBuilder interface.
    // The IEncounterBuilder interface exists to avoid type casting at runtime
    public void BuildShopEncounter(ShopEncounter encounter) {
        Debug.LogError("The enemy encounter scene was loaded but the active encounter is a shop!");
        return;
    }

    private void RegisterCombatEncounterStateActions() {
        TurnPhaseTrigger trigger = new TurnPhaseTrigger(
            TurnPhase.END_PLAYER_TURN,
            UpdateCombatEncounterState());
        TurnManager.Instance.addTurnPhaseTrigger(trigger);
    }

    private IEnumerable UpdateCombatEncounterState() {
        combatEncounterState.UpdateStateOnEndTurn();
        yield return null;
    }

    public void DeckShuffled(DeckInstance instance) {
        combatEncounterState.DeckShuffled(instance);
    }
}
