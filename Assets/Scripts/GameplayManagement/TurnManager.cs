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

    void Start()
    {
        StartCoroutine("LateStart");
    }

    private IEnumerator LateStart() {
        yield return new WaitForEndOfFrame();
        StartCoroutine(turnPhaseEvent.RaiseAtEndOfFrameCoroutine(new TurnPhaseEventInfo(TurnPhase.START_ENCOUNTER)));
    }

    public void turnPhaseChangedEventHandler(TurnPhaseEventInfo info){
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
    }
    
    private IEnumerator nextPhaseAfterTriggers(TurnPhase currentPhase) {
        Debug.Log("nextPhaseAfterTriggers found " + turnPhaseTriggers[currentPhase].Count + " triggers for phase " + currentPhase);
        yield return StartCoroutine(runTriggersForPhase(currentPhase));
        StartCoroutine(turnPhaseEvent.RaiseAtEndOfFrameCoroutine(new TurnPhaseEventInfo(nextPhase[currentPhase])));
    }

    private IEnumerator runTriggersForPhase(TurnPhase phase) {
        foreach(TurnPhaseTrigger trigger in turnPhaseTriggers[phase]) {
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
        Debug.Log("Victory!");
        endEncounterEvent.Raise(new EndEncounterEventInfo(EncounterOutcome.Victory));
    }

    public void registerTurnPhaseTriggerEventHandler(TurnPhaseTriggerEventInfo info) {
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
}