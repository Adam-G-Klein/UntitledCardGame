using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyEncounter : Encounter
{
    public List<Enemy> enemyList = new List<Enemy>();

    private EncounterConstantsSO encounterConstants;

    private LocationStore companionLocationStore;
    private LocationStore enemyLocationStore;

    public EnemyEncounter() {
        this.encounterType = EncounterType.Enemy;
    }

    public EnemyEncounter(EnemyEncounterTypeSO enemyEncounterType) {
        this.encounterType = EncounterType.Enemy;
        enemyList = new List<Enemy>();
        foreach (EnemyTypeSO enemyType in enemyEncounterType.enemies) {
            enemyList.Add(new Enemy(enemyType));
        }
    }

    // sorry to mess up the beautiful pattern but we gotta go fast here
    public override void BuildWithEncounterBuilder(IEncounterBuilder encounterBuilder) {
        encounterBuilder.BuildEnemyEncounter(this, encounterBuilder.companionLocationStore, encounterBuilder.enemyLocationStore);
    }

    public void Build(
            List<Companion> companionList,
            EncounterConstantsSO constants,
            List<CompanionInstance> createdCompanions,
            List<EnemyInstance> createdEnemies,
            LocationStore companionLocationStore,
            LocationStore enemyLocationStore)
    {
        this.encounterType = EncounterType.Enemy;
        this.encounterConstants = constants;
        this.companionLocationStore = companionLocationStore;
        this.enemyLocationStore = enemyLocationStore;   
        setupEnemies(createdEnemies);
        setupCompanions(companionList, createdCompanions);
    }

    private void setupEnemies(List<EnemyInstance> createdEnemies)
    {
        if (enemyList.Count > enemyLocationStore.getTopLevelCount()) {
            Debug.LogError("The enemy locations list does not contain enough locations");
            return;
        }

        for(int i = 0; i < enemyList.Count; i++)
        {
            createdEnemies.Add(PrefabInstantiator.instantiateEnemy(
                encounterConstants.enemyPrefab,
                enemyList[i],
                enemyLocationStore.getLoc(i),
                enemyLocationStore.transform));
        }
    }

    private void setupCompanions(List<Companion> companionList, List<CompanionInstance> createdCompanions)
    {
        int activeCompanionsCount = companionList.Count;
        if (activeCompanionsCount > companionLocationStore.getTopLevelCount()) {
            Debug.LogError("The companion locations list does not contain enough locations");
            return;
        }

        for(int i = 0; i < activeCompanionsCount; i++)
        {
            createdCompanions.Add(PrefabInstantiator.InstantiateCompanion(
                encounterConstants.companionPrefab,
                companionList[i],
                companionLocationStore.getLoc(i),
                companionLocationStore.transform));
        }
    }
}
