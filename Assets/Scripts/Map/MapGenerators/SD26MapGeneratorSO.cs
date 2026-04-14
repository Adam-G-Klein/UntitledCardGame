using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "SD26MapGenerator",
    menuName = "Map/Map Generator/SD26 Map Generator")]
[System.Serializable]
public class SD26MapGenerator : MapGeneratorSO
{
    [Header("Start with enemy, then shop, and repeat until out of enemy encounters")]
    public List<SD26EnemyEncounter> enemyEncounters;
    public List<ShopEncounter> shopEncounters;
    public int playerStartingGold = 0;

    public override Map generateMap()
    {
        List<Encounter> encounters = new List<Encounter>();
        int actNumber = 1;
        for (int i = 0; i < enemyEncounters.Count; i++) {
            SD26EnemyEncounter sdEE = enemyEncounters[i];
            EnemyEncounter EE = new EnemyEncounter(sdEE.enemyEncounterType);
            EE.act = GetAct(actNumber);
            EE.SetIsElite(sdEE.isElite);
            EE.SetBonusMana(sdEE.bonusManaReward);
            EE.SetBonusTeamSize(sdEE.bonusTeamSizeReward);
            if (sdEE.isElite || sdEE.incActNumber) {
                actNumber += 1;
            }
            encounters.Add(EE);
            if (i != enemyEncounters.Count - 1) {
                encounters.Add(shopEncounters[i]);
            }
        }

        return new Map(encounters);
    }

    public override int getPlayerStartingGold()
    {
        return playerStartingGold;
    }

    public override ShopDataSO getShopData()
    {
        return null;
    }

    private Act GetAct(int actNum) {
        switch (actNum) {
            case 1:
                return Act.One;

            case 2:
                return Act.Two;

            case 3:
                return Act.Three;
        }

        return Act.One;
    }

    [System.Serializable]
    public class SD26EnemyEncounter {
        public EnemyEncounterTypeSO enemyEncounterType;
        public int bonusManaReward;
        public int bonusTeamSizeReward;
        public bool isElite;
        public bool incActNumber;
    }
}