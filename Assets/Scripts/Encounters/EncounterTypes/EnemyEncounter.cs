using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyEncounter : Encounter
{
    [Header("Enemies")]
    public List<Enemy> enemyList;
    public List<Vector2> enemyLocations;
    public GameObject enemyPrefab;
    
    [Header("Companions")]
    public CompanionListVariable activeCompanions;
    public List<Vector2> companionLocations;
    public GameObject companionPrefab;

    public override void build()
    {
        setupEnemies();
        setupCompanions();
    }

    private void setupEnemies()
    {
        GameObject instantiatedEnemy;
        if (enemyList.Count > enemyLocations.Count) {
            Debug.LogError("The enemy locations list does not contain enough locations");
            return;
        }

        for(int i = 0; i < enemyList.Count; i++)
        {
            instantiatedEnemy = GameObject.Instantiate(
                enemyPrefab, 
                enemyLocations[i],
                Quaternion.identity);
            instantiatedEnemy.GetComponent<EnemyInstance>().enemy = enemyList[i];
        }
    }

    private void setupCompanions()
    {
        GameObject instantiatedCompanion;

        int activeCompanionsCount = activeCompanions.companionList.Count;
        if (activeCompanionsCount > companionLocations.Count) {
            Debug.LogError("The companion locations list does not contain enough locations");
            return;
        }

        for(int i = 0; i < activeCompanionsCount; i++)
        {
            instantiatedCompanion = GameObject.Instantiate(
                companionPrefab,
                companionLocations[i],
                Quaternion.identity);
            instantiatedCompanion.GetComponent<CompanionInstance>().companion = 
                activeCompanions.companionList[i];
        }
    }
}
