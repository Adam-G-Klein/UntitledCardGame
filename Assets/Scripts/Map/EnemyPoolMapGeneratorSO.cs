using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "EnemyPoolMapGenerator", 
    menuName = "Map/Map Generator/Enemy Pool Map Generator")]
[System.Serializable]
public class EnemyPoolMapGeneratorSO: MapGeneratorSO {

    public List<EnemyEncounterPool> enemyEncounterPools;
    public ShopDataSO shopData;

    public override Map generateMap() {
        List<Encounter> encounters = new List<Encounter>();

        foreach (EnemyEncounterPool pool in enemyEncounterPools) {
            encounters.Add(new EnemyEncounter(pool.pool[Random.Range(0, pool.pool.Count-1)]));
        }

        return new Map(encounters);
    }
}

[System.Serializable]
public class EnemyEncounterPool {
    public List<EnemyEncounterTypeSO> pool;
}