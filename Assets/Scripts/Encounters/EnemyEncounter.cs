using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class EnemyEncounter : Encounter
{
    public bool isEliteEncounter;
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

    public void SetIsElite(bool isElite) {
        isEliteEncounter = isElite;
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
        Debug.Log("EnemyEncounter: enemy list count: " + enemyList.Count + "=?" + placer.getEnemyPlacesCount());

        if (enemyList.Count > placer.getEnemyPlacesCount()) {
            Debug.LogError("The UIDocument does not contain enough enemy places!");
            return;
        }


        for(int i = 0; i < enemyList.Count; i++)
        {
            WorldPositionVisualElement newEnemyPlacement = placer.checkoutEnemyMapping();
            EnemyInstance newEnemy = PrefabInstantiator.instantiateEnemy(
                encounterConstants.enemyPrefab,
                newEnemyPlacement.worldPos,
                encounterBuilder.transform);
            newEnemy.Setup(newEnemyPlacement, enemyList[i]);
            createdEnemies.Add(newEnemy);
            placer.addMapping(newEnemyPlacement, newEnemy.gameObject);
        }
    }

    private void setupCompanions(List<Companion> companionList, List<CompanionInstance> createdCompanions)
    {
        Debug.Log("EnemyEncounter: companion list count: " + companionList.Count + "=?" + placer.getCompanionPlacesCount());
        if (companionList.Count > placer.getCompanionPlacesCount())
        {
            Debug.LogError("The UIDocument does not contain enough companion places!");
            return;
        }

        for (int i = 0; i < companionList.Count; i++)
        {
            WorldPositionVisualElement newCompanionPlacement = placer.checkoutCompanionMapping();
            CompanionInstance newCompanion = PrefabInstantiator.InstantiateCompanion(
                encounterConstants.companionPrefab,
                newCompanionPlacement.worldPos,
                encounterBuilder.transform);
            newCompanion.Setup(newCompanionPlacement, companionList[i]);
            createdCompanions.Add(newCompanion);
            placer.addMapping(newCompanionPlacement, newCompanion.gameObject);
        }
    }
}

[System.Serializable] 
public class EnemyEncounterSerializable : EncounterSerializable
{
    public List<EnemySerializeable> enemies;
    public bool isEliteEncounter;

    public EnemyEncounterSerializable(EnemyEncounter encounter) : base(encounter)
    {
        this.enemies = encounter.enemyList
            .Select(enemy => new EnemySerializeable(enemy))
            .ToList();
        this.isEliteEncounter = encounter.isEliteEncounter;
    }
}