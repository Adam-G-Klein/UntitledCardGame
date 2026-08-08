using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewSimpleEnemyPoolMapGenerator",
    menuName = "Map/Map Generator/Simple Enemy Pool Map Generator")]
[System.Serializable]
public class SimpleEnemyPoolMapGeneratorSO: MapGeneratorSO {

    public List<EnemyEncounterPoolSO> enemyEncounterPools;
    public int playerStartingGold = 0;
    public ShopDataSO shopData;
    public bool useDifferentShopDatas = false;
    [Header("Only used if above boolean is checked")]
    public List<ShopDataSO> shopDatas;

    public override Map generateMap() {
        int actNumber = 1;

        // Deduplicate the pools so we can choose without replacement over the whole list.
        // We do not want to select multiple of the same enemy from the same pool.
        Dictionary<EnemyEncounterPoolSO, List<EnemyEncounterTypeSO>> enemyPools = new Dictionary<EnemyEncounterPoolSO, List<EnemyEncounterTypeSO>>();
        List<EnemyEncounter> enemyEncounters = new List<EnemyEncounter>();
        for (int i = 0; i < enemyEncounterPools.Count; i++) {
            EnemyEncounterPoolSO pool = enemyEncounterPools[i];
            if (!enemyPools.ContainsKey(pool)) {
                enemyPools.Add(pool, new List<EnemyEncounterTypeSO>(pool.enemyEncounterTypes));
            }

            List<EnemyEncounterTypeSO> givenPool = enemyPools[pool];
            if (givenPool.Count == 0) {
                Debug.LogError(
                    "Tried to choose an enemy encounter from the pool " + pool.name + " but it was empty. " +
                    "This can happen because we choose enemy encounters without replacement - make sure there are enough encounters in the pool."
                );
            }
            int encounterTypeNumber = Random.Range(0, givenPool.Count);
            EnemyEncounterTypeSO chosen = givenPool[encounterTypeNumber];
            givenPool.Remove(chosen);

            // TODO: James, do not mutate the state of the enemy pool with the choose without replacement.
            EnemyEncounter EE = new EnemyEncounter(chosen)
            {
                act = GetAct(actNumber)
            };
            EE.SetIsElite(pool.isElite);
            EE.SetBonusMana(pool.bonusManaReward);
            EE.SetBonusTeamSize(pool.bonusTeamSizeReward);
            if (pool.isElite || pool.incActNumber) {
                actNumber += 1;
            }
            enemyEncounters.Add(EE);
        }

        // Insert all the shop encounters.
        List<ShopEncounter> shopEncounters = new List<ShopEncounter>();
        for (int i = 0; i < enemyEncounters.Count - 1; i++)
        {
            ShopDataSO currentShopData = shopData;
            if (useDifferentShopDatas)
            {
                currentShopData = shopDatas[i];
            }
            ShopEncounter SE = new ShopEncounter(currentShopData)
            {
                // Shop takes the act number from the next enemy encounter.
                act = enemyEncounters[i+1].act
            };

            // Find the next elite encounter in the enemy encounters.
            int nextEliteEncounterIndex = -1;
            for (int j = i + 1; j < enemyEncounters.Count; j++)
            {
                if (enemyEncounters[j].isEliteEncounter)
                {
                    nextEliteEncounterIndex = j;
                    break;
                }
            }
            // Add special enemy trash talk dialogue if the first shop of the act.
            if (i == 0 || shopEncounters[i - 1].act != SE.act)
            {
                Enemy nextElite = enemyEncounters[nextEliteEncounterIndex].enemyList.FirstOrDefault();
                if (nextElite != null && nextElite.enemyType != null && nextElite.enemyType.trashTalkDialogue != null && nextElite.enemyType.trashTalkDialogue.Count > 0)
                {
                    SE.trashTalk.AddRange(nextElite.enemyType.GetTrashTalkDialogue());
                }
            }

            shopEncounters.Add(SE);
        }

        // Zip the enemy encounters and shop encounters together.
        List<Encounter> encounters = new List<Encounter>();
        for (int i = 0; i < enemyEncounters.Count; i++)
        {
            encounters.Add(enemyEncounters[i]);
            if (i < shopEncounters.Count)
                encounters.Add(shopEncounters[i]);
        }

        return new Map(encounters);
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
}