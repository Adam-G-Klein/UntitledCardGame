using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewCardTestMapGenerator",
    menuName = "Map/Map Generator/Card Test Map Generator")]
[System.Serializable]
public class CardTestMapGeneratorSO : MapGeneratorSO {

    public EnemyEncounterTypeSO testEncounterType;
    public int numEncountersToGenerate = 500;

    public override Map generateMap() {
        List<Encounter> encounters = new();
        for (int i = 0; i < numEncountersToGenerate; i++) {
            encounters.Add(new EnemyEncounter(testEncounterType) { act = Act.One });
        }
        return new Map(encounters);
    }
}
