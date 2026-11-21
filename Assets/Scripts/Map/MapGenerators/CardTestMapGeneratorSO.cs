using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewCardTestMapGenerator",
    menuName = "Map/Map Generator/Card Test Map Generator")]
[System.Serializable]
public class CardTestMapGeneratorSO : MapGeneratorSO {

    public EnemyEncounterTypeSO testEncounterType;
    public int numEncountersToGenerate = 500;
    [Tooltip("If set, a shop using this data is added after every test combat. Leave empty for back-to-back combats.")]
    public ShopDataSO shopData;

    public override Map generateMap() {
        List<Encounter> encounters = new();
        for (int i = 0; i < numEncountersToGenerate; i++) {
            encounters.Add(new EnemyEncounter(testEncounterType) { act = Act.One });
            if (shopData != null) {
                encounters.Add(new ShopEncounter(shopData) { act = Act.One });
            }
        }
        return new Map(encounters);
    }
}
