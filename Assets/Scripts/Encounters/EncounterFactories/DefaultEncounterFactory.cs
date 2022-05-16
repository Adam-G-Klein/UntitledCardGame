using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultEncounterFactory : MonoBehaviour
{
    public void generateEncounter(DefaultEncounter encounter){
        PrefabStore prefabStore = GameObject.Find("PrefabStore").GetComponent<PrefabStore>();
        EnemyLocationStore locStore = GameObject.Find("EnemyLocationStore").GetComponent<EnemyLocationStore>();
        List<Enemy> enemies = encounter.getEnemies();
        List<Vector2> locList = locStore.getLocs(enemies.Count);
        Enemy enemy;
        for(int i = 0; i < enemies.Count ; i +=1 ){
            enemy = enemies[i];
            Object.Instantiate(
                prefabStore.getPrefabByName(enemy.getPrefabName()),
                locList[i], 
                Quaternion.identity);
            

        }
        
    }

}
