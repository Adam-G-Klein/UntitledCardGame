using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;
using TMPro;
using UnityEngine.UIElements;
using FMODUnity;
using FMOD.Studio;

[RequireComponent(typeof(EndEncounterEventListener))]
public class EnemyEncounterManager : GenericSingleton<EnemyEncounterManager>, IEncounterBuilder, IControlsReceiver
{
    public GameStateVariableSO gameState;
    public CombatEncounterView combatEncounterView;
    public EncounterConstantsSO encounterConstants;
    public CombatEncounterState combatEncounterState;
    public delegate void OnEncounterEndHandler();
    public event OnEncounterEndHandler onEncounterEndHandler;
    public EnemyPortraitController enemyPortraitController;
    [SerializeField]
    private GameObject optionsUI;
    [SerializeField]
    // There's so many ways we could do this
    // choosing the simplest one for now
    private GameObject postCombatUI;
    [SerializeField]
    private GameObject victoryUI;

    [SerializeField]
    private EndEncounterEvent endEncounterEvent;
    [SerializeField]
    private UIStateEvent uIStateEvent;
    [SerializeField]
    private GameObject postGamePopup;
    [SerializeField]
    public GameObject placerGO;
    private bool encounterBuilt = false;
    private bool inToolTip = false;
    private bool castingCard = false;
    private bool combatOver = false;


    [SerializeField]
    [Header("Super hacky way to delay the end combat screen, this or\n" +
        "TurnPhaseDisplay should own the animation and screen display front to back")]
    private float endCombatScreenDelay = 5.0f;

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


    void Start() {
        encounterBuilt = false;
        // This ends up calling BuildEnemyEncounter below
        combatEncounterView.SetupFromGamestate(this);
        StartCoroutine(StartWhenUIDocReady());
    }

    IEnumerator StartWhenUIDocReady() {
        yield return new WaitUntil(() => placer != null && placer.IsReady());
        Debug.Log("EnemyEncounterManager: UIDocumentGameObjectPlacer is ready, building encounter");
        LateStart();
    }

    void LateStart() {
        ControlsManager.Instance.RegisterControlsReceiver(this);
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
        EnemyEncounterViewModel.Instance.SetListener(combatEncounterView);
        EnemyEncounterViewModel.Instance.SetStateDirty();
        combatEncounterView.ResetEntities(createdCompanions, createdEnemies);
        // set up the EnemyEncounterViewModel, which passes information to the UI
        encounterBuilt = true;
        combatOver = false;
    }

    public bool IsEncounterBuilt(){
        return encounterBuilt;
    }

    void Update() {
        if(Input.GetKeyDown(KeyCode.K)) {
            endEncounterEvent.Raise(new EndEncounterEventInfo(EncounterOutcome.Victory));
        }
    }

    public void EndEncounterHandler(EndEncounterEventInfo info)
    {
        combatOver = true;
        Debug.Log("EndEncounterHandler called, info.outcome is " + info.outcome + " gameState.GetLoopIndex() is " + gameState.GetLoopIndex() + " gameState.lastTutorialLoopIndex is " + gameState.lastTutorialLoopIndex);
        if (info.outcome == EncounterOutcome.Defeat)
        {
            LoseGameHandler();
            return;
        }
        if (gameState.activeEncounter.GetValue().id == gameState.map.GetValue().encounters[gameState.map.GetValue().encounters.Count - 1].id)
        {
            WinGameHandler();
            return;
        }
        gameState.activeEncounter.GetValue().isCompleted = true;
        Debug.Log("EndEncounterHandler called, activeEncounter is " + gameState.activeEncounter.GetValue().id + " isCompleted is " + gameState.activeEncounter.GetValue().isCompleted);

        // Gold interest calculation
        int baseGoldEarnedPerBattle = gameState.baseShopData.goldEarnedPerBattle;
        if (gameState.conferenceMode) baseGoldEarnedPerBattle += 2;
        int extraGold = Mathf.FloorToInt(gameState.baseShopData.interestRate * gameState.playerData.GetValue().gold);
        if (extraGold > gameState.baseShopData.interestCap)
        {
            Debug.Log("capping extra gold " + extraGold.ToString() + " at interest cap " + gameState.baseShopData.interestCap.ToString());
            extraGold = gameState.baseShopData.interestCap;
        }
        gameState.playerData.GetValue().gold += baseGoldEarnedPerBattle + extraGold;

        // Give player shop upgrade increments for defeating the enemy
        gameState.EarnUpgradeIncrement();

        if (onEncounterEndHandler != null)
        {
            foreach (OnEncounterEndHandler handler in onEncounterEndHandler.GetInvocationList())
            {
                handler.Invoke();
            }
        }

        // Revive all companions that died during combat to death's door.
        foreach (Companion companion in gameState.companions.allCompanions)
        {
            if (companion.combatStats.currentHealth <= 0)
            {
                companion.combatStats.setCurrentHealth(1);
            }

        }

        gameState.LoadNextLocation();
        EnemyEncounterViewModel.Instance.SetInMenu(true);
        postCombatUI.SetActive(true);
        uIStateEvent.Raise(new UIStateEventInfo(UIState.END_ENCOUNTER));
        //postCombatUI.transform.SetSiblingIndex(postCombatUI.transform.parent.childCount - 1);
        TurnOffInteractions();
        TurnOffFocusing();// Needs to go before the next line bc this line disables existing focus, while the next line sets up more focus
        postCombatUI.GetComponent<EndEncounterView>().Setup(baseGoldEarnedPerBattle, extraGold, gameState.baseShopData.interestCap, gameState.baseShopData.interestRate);
        StartCoroutine(displayPostCombatUIAfterDelay());

        DialogueManager.Instance.SetDialogueLocation(gameState);
        DialogueManager.Instance.StartAnyDialogueSequence();
        SetInToolTip(false);
        MusicController.Instance.SetCombatState("Victory");
    }

    private void WinGameHandler() {
        ProgressManager.Instance.ReportProgressEvent(GameActionType.WIN_A_RUN, 1);
        ProgressManager.Instance.SetMaxAscensionUnlocked(gameState.ascensionLevel + 1); // when you win at ascension level x, you unlock ascension level x + 1
        SaveManager.Instance.SavePlayerProgress(); // lock in values now in case of crash/player quits before progression screen
        EntityVictoryStatsManager.Instance.ReportWin(gameState.companions.activeCompanions, gameState.ascensionLevel);
        SaveManager.Instance.DeleteSaveData();
        gameState.LoadNextLocation();
        victoryUI.SetActive(true);
        MusicController.Instance.SetCombatState("Victory");
        uIStateEvent.Raise(new UIStateEventInfo(UIState.END_ENCOUNTER));
        victoryUI.transform.SetSiblingIndex(postCombatUI.transform.parent.childCount - 1);
        victoryUI.GetComponent<VictoryView>().Setup(gameState.companions.activeCompanions);
        TurnOffInteractions();
        StartCoroutine(displayVictoryUIAfterDelay());
        SetInToolTip(false);
    }

    private void LoseGameHandler() {
        SaveManager.Instance.DeleteSaveData();
        postGamePopup.SetActive(true);
        MusicController.Instance.SetCombatState("Defeat");
        postGamePopup.GetComponent<DefeatView>().Setup(((EnemyEncounter)gameState.activeEncounter.GetValue()).enemyList);
        TurnOffInteractions();
        StartCoroutine(displayDefeatUIAfterDelay());
        SetInToolTip(false);
    }

    public void TurnOffInteractions() {
        FocusManager.Instance.Unfocus();
        PlayerHand.Instance.DisableHand();
        combatEncounterView.UpdateView();
    }

    public void TurnOffFocusing() {
        FocusManager.Instance.UnregisterAll();
    }

    private IEnumerator displayPostCombatUIAfterDelay()
    {
        yield return new WaitForSeconds(endCombatScreenDelay);
        postCombatUI.GetComponent<EndEncounterView>().Show();
        MusicController.Instance.PlaySFX("event:/SFX/SFX_EarnMoney");
    }
    private IEnumerator displayVictoryUIAfterDelay() {
        yield return new WaitForSeconds(endCombatScreenDelay);
        victoryUI.GetComponent<VictoryView>().Show();
    }
    private IEnumerator displayDefeatUIAfterDelay() {
        yield return new WaitForSeconds(endCombatScreenDelay);
        postGamePopup.GetComponent<DefeatView>().Show();
    }

    // This exists to satisfy the IEncounterBuilder interface.
    // The IEncounterBuilder interface exists to avoid type casting at runtime
    public void BuildShopEncounter(ShopEncounter encounter) {
        Debug.LogError("The enemy encounter scene was loaded but the active encounter is a shop!");
        return;
    }

    private void RegisterCombatEncounterStateActions()
    {
        TurnPhaseTrigger trigger = new TurnPhaseTrigger(
            TurnPhase.END_PLAYER_TURN,
            UpdateCombatEncounterState());
        TurnManager.Instance.addTurnPhaseTrigger(trigger);

        TurnPhaseTrigger startTurnTrigger = new TurnPhaseTrigger(
            TurnPhase.START_PLAYER_TURN,
            UpdateTurnCounterDisplay());
        TurnManager.Instance.addTurnPhaseTrigger(startTurnTrigger);

        TurnPhaseTrigger setTurnCounterTrigger = new TurnPhaseTrigger(
            TurnPhase.START_ENCOUNTER,
            ResetTurnCounter());
        TurnManager.Instance.addTurnPhaseTrigger(setTurnCounterTrigger);

        if (ProgressManager.Instance.IsFeatureEnabled(AscensionType.ENEMIES_DEADLIER))
        {
            TurnPhaseTrigger startcombatTrigger = new TurnPhaseTrigger(
                TurnPhase.START_ENCOUNTER,
                SetupDeadlyEnemies()
            );
            TurnManager.Instance.addTurnPhaseTrigger(startcombatTrigger);
        }
    }

    private IEnumerable SetupDeadlyEnemies() {
        foreach (EnemyInstance enemy in EnemyEncounterViewModel.Instance.enemies)
        {
            int additionalStrength = enemy.enemy.enemyType.DeadlierEnemyBonusStr;
            if (additionalStrength == -1) additionalStrength = (int)ProgressManager.Instance.GetAscensionSO(AscensionType.ENEMIES_DEADLIER).
                ascensionModificationValues.GetValueOrDefault("extraStrength", 1f);
            enemy.GetCombatInstance().ApplyStatusEffects(
                StatusEffectType.Strength,
                additionalStrength
            );
        }
        yield return null;
    }

    private IEnumerable ResetTurnCounter() {
        combatEncounterState.turn = 1;
        yield return null;
    }
    private IEnumerable UpdateTurnCounterDisplay()
    {
        combatEncounterView.mapView.UpdateTurnCounter(combatEncounterState.turn);
        yield return null;
    }

    private IEnumerable UpdateCombatEncounterState()
    {
        combatEncounterState.UpdateStateOnEndTurn();
        yield return null;
    }

    public void DeckShuffled(DeckInstance instance) {
        combatEncounterState.DeckShuffled(instance);
    }

    public bool GetInToolTip() {
        return inToolTip;
    }

    public void SetInToolTip(bool b) {
        inToolTip = b;
    }


    public void SetCastingCard(bool isCasting)
    {
        castingCard = isCasting;
    }

    public bool GetCastingCard() {return castingCard;}

    internal bool GetCombatOver()
    {
        return combatOver;
    }

    public void ToggleUIDocuments(bool inMenu) {
        EnemyEncounterViewModel.Instance.SetInMenu(inMenu);
    }

    public void ProcessGFGInputAction(GFGInputAction action)
    {
        if (action == GFGInputAction.END_TURN && HotkeyManager.Instance.endTurnHotkeyEnabled) {
            HotkeyManager.Instance.EndTurn();
        }
    }

    public void SwappedControlMethod(ControlsManager.ControlMethod controlMethod)
    {
        // just needed to implement the interface
        return;
    }
}