using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;
using TMPro;

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

    public CompanionInstanceController companionInstanceController;

    public EnemyInstanceController enemyInstanceController;

    [SerializeField]
    // There's so many ways we could do this
    // choosing the simplest one for now
    private GameObject postCombatUI;

    [SerializeField]
    private GameObject companionLocationStoreGO;
    [SerializeField]
    private GameObject enemyLocationStoreGO;

    [SerializeField]
    private UIStateEvent uIStateEvent;
    [SerializeField]
    private GameObject postGamePopup;
    [SerializeField]
    private TurnPhaseEvent turnPhaseEvent;

    public LocationStore companionLocationStore {
        get {
        if(companionLocationStoreGO == null) {
            Debug.LogError("companionLocationStoreGO is null");
            return null;
        } else return companionLocationStoreGO.GetComponent<LocationStore>();
        } set {
            companionLocationStore = value;
        }
    }
    public LocationStore enemyLocationStore {
        get {
        if(enemyLocationStoreGO == null) {
            Debug.LogError("enemyLocationStoreGO is null");
            return null;
        } else return enemyLocationStoreGO.GetComponent<LocationStore>();
        } set {
            enemyLocationStore = value;
        }
    }

    void Start() {
        // This ends up calling BuildEnemyEncounter below
        gameState.activeEncounter.GetValue().BuildWithEncounterBuilder(this);
        ManaManager.Instance.SetManaPerTurn(gameState.playerData.GetValue().manaPerTurn);
        RegisterCombatEncounterStateActions();
    }

    public void BuildEnemyEncounter(EnemyEncounter encounter,
        LocationStore companionLocationStore,
        LocationStore enemyLocationStore) {
        List<CompanionInstance> createdCompanions = companionInstanceController.SetupCompanions(
            gameState.companions.activeCompanions,
            encounterConstants
        );
        List<EnemyInstance> createdEnemies = enemyInstanceController.SetupEnemies(
            encounter.enemyList,
            encounterConstants
        );
        characterPortraitController.SetupCharacterPortraits(createdCompanions);
        enemyPortraitController.SetupEnemyPortraits(createdEnemies);
    }

    void Update() {

        if(Input.GetKeyDown(KeyCode.S)) {
            turnPhaseEvent.Raise(new TurnPhaseEventInfo(TurnPhase.END_ENCOUNTER));
        }
        if(Input.GetKeyDown(KeyCode.Escape)) {
            Application.Quit();
        }
    }

    public void EndEncounterHandler(EndEncounterEventInfo info) {
        Debug.Log("EndEncounterHandler called, info.outcome is " + info.outcome + " gameState.GetLoopIndex() is " + gameState.GetLoopIndex() + " gameState.lastTutorialLoopIndex is " + gameState.lastTutorialLoopIndex);
        if(info.outcome == EncounterOutcome.Defeat) {
            postGamePopup.SetActive(true);
            return;
        }
        gameState.activeEncounter.GetValue().isCompleted = true;
        Debug.Log("EndEncounterHandler called, activeEncounter is " + gameState.activeEncounter.GetValue().id + " isCompleted is " + gameState.activeEncounter.GetValue().isCompleted);

        // Gold interest calculation
        int baseGoldEarnedPerBattle = gameState.baseShopData.goldEarnedPerBattle;
        int extraGold = Mathf.FloorToInt(gameState.baseShopData.interestRate * gameState.playerData.GetValue().gold);
        if (extraGold > gameState.baseShopData.interestCap) {
            Debug.Log("capping extra gold " + extraGold.ToString() + " at interest cap " + gameState.baseShopData.interestCap.ToString());
            extraGold = gameState.baseShopData.interestCap;
        }
        gameState.playerData.GetValue().gold += baseGoldEarnedPerBattle + extraGold;

        if (onEncounterEndHandler != null) {
            foreach (OnEncounterEndHandler handler in onEncounterEndHandler.GetInvocationList()) {
                handler.Invoke();
            }
        }

        // Revive all companions that died during combat to death's door.
        foreach (Companion companion in gameState.companions.allCompanions) {
            if (companion.combatStats.currentHealth <= 0) {
                companion.combatStats.setCurrentHealth(1);
            }
        }

        gameState.LoadNextLocation();
        postCombatUI.SetActive(true);
        uIStateEvent.Raise(new UIStateEventInfo(UIState.END_ENCOUNTER));
        postCombatUI.transform.SetSiblingIndex(postCombatUI.transform.parent.childCount - 1);
        TMP_Text[] texts = postCombatUI.GetComponentsInChildren<TMP_Text>();
        TMP_Text rewardText = texts[1];
        rewardText.text = "Base gold earned\n$" +
            baseGoldEarnedPerBattle.ToString() +
            "\ninterest (" +
            gameState.baseShopData.interestRate.ToString("P0") +
            ", capped at $" +
            gameState.baseShopData.interestCap.ToString() +
            ")\n$" +
            extraGold.ToString();
        /* re-enable for the companions to talk at the end of the encounter
        DialogueManager.Instance.SetDialogueLocation(gameState);
        DialogueManager.Instance.StartAnyDialogueSequence();
        */
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
