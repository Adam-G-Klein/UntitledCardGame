using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;
using TMPro;
using FMODUnity;
using FMOD.Studio;
using UnityEngine.UI;

[RequireComponent(typeof(EndEncounterEventListener))]
public class EnemyEncounterManager : GenericSingleton<EnemyEncounterManager>, IEncounterBuilder, IControlsReceiver
{
    public GameStateVariableSO gameState;
    public CombatEncounterView combatEncounterView;
    public EncounterConstantsSO encounterConstants;
    public CombatEncounterState combatEncounterState;
    public delegate void OnEncounterEndHandler();
    public event OnEncounterEndHandler onEncounterEndHandler;
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
    private TurnPhaseEvent turnPhaseEvent;
    [SerializeField]
    private GameObject postGamePopup;
    [SerializeField]
    public GameObject placerGO;
    [SerializeField]
    private GameObject combatEnvironmentParent;
    private bool startTheTurns = false;
    private bool cinematicIntroComplete = false;
    private bool cinematicOutroComplete = false;
    private bool inToolTip = false;
    private bool castingCard = false;
    private bool combatOver = false;
    private bool endTurnEnabled = true;
    private bool inDeckView = false;
    private bool inOptionsView = false;
    private bool isEliteCombat = false;
    public bool isBoss = false;
    private string encounterName;


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
        startTheTurns = false;
        // This ends up calling BuildEnemyEncounter below
        combatEncounterView.SetupFromGamestate(this);
        OptionsViewController.Instance.SetEnterHandler(OnMenuOpenedHandler);
        OptionsViewController.Instance.SetExitHandler(OnMenuClosedHandler);
        MultiDeckViewManager.Instance.SetEnterHandler(OnDeckViewOpenedHandler);
        MultiDeckViewManager.Instance.SetExitHandler(OnDeckViewClosedHandler);
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
        PlayerHand.Instance.SetupCompanionOrder();
        RegisterCombatEncounterStateActions();
    }

    public void BuildEnemyEncounter(EnemyEncounter encounter,
            UIDocumentGameObjectPlacer placer)
    {
        List<CompanionInstance> createdCompanions = new List<CompanionInstance>();
        List<EnemyInstance> createdEnemies = new List<EnemyInstance>();
        encounter.Build(gameState.companions.activeCompanions,
            encounterConstants,
            createdCompanions,
            createdEnemies,
            placer
            );
        // set up the EnemyEncounterViewModel, which passes information to the UI
        EnemyEncounterViewModel.Instance.companions = createdCompanions;
        EnemyEncounterViewModel.Instance.enemies = createdEnemies;
        EnemyEncounterViewModel.Instance.SetListener(combatEncounterView);
        EnemyEncounterViewModel.Instance.SetStateDirty();
        combatEncounterView.ResetEntities(createdCompanions, createdEnemies);
        combatOver = false;
        isEliteCombat = encounter.isEliteEncounter;
        encounterName = encounter.encounterName;

        switch (encounter.act) {
            case Act.One:
                SetupCombatEnvironment(encounterConstants.actOneEnvironment);
            break;

            case Act.Two:
                SetupCombatEnvironment(encounterConstants.actTwoEnvironment);
            break;

            case Act.Three:
                SetupCombatEnvironment(encounterConstants.actThreeEnvironment);
            break;

            default:
                // The scene has the act one environment setup by default
            break;
        }

        // Fire off a combatEnded Analytics event.
        var combatStartedEvent = new CombatStartedAnalyticsEvent
        {
            EncounterName = encounterName,
        };
        AnalyticsManager.Instance.RecordEvent(combatStartedEvent);

        // a coroutine for all of the things (like dialogue and a boss start animation) to yield on before we tell the turn manager
        // sets the startTheTurns flag to start the turns (display player turn splash, deal cards)
        StartCoroutine(PreEncounterCoroutine());
    }

    public bool EncounterStartReady()
    {
        // Set in PreEncounterCoroutine
        return startTheTurns;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            if(!cinematicIntroComplete)
            {
                cinematicIntroComplete = true;
            }
            else
            {
                endEncounterEvent.Raise(new EndEncounterEventInfo(EncounterOutcome.Victory));
            }
        }
    }

    public void CinematicIntroComplete()
    {
        cinematicIntroComplete = true;
    }

    public void CinematicOutroComplete()
    {
        cinematicOutroComplete = true;
    }

    private IEnumerator PreEncounterCoroutine()
    {
        Debug.Log("Starting PreEncounterCoroutine, waiting for cinematic intro to complete");
        if(isBoss)
        {
            yield return new WaitUntil(() => cinematicIntroComplete == true);
        }
        // todo, yield return on additional dialogue being complete from another manager?
        startTheTurns = true;
        yield return null;
    }

    public void EndEncounterHandler(EndEncounterEventInfo info)
    {
        StartCoroutine(EndEncounterCoroutine(info));
    }

    /*
        Needed to move the end encounter code from a plain function to a coroutine because
        of the VFX timing for reviving companions at the end of the encounter
    */
    private IEnumerator EndEncounterCoroutine(EndEncounterEventInfo info) {
        TurnOffInteractions();
        combatOver = true;
        combatEncounterView.SetEndCombat(true);

        // Fire off a combatEnded Analytics event.
        var combatEndedEvent = new CombatEndedAnalyticsEvent
        {
            EncounterName = encounterName,
            TurnIndex = combatEncounterState.turn
        };
        AnalyticsManager.Instance.RecordEvent(combatEndedEvent);

        Debug.Log("EndEncounterHandler called, info.outcome is " + info.outcome + " gameState.GetLoopIndex() is " + gameState.GetLoopIndex() + " gameState.lastTutorialLoopIndex is " + gameState.lastTutorialLoopIndex);
        if (info.outcome == EncounterOutcome.Defeat)
        {
            LoseGameHandler();
            yield break;
        }
        if (gameState.activeEncounter.GetValue().id == gameState.map.GetValue().encounters[gameState.map.GetValue().encounters.Count - 1].id)
        {
            if (isBoss) {
                // Kick off the cinematic outro timeline here
                Debug.Log("EnemyEncounterManager was supposed to kickoff the outra cinematic here");
                yield return new WaitUntil(() => cinematicOutroComplete == true);
            }
            WinGameHandler();
            yield break;
        }
        gameState.activeEncounter.GetValue().isCompleted = true;
        Debug.Log("EndEncounterHandler called, activeEncounter is " + gameState.activeEncounter.GetValue().id + " isCompleted is " + gameState.activeEncounter.GetValue().isCompleted);

        // Gold interest calculation
        int baseGoldEarnedPerBattle = gameState.baseShopData.goldEarnedPerBattle;
        int extraGold = Mathf.FloorToInt(gameState.baseShopData.interestRate * gameState.playerData.GetValue().gold);
        if (extraGold > gameState.baseShopData.interestCap)
        {
            Debug.Log("capping extra gold " + extraGold.ToString() + " at interest cap " + gameState.baseShopData.interestCap.ToString());
            extraGold = gameState.baseShopData.interestCap;
        }

        MusicController.Instance.SetCombatState("Victory");

        // Give player shop upgrade increments for defeating the enemy
        gameState.EarnUpgradeIncrement();

        if (onEncounterEndHandler != null)
        {
            foreach (OnEncounterEndHandler handler in onEncounterEndHandler.GetInvocationList())
            {
                handler.Invoke();
            }
        }

        List<Companion> revivedCompanions = new List<Companion>();
        // Revive all companions that died during combat to death's door.
        foreach (Companion companion in gameState.companions.allCompanions)
        {
            if (companion.combatStats.currentHealth <= 0)
            {
                companion.combatStats.setCurrentHealth(1);
                revivedCompanions.Add(companion);
            }
        }

        yield return StartCoroutine(CombatEntityManager.Instance.ReviveCompanions(revivedCompanions, encounterConstants.companionRevivePrefab, 0.5f));


        // Heal all surviving companions by a certain amount for elite combats.
        if (isEliteCombat)
        {
            foreach (Companion companion in gameState.companions.activeCompanions)
            {
                if (companion.combatStats.currentHealth > 0)
                {
                    companion.combatStats.Heal(gameState.baseShopData.postEliteHealingAmount);
                }
            }
            yield return StartCoroutine(
                CombatEntityManager.Instance.HealAliveCompanions(encounterConstants.companionRevivePrefab, 0.2f)
            );
        }

        gameState.LoadNextLocation();

        EnemyEncounterViewModel.Instance.SetInMenu(true);
        postCombatUI.SetActive(true);
        uIStateEvent.Raise(new UIStateEventInfo(UIState.END_ENCOUNTER));
        //postCombatUI.transform.SetSiblingIndex(postCombatUI.transform.parent.childCount - 1);

        TurnOffFocusing();// Needs to go before the next line bc this line disables existing focus, while the next line sets up more focus
        postCombatUI.GetComponent<EndEncounterView>().Setup(baseGoldEarnedPerBattle, extraGold, gameState.baseShopData.interestCap, gameState.baseShopData.interestRate);
        StartCoroutine(displayPostCombatUIAfterDelay());

        DialogueManager.Instance.SetDialogueLocation(gameState);
        DialogueManager.Instance.StartAnyDialogueSequence();
        SetInToolTip(false);
        yield return null;
    }

    private void WinGameHandler() {
        var winGameEvent = new RunEndedAnalyticsEvent
        {
            EndCondition = RunEndedCondition.VICTORY,
        };
        AnalyticsManager.Instance.RecordEvent(winGameEvent);

        ProgressManager.Instance.ReportProgressEvent(GameActionType.WIN_A_RUN, 1);
        ProgressManager.Instance.SetMaxAscensionUnlocked(gameState.ascensionLevel + 1); // when you win at ascension level x, you unlock ascension level x + 1
        SaveManager.Instance.SavePlayerProgress(); // lock in values now in case of crash/player quits before progression screen
        EntityVictoryStatsManager.Instance.ReportWin(gameState.companions.activeCompanions, gameState.ascensionLevel);
        SaveManager.Instance.DeleteSaveData();
        gameState.LoadNextLocation();
        victoryUI.SetActive(true);
        MusicController.Instance.PrepareForGoingBackToMainMenu();
        MusicController.Instance.PlaySFX("event:/MX/MX_CompletedRun");
        uIStateEvent.Raise(new UIStateEventInfo(UIState.END_ENCOUNTER));
        victoryUI.transform.SetSiblingIndex(postCombatUI.transform.parent.childCount - 1);
        victoryUI.GetComponent<VictoryView>().Setup(gameState.companions.activeCompanions);
        TurnOffInteractions();
        StartCoroutine(displayVictoryUIAfterDelay());
        SetInToolTip(false);
    }

    private void LoseGameHandler() {
        var loseGameEvent = new RunEndedAnalyticsEvent
        {
            EndCondition = RunEndedCondition.DEFEAT,
        };
        AnalyticsManager.Instance.RecordEvent(loseGameEvent);

        SaveManager.Instance.DeleteSaveData();
        postGamePopup.SetActive(true);
        MusicController.Instance.SetCombatState("Defeat");
        TurnOffInteractions();
        postGamePopup.GetComponent<DefeatView>().Setup(((EnemyEncounter)gameState.activeEncounter.GetValue()).enemyList);
        StartCoroutine(displayDefeatUIAfterDelay());
        SetInToolTip(false);
    }

    public void TurnOffInteractions() {
        MultiDeckViewManager.Instance.TurnOffInteractions();
        FocusManager.Instance.UnregisterAll();
        PlayerHand.Instance.DisableHand();
        combatEncounterView.DisableInteractions();
        // combatEncounterView.UpdateView();
    }

    public void TurnOffFocusing() {
        FocusManager.Instance.UnregisterAll();
    }

    private IEnumerator displayPostCombatUIAfterDelay()
    {
        yield return new WaitForSeconds(endCombatScreenDelay);
        postCombatUI.GetComponent<EndEncounterView>().Show();
        //MusicController.Instance.PlaySFX("event:/SFX/SFX_EarnMoney");
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

    private void SetupCombatEnvironment(GameObject prefab) {
        foreach (Transform child in combatEnvironmentParent.transform)
        {
            Destroy(child.gameObject);
        }

        Instantiate(prefab, Vector3.zero, Quaternion.identity, combatEnvironmentParent.transform);
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
            CombatInstance ci = enemy.GetCombatInstance();
            int additionalStrength = enemy.enemy.enemyType.DeadlierEnemyBonusStr;
            if (additionalStrength == -1) additionalStrength = (int)ProgressManager.Instance.GetAscensionSO(AscensionType.ENEMIES_DEADLIER).
                ascensionModificationValues.GetValueOrDefault("extraStrength", 1f);
            ci.ApplyStatusEffects(
                StatusEffectType.Strength,
                additionalStrength
            );
            int additionalHealth = enemy.enemy.enemyType.HealthierEnemyBonusHealth;
            if (additionalHealth == -1) additionalHealth = (int)ProgressManager.Instance.GetAscensionSO(AscensionType.ENEMIES_DEADLIER).
                ascensionModificationValues.GetValueOrDefault("extraHealth", 0f);
            ci.combatStats.IncreaseMaxHealth(additionalHealth);
            ci.combatStats.setCurrentHealth(ci.combatStats.getCurrentHealth() + additionalHealth);
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


    public void SetCastingCard(bool isCasting) {
        castingCard = isCasting;
    }

    public bool GetCastingCard() {
        return castingCard;
    }

    internal bool GetCombatOver()
    {
        return combatOver;
    }

    private void OnMenuOpenedHandler()
    {
        Debug.Log("OnMenuOpenedHandler");
        inOptionsView = true;
        endTurnEnabled = false;
    }

    private void OnMenuClosedHandler()
    {
        Debug.Log("OnMenuClosedHandler");
        inOptionsView = false;
        endTurnEnabled = !inDeckView;
    }

    private void OnDeckViewOpenedHandler()
    {
        Debug.Log("OnDeckViewOpenedHandler");
        inDeckView = true;
        endTurnEnabled = false;
    }

    private void OnDeckViewClosedHandler()
    {
        Debug.Log("OnDeckViewClosedHandler");
        inDeckView = false;
        endTurnEnabled = !inOptionsView;
    }

    public void TryEndPlayerTurn()
    {
        if (endTurnEnabled && TurnManager.Instance.GetTurnPhase() == TurnPhase.PLAYER_TURN)
            StartCoroutine(turnPhaseEvent.RaiseAtEndOfFrameCoroutine(new TurnPhaseEventInfo(TurnPhase.BEFORE_END_PLAYER_TURN)));
    }

    public void CanEndTurn(bool val) {
        endTurnEnabled = val;
    }

    public void ToggleUIDocuments(bool inMenu) {
        EnemyEncounterViewModel.Instance.SetInMenu(inMenu);
    }

    public void DamageIndicator(CombatInstance instance, int damage) {
        combatEncounterView.DamageIndicator(instance, damage);
    }

    public void ProcessGFGInputAction(GFGInputAction action)
    {
        if (action == GFGInputAction.END_TURN) {
            TryEndPlayerTurn();
        }
        if (action == GFGInputAction.OPEN_MULTI_DECK_VIEW)
        {
            if (combatOver) return;
            MultiDeckViewManager.Instance.ShowCombatDeckView(gameState.hoveredCompanion);
        }
    }

    public void SwappedControlMethod(ControlsManager.ControlMethod controlMethod)
    {
        // just needed to implement the interface
        return;
    }
}