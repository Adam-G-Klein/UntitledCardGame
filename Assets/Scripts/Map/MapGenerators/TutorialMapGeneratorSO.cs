using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewTutorialMapGenerator",
    menuName = "Map/Map Generator/Tutorial Map Generator")]
[System.Serializable]
public class TutorialMapGeneratorSO : MapGeneratorSO
{
    [Header("Fully static map: enemy encounters interleaved with shops,\n" +
        "starting with an enemy. Needs exactly one more enemy than shops.\n" +
        "Acts are set explicitly per encounter so the demo-copied tutorial\n" +
        "portion can sit inside act one.")]
    public List<TutorialEnemyEncounter> enemyEncounters;
    public List<ShopEncounter> shopEncounters;

    public override Map generateMap()
    {
        List<Encounter> encounters = new List<Encounter>();
        for (int i = 0; i < enemyEncounters.Count; i++) {
            TutorialEnemyEncounter tutEE = enemyEncounters[i];
            EnemyEncounter EE = new EnemyEncounter(tutEE.enemyEncounterType);
            EE.act = tutEE.act;
            EE.SetIsElite(tutEE.isElite);
            EE.SetBonusMana(tutEE.bonusManaReward);
            EE.SetBonusTeamSize(tutEE.bonusTeamSizeReward);
            encounters.Add(EE);
            if (i != enemyEncounters.Count - 1) {
                // Copy so run state (isCompleted, static shop indices) never
                // mutates this asset's serialized shop encounters across runs
                encounters.Add(new ShopEncounter(shopEncounters[i]));
            }
        }

        return new Map(encounters);
    }

    [System.Serializable]
    public class TutorialEnemyEncounter {
        public EnemyEncounterTypeSO enemyEncounterType;
        public Act act = Act.One;
        public int bonusManaReward;
        public int bonusTeamSizeReward;
        public bool isElite;
    }
}
