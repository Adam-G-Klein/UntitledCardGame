using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewEnemyTypeMapGenerator", 
    menuName = "Map/Map Generator/Enemy Type Map Generator")]
[System.Serializable]
public class EnemyTypeMapGeneratorSO: MapGeneratorSO {

    public List<EnemyEncounterTypeSO> enemyEncounterTypes;
    public ShopDataSO shopData;

    public override Map generateMap() {
        List<Encounter> encounters = new List<Encounter>();

        foreach (EnemyEncounterTypeSO type in enemyEncounterTypes) {
            encounters.Add(new EnemyEncounter(type));
            encounters.Add(new ShopEncounter(shopData));
        }

        return new Map(encounters);
    }
}