using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyInstantiatedEventListener))]
[RequireComponent(typeof(EnemyTurnFinishedEventListener))]
public class EnemyManager : MonoBehaviour
{
    /* There should never be much code in here if we 
        want to keep with the pattern of having very 
        modular scenes. All I know right now is that we'll need 
        something in the scene that listens to companions/enemy
        instantiations and knows about all of them
    */
    //For now it looks like we don't have an Enemy class the same way
    // we have a Companion class. Going with EnemyInstance for now because
    // it's where we have the Ids

    public int enemiesDoneWithTurn = 0;
    private List<EnemyInstance> enemies = new List<EnemyInstance>();

    [SerializeField]
    private TurnPhaseEvent turnPhaseEvent;

    public void enemyInstantiatedEventHandler(EnemyInstantiatedEventInfo info){
        Debug.Log("Enemy " + info.enemyInstance.id + " Instantiated and added to manager");
        enemies.Add(info.enemyInstance);
    }

    public string getRandomEnemyId(){
        return enemies[Random.Range(0,enemies.Count)].id;
    }

    // If we ever want them to attack one at a time, the only
    // way I can see to avoid having the enemy manager do this
    // is by having messages sent between the enemies
    // to decide which one will attack 
    // Not implementing that right now, can be a TODO

    public void enemiesAttack() {

        foreach(EnemyInstance enemyInstance in enemies){
            enemyInstance.turnStartEventHandler();
        }
    }

    public List<string> getEnemyIds(){
        List<string> returnList = new List<string>();
        foreach(EnemyInstance instance in enemies) {
            returnList.Add(instance.id);
        }
        return returnList;
    }

    public void enemyTurnFinishedEventHandler(EnemyTurnFinishedEventInfo info){
        enemiesDoneWithTurn++;
        if(enemiesDoneWithTurn == enemies.Count){
            StartCoroutine(turnPhaseEvent.RaiseAtEndOfFrameCoroutine(new TurnPhaseEventInfo(TurnPhase.END_ENEMY_TURN)));
            enemiesDoneWithTurn = 0;
        }
    }

    public void turnPhaseEventHandler(TurnPhaseEventInfo info) {
        switch(info.newPhase) {
            case TurnPhase.START_ENEMY_TURN:
                //no op for now
                StartCoroutine(turnPhaseEvent.RaiseAtEndOfFrameCoroutine(new TurnPhaseEventInfo(TurnPhase.ENEMIES_TURN)));
                break;
            case TurnPhase.ENEMIES_TURN:
                Debug.Log("Enemy Manager instructing enemies to attack");
                enemiesAttack();
                break;
            case TurnPhase.END_ENEMY_TURN:
                //no op for now, this event is picked up by the turn manager
                break;
        }
    }

}
