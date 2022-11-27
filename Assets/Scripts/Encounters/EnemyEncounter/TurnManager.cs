using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        StartCoroutine(turnPhaseEvent.RaiseAtEndOfFrameCoroutine(new TurnPhaseEventInfo(TurnPhase.ENEMIES_TURN)));
    }

    public void turnPhaseChangedEventHandler(TurnPhaseEventInfo info){
        switch(info.newPhase){
            case TurnPhase.END_ENEMY_TURN:
                // Currently raised by the EnemyManager
                StartCoroutine(turnPhaseEvent.RaiseAtEndOfFrameCoroutine(new TurnPhaseEventInfo(TurnPhase.START_PLAYER_TURN)));
                break;
            case TurnPhase.START_PLAYER_TURN:
                // If we raise the event immediately, the end turn button hears the PLAYER_TURN
                // event before the START_PLAYER_TURN event, and disables itself on hearing START_PLAYER_TURN
                // Unsure if this will be needed everywhere
                StartCoroutine(turnPhaseEvent.RaiseAtEndOfFrameCoroutine(new TurnPhaseEventInfo(TurnPhase.PLAYER_TURN)));
                break;
            case TurnPhase.END_PLAYER_TURN:
                // Currently just raised by the end turn button in the UI
                // no op for now, companions and a bunch of ui elements will probably listen to this
                StartCoroutine(turnPhaseEvent.RaiseAtEndOfFrameCoroutine(new TurnPhaseEventInfo(TurnPhase.START_ENEMY_TURN)));
                break;
        }
    }
}