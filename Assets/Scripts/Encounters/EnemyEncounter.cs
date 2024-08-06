using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class EnemyEncounter : Encounter
{
    public List<Enemy> enemyList = new List<Enemy>();

    private EncounterConstantsSO encounterConstants;

    private UIDocumentGameObjectPlacer placer;
    private IEncounterBuilder encounterBuilder;

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
        this.encounterBuilder = encounterBuilder;
        encounterBuilder.BuildEnemyEncounter(this, encounterBuilder.placer);
    }

    public void Build(
            List<Companion> companionList,
            EncounterConstantsSO constants,
            List<CompanionInstance> createdCompanions,
            List<EnemyInstance> createdEnemies,
            UIDocumentGameObjectPlacer placer)
    {
        this.encounterType = EncounterType.Enemy;
        this.encounterConstants = constants;
        this.placer = placer;
        setupEnemies(createdEnemies);
        setupCompanions(companionList, createdCompanions);
    }

    private void setupEnemies(List<EnemyInstance> createdEnemies)
    {
        if (enemyList.Count > placer.getEnemyPlacesCount()) {
            Debug.LogError("The UIDocument does not contain enough enemy places!");
            return;
        }

        Debug.Log("EnemyEncounter: enemy list count: " + enemyList.Count);

        for(int i = 0; i < enemyList.Count; i++)
        {
            EnemyInstance newEnemy = PrefabInstantiator.instantiateEnemy(
                encounterConstants.enemyPrefab,
                enemyList[i],
                placer.getNextEnemyPosition(),
                encounterBuilder.transform);
            createdEnemies.Add(newEnemy);
            placer.addMapping(newEnemy.gameObject);
        }
    }

    private void setupCompanions(List<Companion> companionList, List<CompanionInstance> createdCompanions)
    {
        if (companionList.Count > placer.getCompanionPlacesCount()) {
            Debug.LogError("The UIDocument does not contain enough companion places!");
            return;
        }

        Debug.Log("EnemyEncounter: companion list count: " + companionList.Count);

        for(int i = 0; i < companionList.Count; i++)
        {
            CompanionInstance newCompanion = PrefabInstantiator.InstantiateCompanion(
                encounterConstants.companionPrefab,
                companionList[i],
                placer.getNextCompanionPosition(),
                encounterBuilder.transform);
            createdCompanions.Add(newCompanion);
            placer.addMapping(newCompanion.gameObject);
        }
    }
}
