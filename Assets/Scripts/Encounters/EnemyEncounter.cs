using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyEncounter : Encounter
{
    [Header("Enemies")]
    public List<Enemy> enemyList;
    public List<Vector3> enemyLocations;
    
    [Header("Companions")]
    public CompanionListVariableSO activeCompanions;
    public List<Vector3> companionLocations;

    private EncounterConstants encounterConstants;

    public override void build(EncounterConstants constants)
    {
        this.encounterConstants = constants;
        setupEnemies();
        setupCompanions();
        this.encounterType = EncounterType.Enemy;
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
                encounterConstants.enemyPrefab, 
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
                encounterConstants.companionPrefab,
                companionLocations[i],
                Quaternion.identity);
            instantiatedCompanion.GetComponent<CompanionInstance>().companion = 
                activeCompanions.companionList[i];
        }
    }
}
