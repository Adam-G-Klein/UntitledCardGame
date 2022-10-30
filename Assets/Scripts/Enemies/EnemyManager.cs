using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyInstantiatedEventListener))]
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
    private List<EnemyInstance> enemies = new List<EnemyInstance>();

    public void enemyInstantiatedEventHandler(EnemyInstantiatedEventInfo info){
        Debug.Log("Enemy " + info.enemy.id + " Instantiated and added to manager");
        enemies.Add(info.enemy);
    }

    public string getRandomEnemyId(){
        return enemies[Random.Range(0,enemies.Count)].id;
    }
    
}
