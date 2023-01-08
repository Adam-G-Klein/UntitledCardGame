using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/* Turn phases:
Draw card from each companion
Player plays cards until end turn is pressed
Enemies deal damage
*/
[RequireComponent(typeof(TurnPhaseEventListener))]
public class TurnManager : MonoBehaviour
{
    [SerializeField]
    private TurnPhaseEvent turnPhaseEvent;
    // Start is called before the first frame update

    private Dictionary<TurnPhase, TurnPhase> nextPhase = new Dictionary<TurnPhase, TurnPhase>(){
        {TurnPhase.START_PLAYER_TURN, TurnPhase.PLAYER_TURN},
        {TurnPhase.PLAYER_TURN, TurnPhase.BEFORE_END_PLAYER_TURN},
        {TurnPhase.BEFORE_END_PLAYER_TURN, TurnPhase.END_PLAYER_TURN},
        {TurnPhase.END_PLAYER_TURN, TurnPhase.START_ENEMY_TURN},
        {TurnPhase.START_ENEMY_TURN, TurnPhase.ENEMIES_TURN},
        {TurnPhase.ENEMIES_TURN, TurnPhase.END_ENEMY_TURN},
        {TurnPhase.END_ENEMY_TURN, TurnPhase.START_PLAYER_TURN}
    };

    private Dictionary<TurnPhase, List<TurnPhaseTrigger>> turnPhaseTriggers = new Dictionary<TurnPhase, List<TurnPhaseTrigger>>(){
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
        StartCoroutine(turnPhaseEvent.RaiseAtEndOfFrameCoroutine(new TurnPhaseEventInfo(TurnPhase.START_PLAYER_TURN)));
    }

    public void turnPhaseChangedEventHandler(TurnPhaseEventInfo info){
        switch(info.newPhase){
            case TurnPhase.END_ENEMY_TURN:
                // Currently raised by the EnemyManager
                StartCoroutine(nextPhaseAfterTriggers(info.newPhase));
                break;
            case TurnPhase.START_PLAYER_TURN:
                // If we raise the event immediately, the end turn button hears the PLAYER_TURN
                // event before the START_PLAYER_TURN event, and disables itself on hearing START_PLAYER_TURN
                // Unsure if this will be needed everywhere
                StartCoroutine(nextPhaseAfterTriggers(info.newPhase));
                break;
            case TurnPhase.BEFORE_END_PLAYER_TURN: 
                // Currently raised by the end turn button in the UI
                StartCoroutine(nextPhaseAfterTriggers(info.newPhase));
                break;
            case TurnPhase.END_PLAYER_TURN:
                // Currently just raised by the end turn button in the UI
                // no op for now, companions and a bunch of ui elements will probably listen to this
                StartCoroutine(nextPhaseAfterTriggers(info.newPhase));
                break;
        }
    }
    
    private IEnumerator nextPhaseAfterTriggers(TurnPhase currentPhase) {
        Debug.Log("nextPhaseAfterTriggers found " + turnPhaseTriggers[currentPhase].Count + " triggers for phase " + currentPhase);
        foreach(TurnPhaseTrigger trigger in turnPhaseTriggers[currentPhase]) {
            yield return StartCoroutine(trigger.triggerResponse.GetEnumerator());
        }
        /*
        yield return new WaitUntil(() => turnPhaseTriggers[currentPhase].All(
            trigger => trigger.isFinished)
        );
        */
        StartCoroutine(turnPhaseEvent.RaiseAtEndOfFrameCoroutine(new TurnPhaseEventInfo(nextPhase[currentPhase])));
    }

    public void addTurnPhaseTrigger(TurnPhaseTrigger trigger) {
        turnPhaseTriggers[trigger.phase].Add(trigger);
    }
}