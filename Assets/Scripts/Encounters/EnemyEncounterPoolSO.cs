using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName ="NewEncounterPool",
    menuName = "Encounters/Enemy Encounter/Encounter Pool")]
public class EnemyEncounterPoolSO: ScriptableObject {
    public List<EnemyEncounterTypeSO> enemyEncounterTypes;

    public EnemyEncounterTypeSO getEnemyEncounterType() {
        int encounterTypeNumber = Random.Range(0, enemyEncounterTypes.Count);
        return enemyEncounterTypes[encounterTypeNumber];
    }
}