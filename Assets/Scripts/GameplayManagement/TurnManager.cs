using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Turnphases can be found in TurnPhaseEvent.cs
[RequireComponent(typeof(TurnPhaseEventListener))]
public class TurnManager : GenericSingleton<TurnManager>
{
    [SerializeField]
    private TurnPhaseEvent turnPhaseEvent;
    // Start is called before the first frame update
    [SerializeField]
    private EndEncounterEvent endEncounterEvent;

    private Dictionary<TurnPhase, TurnPhase> nextPhase = new Dictionary<TurnPhase, TurnPhase>(){
        {TurnPhase.START_ENCOUNTER, TurnPhase.BEFORE_START_PLAYER_TURN},
        {TurnPhase.BEFORE_START_PLAYER_TURN, TurnPhase.START_PLAYER_TURN},
        {TurnPhase.START_PLAYER_TURN, TurnPhase.PLAYER_TURN},
        {TurnPhase.PLAYER_TURN, TurnPhase.BEFORE_END_PLAYER_TURN},
        {TurnPhase.BEFORE_END_PLAYER_TURN, TurnPhase.END_PLAYER_TURN},
        {TurnPhase.END_PLAYER_TURN, TurnPhase.START_ENEMY_TURN},
        {TurnPhase.START_ENEMY_TURN, TurnPhase.ENEMIES_TURN},
        {TurnPhase.ENEMIES_TURN, TurnPhase.END_ENEMY_TURN},
        {TurnPhase.END_ENEMY_TURN, TurnPhase.BEFORE_START_PLAYER_TURN}
    };

    private Dictionary<TurnPhase, PriorityEventDispatcher<Func<IEnumerator>>> turnPhaseTriggers = new Dictionary<TurnPhase, PriorityEventDispatcher<Func<IEnumerator>>>(){
        {TurnPhase.START_ENCOUNTER, new()},
        {TurnPhase.BEFORE_START_PLAYER_TURN, new()},
        {TurnPhase.START_PLAYER_TURN, new()},
        {TurnPhase.PLAYER_TURN, new()},
        {TurnPhase.BEFORE_END_PLAYER_TURN, new()},
        {TurnPhase.END_PLAYER_TURN, new()},
        {TurnPhase.START_ENEMY_TURN, new()},
        {TurnPhase.ENEMIES_TURN, new()},
        {TurnPhase.END_ENEMY_TURN, new()},
        {TurnPhase.END_ENCOUNTER, new()}
    };

    private List<string> turnPhaseChangeBlockers = new List<string>();

    private TurnPhase currentTurnPhase = TurnPhase.START_ENCOUNTER;

    void Start()
    {
        StartCoroutine("LateStart");
    }

    private IEnumerator LateStart() {
        yield return new WaitUntil(() => EnemyEncounterManager.Instance.EncounterStartReady());
        // place to hook in a wait on a boss fight animation sequence?
        StartCoroutine(turnPhaseEvent.RaiseAtEndOfFrameCoroutine(new TurnPhaseEventInfo(TurnPhase.START_ENCOUNTER)));
        EnemyEncounterViewModel.Instance.SetStateDirty();
    }

    public TurnPhase GetTurnPhase() {
        return currentTurnPhase;
    }

    public void turnPhaseChangedEventHandler(TurnPhaseEventInfo info) {
        currentTurnPhase = info.newPhase;
        // This is a bit of a hack until we revisit all the different triggers
        // and how we want to architect triggering bettter.
        // There's a nonzero chance this is introducing a race condition, I haven't
        // done all the timing math yet.
        if (turnPhaseChangeBlockers.Count > 0) {
            StartCoroutine(changeTurnPhaseContinueCoroutine(info));
            return;
        }
        if(info.newPhase == TurnPhase.PLAYER_TURN) {
            // The only phase where we just want to wait
            // The end turn button will raise BEFORE_END_PLAYER_TURN
            // to keep us moving
            return;
        }
        if(info.newPhase == TurnPhase.END_ENCOUNTER) {
            StartCoroutine(runEndEncounterTriggers());
            return;
        }
        StartCoroutine(nextPhaseAfterTriggers(info.newPhase));
        Debug.Log("EnemyInstance: UpdateView");
        EnemyEncounterViewModel.Instance.SetStateDirty();
    }

    private IEnumerator changeTurnPhaseContinueCoroutine(TurnPhaseEventInfo info) {
        while(turnPhaseChangeBlockers.Count > 0) {
            yield return null;
        }
        turnPhaseChangedEventHandler(info);
    }

    private IEnumerator nextPhaseAfterTriggers(TurnPhase currentPhase) {
        Debug.Log("nextPhaseAfterTriggers found " + turnPhaseTriggers[currentPhase].Count() + " triggers for phase " + currentPhase);
        yield return StartCoroutine(runTriggersForPhase(currentPhase));
        // Wait for effects to resolve before raising the next turn phase event.
        // We need to do this because many of the "runTriggersForPhase" in the game
        // right now are not managed as coroutines.
        // Otherwise, we could wait on them all to complete with only the `yield return StartCoroutine`
        // expression.
        // Note: this is a lil racy, it depends on the EffectManager being marked as running
        // by another coroutine before this coroutine resumes and checks; not foolproof.
        yield return new WaitUntil(() => EffectManager.Instance.IsEffectRunning() == false);

        // Check to see if any turn phase triggers caused the end of the encounter
        // The comabt entity manager will emit the END_ENCOUNTER turn phase trigger
        if (CombatEntityManager.Instance.IsEncounterEnded()) {
            yield break;
        }

        StartCoroutine(turnPhaseEvent.RaiseAtEndOfFrameCoroutine(new TurnPhaseEventInfo(nextPhase[currentPhase])));
        Debug.Log("EnemyInstance: UpdateView");
        EnemyEncounterViewModel.Instance.SetStateDirty();
    }

    private IEnumerator runTriggersForPhase(TurnPhase phase) {
        Debug.Log("TurnPhaseManager: Running triggers for turn phase " + phase);
        // Copy over the list, because the triggers may result in mutations to the `turnPhaseTriggers` list.
        PriorityEventDispatcher<Func<IEnumerator>> clone = turnPhaseTriggers[phase].Clone();
       yield return StartCoroutine(clone.Invoke().GetEnumerator());
    }

    private IEnumerator runEndEncounterTriggers() {
        Debug.Log("TurnPhaseManager: Running triggers for endEncounter");
        yield return StartCoroutine(turnPhaseTriggers[TurnPhase.END_ENCOUNTER].Invoke().GetEnumerator());
        // Wait for effects started by the end encounter triggers to resolve :)
        yield return new WaitUntil(() => EffectManager.Instance.IsEffectRunning() == false);

        // has to be in a coroutine so we can wait for all the triggers before doing this
        // TODO: implement defeat. I think having the companion/enemy manager raise the end encounter turn phase
        // is correct so we can catch the triggers, but we definitely can't pass the outcome along with that
        EncounterOutcome outcome = CombatEntityManager.Instance.getCompanions().Count > 0 ? EncounterOutcome.Victory : EncounterOutcome.Defeat;
        endEncounterEvent.Raise(new EndEncounterEventInfo(outcome));
    }

    public void registerTurnPhaseTriggerEventHandler(TurnPhaseTriggerEventInfo info) {
        Debug.Log("Registering turn phase trigger event handler for phase " + info.turnPhaseTrigger.phase + " method is : " + info.turnPhaseTrigger.triggerResponse);
        addTurnPhaseTrigger(info.turnPhaseTrigger);
    }

    public void removeTurnPhaseTriggerEventHandler(TurnPhaseTriggerEventInfo info) {
        removeTurnPhaseTrigger(info.turnPhaseTrigger);
    }

    public void addTurnPhaseTrigger(TurnPhaseTrigger trigger, int weight = 0) {
        trigger.boundHandler = () => trigger.triggerResponse.GetEnumerator();
        turnPhaseTriggers[trigger.phase].AddHandler(trigger.boundHandler, weight);
    }

    public void removeTurnPhaseTrigger(TurnPhaseTrigger trigger) {
        turnPhaseTriggers[trigger.phase].RemoveHandler(trigger.boundHandler);
    }

    public void addTurnPhaseBlocker(string blocker) {
        turnPhaseChangeBlockers.Add(blocker);
    }

    public void removeTurnPhaseBlocker(string blocker) {
        turnPhaseChangeBlockers.Remove(blocker);
    }
}