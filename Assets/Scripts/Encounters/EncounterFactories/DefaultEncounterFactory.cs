using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultEncounterFactory 
{
    public DefaultEncounterFactory(){}

    public void generateEncounter(DefaultEncounter encounter){
        EncounterManager manager = GameObject.FindGameObjectWithTag("Managers").GetComponent<EncounterManager>();
        manager.setActiveEncounter(encounter);
        RoomManager roomManager = GameObject.FindGameObjectWithTag("Managers").GetComponent<RoomManager>();
        Debug.Log("activeRoom at encounterGeneration: " + roomManager.getActiveRoom().getSceneString());
        generateEnemies(encounter);
        generatePlayer();
    }

    private void generateEnemies(DefaultEncounter encounter){
        PrefabStore prefabStore = GameObject.Find("PrefabStore").GetComponent<PrefabStore>();
        LocationStore locStore = GameObject.FindGameObjectWithTag("EnemyStore").GetComponent<LocationStore>();
        List<Enemy> enemies = encounter.getEnemies();
        List<Vector2> locList = locStore.getLocs(enemies.Count);
        Enemy enemy;
        for(int i = 0; i < enemies.Count ; i +=1 ){
            enemy = enemies[i];
            // to be replaced by enemy factory call
            // that replacement will also eliminate the need for the prefab store
            Object.Instantiate(
                prefabStore.getPrefabByName(enemy.getPrefabName()),
                locList[i], 
                Quaternion.identity);
        }
    }

    private void generatePlayer(){

        PlayerInEncounterFactory playerInEncounterFactory = new PlayerInEncounterFactory();
        playerInEncounterFactory.generatePlayer();
    }

}
