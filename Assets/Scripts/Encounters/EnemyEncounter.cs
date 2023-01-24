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

    public EnemyEncounter() {
        this.encounterType = EncounterType.Enemy;
    }

    public override void build(EncounterConstants constants)
    {
        this.encounterType = EncounterType.Enemy;
        this.encounterConstants = constants;
        setupEnemies();
        setupCompanions();
    }

    private void setupEnemies()
    {
        if (enemyList.Count > enemyLocations.Count) {
            Debug.LogError("The enemy locations list does not contain enough locations");
            return;
        }

        for(int i = 0; i < enemyList.Count; i++)
        {
            PrefabInstantiator.instantiateEnemy(
                encounterConstants.enemyPrefab,
                enemyList[i],
                enemyLocations[i]);
        }
    }

    private void setupCompanions()
    {

        int activeCompanionsCount = activeCompanions.companionList.Count;
        if (activeCompanionsCount > companionLocations.Count) {
            Debug.LogError("The companion locations list does not contain enough locations");
            return;
        }

        for(int i = 0; i < activeCompanionsCount; i++)
        {
            PrefabInstantiator.instantiateCompanion(
                encounterConstants.companionPrefab,
                activeCompanions.companionList[i],
                companionLocations[i]);
        }
    }
}
