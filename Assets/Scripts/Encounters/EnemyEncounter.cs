using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyEncounter : Encounter
{
    public List<Enemy> enemyList = new List<Enemy>();

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

}
