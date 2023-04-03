using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewSimpleEnemyPoolMapGenerator", 
    menuName = "Map/Map Generator/Simple Enemy Pool Map Generator")]
[System.Serializable]
public class SimpleEnemyPoolMapGeneratorSO: MapGeneratorSO {

    public List<EnemyEncounterPoolSO> enemyEncounterPools;
    public ShopDataSO shopData;

    public override Map generateMap() {
        List<Encounter> encounters = new List<Encounter>();

        foreach (EnemyEncounterPoolSO pool in enemyEncounterPools) {
            encounters.Add(new EnemyEncounter(pool.getEnemyEncounterType()));
            encounters.Add(new ShopEncounter(shopData));
        }


        return new Map(encounters);
    }
}