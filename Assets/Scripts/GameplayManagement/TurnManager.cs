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
        {TurnPhase.START_ENCOUNTER, TurnPhase.START_PLAYER_TURN},
        {TurnPhase.START_PLAYER_TURN, TurnPhase.PLAYER_TURN},
        {TurnPhase.PLAYER_TURN, TurnPhase.BEFORE_END_PLAYER_TURN},
        {TurnPhase.BEFORE_END_PLAYER_TURN, TurnPhase.END_PLAYER_TURN},
        {TurnPhase.END_PLAYER_TURN, TurnPhase.START_ENEMY_TURN},
        {TurnPhase.START_ENEMY_TURN, TurnPhase.ENEMIES_TURN},
        {TurnPhase.ENEMIES_TURN, TurnPhase.END_ENEMY_TURN},
        {TurnPhase.END_ENEMY_TURN, TurnPhase.START_PLAYER_TURN}
    };

    private Dictionary<TurnPhase, List<TurnPhaseTrigger>> turnPhaseTriggers = new Dictionary<TurnPhase, List<TurnPhaseTrigger>>(){
        {TurnPhase.START_ENCOUNTER, new List<TurnPhaseTrigger>()},
        {TurnPhase.START_PLAYER_TURN, new List<TurnPhaseTrigger>()},
        {TurnPhase.PLAYER_TURN, new List<TurnPhaseTrigger>()},
        {TurnPhase.BEFORE_END_PLAYER_TURN, new List<TurnPhaseTrigger>()},
        {TurnPhase.END_PLAYER_TURN, new List<TurnPhaseTrigger>()},
        {TurnPhase.START_ENEMY_TURN, new List<TurnPhaseTrigger>()},
        {TurnPhase.ENEMIES_TURN, new List<TurnPhaseTrigger>()},
        {TurnPhase.END_ENEMY_TURN, new List<TurnPhaseTrigger>()},
        {TurnPhase.END_ENCOUNTER, new List<TurnPhaseTrigger>()}
    };

    private List<string> turnPhaseChangeBlockers = new List<string>();

    void Start()
    {
        StartCoroutine("LateStart");
    }

    private IEnumerator LateStart() {
        yield return new WaitForEndOfFrame();
        StartCoroutine(turnPhaseEvent.RaiseAtEndOfFrameCoroutine(new TurnPhaseEventInfo(TurnPhase.START_ENCOUNTER)));
        EnemyEncounterViewModel.Instance.SetStateDirty();
    }

    public void turnPhaseChangedEventHandler(TurnPhaseEventInfo info) {
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
        Debug.Log("nextPhaseAfterTriggers found " + turnPhaseTriggers[currentPhase].Count + " triggers for phase " + currentPhase);
        yield return StartCoroutine(runTriggersForPhase(currentPhase));
        // Wait for effects to resolve before raising the next turn phase event.
        // We need to do this because many of the "runTriggersForPhase" in the game
        // right now are not managed as coroutines.
        // Otherwise, we could wait on them all to complete with only the `yield return StartCoroutine`
        // expression.
        // Note: this is a lil racy, it depends on the EffectManager being marked as running
        // by another coroutine before this coroutine resumes and checks; not foolproof.
        yield return new WaitUntil(() => EffectManager.Instance.IsEffectRunning() == false);
        StartCoroutine(turnPhaseEvent.RaiseAtEndOfFrameCoroutine(new TurnPhaseEventInfo(nextPhase[currentPhase])));
        Debug.Log("EnemyInstance: UpdateView");
        EnemyEncounterViewModel.Instance.SetStateDirty();
    }

    private IEnumerator runTriggersForPhase(TurnPhase phase) {
        Debug.Log("TurnPhaseManager: Running triggers for turn phase " + phase);
        // Copy over the list, because the triggers may result in mutations to the `turnPhaseTriggers` list.
        List<TurnPhaseTrigger> triggersToRun = new();
        foreach(TurnPhaseTrigger trigger in turnPhaseTriggers[phase]) {
            triggersToRun.Add(trigger);
        }
        foreach(TurnPhaseTrigger trigger in triggersToRun) {
            yield return StartCoroutine(trigger.triggerResponse.GetEnumerator());
        }
    }

    private IEnumerator runEndEncounterTriggers() {
        foreach(TurnPhaseTrigger trigger in turnPhaseTriggers[TurnPhase.END_ENCOUNTER]) {
            yield return StartCoroutine(trigger.triggerResponse.GetEnumerator());
        }
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

    public void addTurnPhaseTrigger(TurnPhaseTrigger trigger) {
        turnPhaseTriggers[trigger.phase].Add(trigger);
    }

    public void removeTurnPhaseTrigger(TurnPhaseTrigger trigger) {
        turnPhaseTriggers[trigger.phase].Remove(trigger);
    }

    public void addTurnPhaseBlocker(string blocker) {
        turnPhaseChangeBlockers.Add(blocker);
    }

    public void removeTurnPhaseBlocker(string blocker) {
        turnPhaseChangeBlockers.Remove(blocker);
    }
}