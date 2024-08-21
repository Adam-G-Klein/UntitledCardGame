using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyInstanceController : MonoBehaviour
{
    [SerializeField] private Transform parentTransform;

    public List<EnemyInstance> SetupEnemies(List<Enemy> enemies, EncounterConstantsSO encounterConstants) {
        List<EnemyInstance> created = new();
        foreach (Enemy enemy in enemies) {
            Debug.Log("instating enemy " + enemy.enemyType.name.ToString());
            created.Add(PrefabInstantiator.instantiateEnemy(
                encounterConstants.enemyPrefab,
                enemy,
                Vector3.zero,
                parentTransform
            ));
        }
        return created;
    }
}
