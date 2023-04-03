using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewEnemyEncounterType", 
    menuName = "Encounters/Enemy Encounter/Enemy Encounter Type")]
public class EnemyEncounterTypeSO: ScriptableObject {
    public List<EnemyTypeSO> enemies;
}