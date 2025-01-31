using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(
    fileName ="NewEncounterPool",
    menuName = "Encounters/Enemy Encounter/Encounter Pool")]
public class EnemyEncounterPoolSO: ScriptableObject {
    public List<EnemyEncounterTypeSO> enemyEncounterTypes;
}
