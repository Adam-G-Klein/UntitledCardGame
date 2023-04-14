using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Turnphases can be found in TurnPhaseEvent.cs
[RequireComponent(typeof(TurnPhaseEventListener))]
public class TurnManager : MonoBehaviour
{
    [SerializeField]
    private TurnPhaseEvent turnPhaseEvent;
    // Start is called before the first frame update

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
        {TurnPhase.END_ENEMY_TURN, new List<TurnPhaseTrigger>()}
    };

    void Start()
    {
        StartCoroutine("LateStart");
    }

    // Update is called once per frame
    void Update()
    {
        
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
        StartCoroutine(nextPhaseAfterTriggers(info.newPhase));
    }
    
    private IEnumerator nextPhaseAfterTriggers(TurnPhase currentPhase) {
        Debug.Log("nextPhaseAfterTriggers found " + turnPhaseTriggers[currentPhase].Count + " triggers for phase " + currentPhase);
        foreach(TurnPhaseTrigger trigger in turnPhaseTriggers[currentPhase]) {
            yield return StartCoroutine(trigger.triggerResponse.GetEnumerator());
        }
        StartCoroutine(turnPhaseEvent.RaiseAtEndOfFrameCoroutine(new TurnPhaseEventInfo(nextPhase[currentPhase])));
    }

    public void registerTurnPhaseTriggerEventHandler(TurnPhaseTriggerEventInfo info) {
        addTurnPhaseTrigger(info.turnPhaseTrigger);
    }

    public void removeTurnPhaseTriggerEventHandler(TurnPhaseTriggerEventInfo info) {
        removeTurnPhaseTrigger(info.turnPhaseTrigger);
    }

    private void addTurnPhaseTrigger(TurnPhaseTrigger trigger) {
        turnPhaseTriggers[trigger.phase].Add(trigger);
    }

    public void removeTurnPhaseTrigger(TurnPhaseTrigger trigger) {
        turnPhaseTriggers[trigger.phase].Remove(trigger);
    }
}