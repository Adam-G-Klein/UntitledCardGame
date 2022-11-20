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
        Debug.Log("Enemy " + info.enemy.id + " Instantiated and added to manager");
        enemies.Add(info.enemy);
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

        // See EnemyTurnFinishedEvent.cs for how this should work in the future
        // Will want a callback that increments a counter and raises a turn phase
        // event when all the enemies have attacked
        foreach(EnemyInstance enemyInstance in enemies){
            enemyInstance.turnStartEventHandler();
        }
    }

    public void enemyTurnFinishedEventHandler(EnemyTurnFinishedEventInfo info){
        enemiesDoneWithTurn++;
        if(enemiesDoneWithTurn == enemies.Count){
            turnPhaseEvent.Raise(new TurnPhaseEventInfo(TurnPhase.END_ENEMY_TURN));
            enemiesDoneWithTurn = 0;
        }
    }

    public void turnPhaseEventHandler(TurnPhaseEventInfo info) {
        switch(info.newPhase) {
            case TurnPhase.START_ENEMY_TURN:
                //no op for now
                break;
            case TurnPhase.ENEMIES_TURN:
                Debug.Log("Enemy Manager instructing enemies to attack");
                enemiesAttack();
                break;
            case TurnPhase.END_ENEMY_TURN:
                //no op for now
                break;
        }
    }

}
