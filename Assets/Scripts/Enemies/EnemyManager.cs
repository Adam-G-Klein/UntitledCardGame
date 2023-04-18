using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CombatEntityDeathEventListener))]
[RequireComponent(typeof(CombatEntityInstantiatedEventListener))]
public class EnemyManager : MonoBehaviour
{

    [SerializeField]
    private TurnPhaseEvent turnPhaseEvent;
    
    private List<EnemyInstance> enemies = new List<EnemyInstance>();

    public string getRandomEnemyId(){
        return enemies[Random.Range(0,enemies.Count)].id;
    }

    public void enemiesAttack() {

        foreach(EnemyInstance enemyInstance in enemies){
            enemyInstance.turnStartEventHandler();
        }
    }

    public void combatEntityInstantiatedHandler(CombatEntityInstantiatedEventInfo info) {
        if(info.instance is EnemyInstance){
            enemies.Add((EnemyInstance) info.instance);
        }
    }

    public void combatEntityDeathHandler(CombatEntityDeathEventInfo info) {
        if(info.instance is EnemyInstance){
            enemies.Remove((EnemyInstance) info.instance);
            if(enemies.Count == 0) {
                StartCoroutine(turnPhaseEvent.RaiseAtEndOfFrameCoroutine(new TurnPhaseEventInfo(TurnPhase.END_ENCOUNTER)));
            }
        }
    }

    public List<string> getEnemyIds(){
        List<string> returnList = new List<string>();
        foreach(EnemyInstance instance in enemies) {
            returnList.Add(instance.id);
        }
        return returnList;
    }

    public List<EnemyInstance> getEnemies(){
        return enemies;
    }

}
