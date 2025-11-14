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
        int actNumber = 1;

        // Deduplicate the pools so we can choose without replacement over the whole list.
        // We do not want to select multiple of the same enemy from the same pool.
        Dictionary<EnemyEncounterPoolSO, List<EnemyEncounterTypeSO>> enemyPools = new Dictionary<EnemyEncounterPoolSO, List<EnemyEncounterTypeSO>>();
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
            EnemyEncounter EE = new EnemyEncounter(chosen);
            EE.act = GetAct(actNumber);
            EE.SetIsElite(pool.isElite);
            if (pool.isElite) {
                actNumber += 1;
            }
            encounters.Add(EE);
            if (i != enemyEncounterPools.Count - 1) {
                ShopEncounter SE = new ShopEncounter(shopData);
                SE.act = GetAct(actNumber);
                encounters.Add(SE);
            }
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