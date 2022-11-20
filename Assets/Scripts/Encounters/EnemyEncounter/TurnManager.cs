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
        turnPhaseEvent.Raise(new TurnPhaseEventInfo(TurnPhase.ENEMIES_TURN));
    }

    public void turnPhaseChangedEventHandler(TurnPhaseEventInfo info){
        switch(info.newPhase){
            case TurnPhase.END_ENEMY_TURN:
                turnPhaseEvent.Raise(new TurnPhaseEventInfo(TurnPhase.START_PLAYER_TURN));
                break;
            case TurnPhase.START_PLAYER_TURN:
                // no op for now, companions and a bunch of ui elements will probably listen to this
                turnPhaseEvent.Raise(new TurnPhaseEventInfo(TurnPhase.PLAYER_TURN));
                break;
        }
    }
}