using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.UIElements;

[System.Serializable]
public class EnemyEncounter : Encounter
{
    public bool isEliteEncounter;
    public int bonusManaReward = 0;
    public int bonusTeamSizeReward = 0;
    public List<Enemy> enemyList = new List<Enemy>();

    private EncounterConstantsSO encounterConstants;

    private UIDocumentGameObjectPlacer placer;
    private IEncounterBuilder encounterBuilder;
    public string encounterName;

    public EnemyEncounter() {
        this.encounterType = EncounterType.Enemy;
    }

    public EnemyEncounter(EnemyEncounterTypeSO enemyEncounterType) {
        this.encounterType = EncounterType.Enemy;
        enemyList = new List<Enemy>();
        foreach (EnemyTypeSO enemyType in enemyEncounterType.enemies)
        {
            enemyList.Add(new Enemy(enemyType));
        }
        encounterName = enemyEncounterType.name;
    }

    public EnemyEncounter(EnemyEncounterSerializable enemyEncounterSerializable, SORegistry registry) {
        this.act = enemyEncounterSerializable.act;
        this.encounterType = EncounterType.Enemy;
        this.enemyList = enemyEncounterSerializable.enemies.Select(enemy => new Enemy(enemy, registry)).ToList();
        this.isEliteEncounter = enemyEncounterSerializable.isEliteEncounter;
        this.encounterName = enemyEncounterSerializable.encounterName;
        this.bonusManaReward = enemyEncounterSerializable.bonusManaReward;
        this.bonusTeamSizeReward = enemyEncounterSerializable.bonusTeamSizeReward;
    }

    public void SetIsElite(bool isElite) {
        isEliteEncounter = isElite;
    }

    public void SetBonusMana(int bonusMana) {
        this.bonusManaReward = bonusMana;
    }

    public void SetBonusTeamSize(int bonusTeamSize) {
        this.bonusTeamSizeReward = bonusTeamSize;
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
            GameObject enemyPrefabToInstantiate = getEnemyPrefabToInstantiate(enemyList[i]);
            Vector2 enemyPosition = getEnemySpawnLocation(enemyList[i], newEnemyPlacement);
            if(enemyPrefabToInstantiate == null) {
                Debug.LogError("Enemy prefab is null, you probably didnt set it in the encounterconstants");
            }
            EnemyInstance newEnemy = PrefabInstantiator.instantiateEnemy(
                enemyPrefabToInstantiate,
                enemyPosition,
                encounterBuilder.transform);
            newEnemy.Setup(newEnemyPlacement, enemyList[i], (float) i / enemyList.Count);
            IBossController bossController = newEnemy.TryGetBossController();
            if (bossController != null)
            {
                bossController.Setup();
                EnemyEncounterManager.Instance.isBoss = true;

            }
            createdEnemies.Add(newEnemy);
            placer.addMapping(newEnemyPlacement, newEnemy.gameObject);
        }
    }

    private GameObject getEnemyPrefabToInstantiate(Enemy enemy) {
        GameObject toInstantiate;
        switch(enemy.enemyType.enemyDisplayType) {
            case DisplayType.MEOTHRA:
                toInstantiate = encounterConstants.SmokeAndArmsBossPrefab;
                break;
            default:
                toInstantiate = encounterConstants.enemyPrefab;
                break;
        }
        return toInstantiate;
    }

    private Vector2 getEnemySpawnLocation(Enemy enemy, WorldPositionVisualElement worldPositionVisualElement)
    {

        Vector2 toReturn;
        switch (enemy.enemyType.enemyDisplayType)
        {
            case DisplayType.MEOTHRA:
                toReturn = encounterConstants.bossSpawnLocation;
                break;
            default:
                toReturn = worldPositionVisualElement.worldPos;
                break;
        }
        return toReturn;
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
    public string encounterName;
    public int bonusManaReward = 0;
    public int bonusTeamSizeReward = 0;

    public EnemyEncounterSerializable(EnemyEncounter encounter) : base(encounter)
    {
        this.enemies = encounter.enemyList
            .Select(enemy => new EnemySerializeable(enemy))
            .ToList();
        this.isEliteEncounter = encounter.isEliteEncounter;
        this.encounterName = encounter.encounterName;
        this.bonusManaReward = encounter.bonusManaReward;
        this.bonusTeamSizeReward = encounter.bonusTeamSizeReward;
    }
}