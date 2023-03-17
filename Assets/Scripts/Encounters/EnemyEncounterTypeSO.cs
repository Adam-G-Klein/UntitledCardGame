using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "EnemyEncounterType", 
    menuName = "Encounters/Enemy Encounters/Enemy Encounter Type")]
public class EnemyEncounterTypeSO : ScriptableObject {
    public List<EnemyTypeSO> enemies;
}