using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "EnemyEncounter", 
    menuName = "Encounters/Encounter Type/Enemy Encounter")]
public class EnemyEncounter : EncounterTypeSO
{
    [Header("Enemies")]
    public List<EnemyInEncounter> enemyList;
    public GameObject enemyPrefab;
    
    [Header("Companions")]
    public CompanionListVariable activeCompanions;
    public List<CompanionLocations> companionLocationsList;
    public GameObject companionPrefab;

    public override void Build()
    {
        setupEnemies();
        setupCompanions();
    }

    private void setupEnemies()
    {
        GameObject instantiatedEnemy;

        for(int i = 0; i < enemyList.Count; i++)
        {
            instantiatedEnemy = Instantiate(
                enemyPrefab, 
                enemyList[i].location,
                Quaternion.identity);
            instantiatedEnemy.GetComponent<EnemyInstance>().enemyType = enemyList[i].enemy;
        }
    }

    private void setupCompanions()
    {
        List<Vector2> companionLocations = null;
        GameObject instantiatedCompanion;

        int activeCompanionsCount = activeCompanions.companionList.Count;
        for(int i = 0; i < companionLocationsList.Count; i++)
        {
            if (companionLocationsList[i].count == activeCompanionsCount)
            {
                companionLocations = companionLocationsList[i].locations;
            }
        }

        if (companionLocations == null)
        {
            Debug.Log("Number of active companions doesn't exist in companion location store!");
            return;
        }

        for(int i = 0; i < activeCompanionsCount; i++)
        {
            instantiatedCompanion = Instantiate(
                companionPrefab,
                companionLocations[i],
                Quaternion.identity);
            instantiatedCompanion.GetComponent<CompanionInstance>().companion = 
                activeCompanions.companionList[i];
        }
    }
}
