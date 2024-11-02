using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;
using TMPro;
using UnityEngine.UIElements;

[RequireComponent(typeof(EndEncounterEventListener))]
public class EnemyEncounterManager : GenericSingleton<EnemyEncounterManager>, IEncounterBuilder
{
    public GameStateVariableSO gameState;
    public EncounterConstantsSO encounterConstants;
    public CombatEncounterState combatEncounterState;
    public delegate void OnEncounterEndHandler();
    public event OnEncounterEndHandler onEncounterEndHandler;
    public EnemyPortraitController enemyPortraitController;
    [SerializeField]
    // There's so many ways we could do this
    // choosing the simplest one for now
    private GameObject postCombatUI;

    [SerializeField]
    private EndEncounterEvent endEncounterEvent;
    [SerializeField]
    private UIStateEvent uIStateEvent;
    [SerializeField]
    private GameObject postGamePopup;
    [SerializeField]
    public GameObject placerGO; 
    private bool encounterBuilt = false;

    public UIDocumentGameObjectPlacer placer { get 
    {
        if(placerGO == null) {
            Debug.LogError("companionLocationStoreGO is null");
            return null;
        } else return placerGO.GetComponent<UIDocumentGameObjectPlacer>();
        } set {
            placer = value;
        }
    }


    void Awake() {
        encounterBuilt = false;
        // This ends up calling BuildEnemyEncounter below
        CombatEncounterView.Instance.SetupFromGamestate();
        StartCoroutine(StartWhenUIDocReady());
    }

    IEnumerator StartWhenUIDocReady() {
        yield return new WaitUntil(() => placer.IsReady());
        Debug.Log("EnemyEncounterManager: UIDocumentGameObjectPlacer is ready, building encounter");
        LateStart();
    }

    void LateStart() {
        gameState.activeEncounter.GetValue().BuildWithEncounterBuilder(this);
        ManaManager.Instance.SetManaPerTurn(gameState.playerData.GetValue().manaPerTurn);
        RegisterCombatEncounterStateActions();
    }

    public void BuildEnemyEncounter(EnemyEncounter encounter,
        UIDocumentGameObjectPlacer placer) {
        List<CompanionInstance> createdCompanions = new List<CompanionInstance>();
        List<EnemyInstance> createdEnemies = new List<EnemyInstance>();
        encounter.Build(gameState.companions.activeCompanions,
            encounterConstants,
            createdCompanions,
            createdEnemies,
            placer
            );
        EnemyEncounterViewModel.Instance.companions = createdCompanions;
        EnemyEncounterViewModel.Instance.enemies = createdEnemies;
        EnemyEncounterViewModel.Instance.SetListener(CombatEncounterView.Instance);
        EnemyEncounterViewModel.Instance.SetStateDirty();
        // set up the EnemyEncounterViewModel, which passes information to the UI
        encounterBuilt = true;
    }

    public bool IsEncounterBuilt(){
        return encounterBuilt;
    }

    void Update() {

        if(Input.GetKeyDown(KeyCode.S)) {
            endEncounterEvent.Raise(new EndEncounterEventInfo(EncounterOutcome.Victory));
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

        gameState.LoadNextLocation();
        postCombatUI.SetActive(true);
        uIStateEvent.Raise(new UIStateEventInfo(UIState.END_ENCOUNTER));
        postCombatUI.transform.SetSiblingIndex(postCombatUI.transform.parent.childCount - 1);
        // TODO: add control for the post combat UI, set reward text like this used to:
        /*TMP_Text[] texts = postCombatUI.GetComponentsInChildren<TMP_Text>();
        TMP_Text rewardText = texts[1];
        rewardText.text = "Base gold earned\n$" +
            baseGoldEarnedPerBattle.ToString() +
            "\ninterest (" +
            gameState.baseShopData.interestRate.ToString("P0") +
            ", capped at $" +
            gameState.baseShopData.interestCap.ToString() +
            ")\n$" +
            extraGold.ToString();
            */
        // but do it on the UIDocumentGameObjectPlacer.Instance.uiDoc instead
        // query for the element in the UIDoc and set the text

        
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
