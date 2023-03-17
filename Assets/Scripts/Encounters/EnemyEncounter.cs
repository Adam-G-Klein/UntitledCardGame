using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyEncounter : Encounter
{
    private List<Enemy> enemyList;
    private EnemyEncounterTypeSO enemyEncounterType;
    private EncounterConstants encounterConstants;

    // Just going to hardcode these here for now, will figure this out later
    private static List<Vector3> COMPANION_LOCATIONS = new List<Vector3>() {
        new Vector3(-4, -2, 0),
        new Vector3(-3, 0.5f, 0),
        new Vector3(-6, 0, 0)
    };

    private static List<Vector3> ENEMY_LOCATIONS = new List<Vector3>() {
        new Vector3(3.75f, 1.5f, 0),
        new Vector3(7.5f, 1.5f, 0)
    };

    public EnemyEncounter(EnemyEncounterTypeSO enemyEncounterType) {
        this.encounterType = EncounterType.Enemy;
        this.enemyEncounterType = enemyEncounterType;

        foreach (EnemyTypeSO enemyType in this.enemyEncounterType.enemies) {
            enemyList.Add(new Enemy(enemyType));
        }
    }

    public override void build(List<Companion> companionList, EncounterConstants constants)
    {
        this.encounterType = EncounterType.Enemy;
        this.encounterConstants = constants;
        buildEnemies();
        buildCompanions(companionList);
    }

    private void buildEnemies()
    {
        if (enemyList.Count > ENEMY_LOCATIONS.Count) {
            Debug.LogError("The enemy locations list does not contain enough locations");
            return;
        }

        for(int i = 0; i < enemyList.Count; i++)
        {
            PrefabInstantiator.instantiateEnemy(
                encounterConstants.enemyPrefab,
                enemyList[i],
                ENEMY_LOCATIONS[i]);
        }
    }

    private void buildCompanions(List<Companion> companionList)
    {

        int activeCompanionsCount = companionList.Count;
        if (activeCompanionsCount > COMPANION_LOCATIONS.Count) {
            Debug.LogError("The companion locations list does not contain enough locations");
            return;
        }

        for(int i = 0; i < activeCompanionsCount; i++)
        {
            PrefabInstantiator.instantiateCompanion(
                encounterConstants.companionPrefab,
                companionList[i],
                COMPANION_LOCATIONS[i]);
        }
    }
}
