using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultEncounterFactory 
{
    public DefaultEncounterFactory(){}

    public void generateEncounter(DefaultEncounter encounter){
        GameObject managersGameObject = GameObject.FindGameObjectWithTag("Managers");
        EncounterManager manager = managersGameObject.GetComponent<EncounterManager>();
        manager.setActiveEncounter(encounter);
        RoomManager roomManager = managersGameObject.GetComponent<RoomManager>();
        EnemyManager enemyManager = managersGameObject.GetComponent<EnemyManager>();
        Debug.Log("activeRoom at encounterGeneration: " + roomManager.getActiveRoom().getSceneString());
        generateEnemies(encounter, enemyManager);
        generatePlayer();
    }

    private void generateEnemies(DefaultEncounter encounter, EnemyManager enemyManager){
        PrefabStore prefabStore = GameObject.Find("PrefabStore").GetComponent<PrefabStore>();
        LocationStore locStore = GameObject.FindGameObjectWithTag("EnemyStore").GetComponent<LocationStore>();
        List<Enemy> enemies = encounter.getEnemies();
        List<Vector2> locList = locStore.getLocs(enemies.Count);
        Enemy enemy;
        for(int i = 0; i < enemies.Count ; i +=1 ){
            enemy = enemies[i];
            enemy.buildEnemy(
                prefabStore.getPrefabByName(enemy.getPrefabName()),
                locList[i]);
        }
        enemyManager.setEnemies(enemies);
    }

    private void generatePlayer(){

        PlayerInEncounterFactory playerInEncounterFactory = new PlayerInEncounterFactory();
        playerInEncounterFactory.generatePlayer();
    }

}
