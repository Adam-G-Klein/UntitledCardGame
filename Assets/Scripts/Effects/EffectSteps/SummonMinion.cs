using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    The effect that summons minion to fight for you

    Input: A companion to summon the minion from / around
    Output: The minion that is summoned
    Parameters:
        - MinionType: The MinionTypeSO that designates what minion gets spawned
        - Scale: The fixed scale if GetScaleFromKey is not enabled
        - GetScaleFromKey: If checked, the scale will be pulled from a previous step
        - InputScaleKey: The key from which to pull the scale integer from

*/
[System.Serializable]
public class SummonMinion : EffectStep
{
    [SerializeField]
    private MinionTypeSO minionType;
    [Tooltip(
        "Designates what companion from a previous GetTargets effect " + 
        "to add the minion to"
    )]
    [SerializeField]
    private string inputKey = "";
    [SerializeField]
    private string outputKey = "";
    [SerializeField]
    private int scale;
    [SerializeField]
    private bool getScaleFromKey = false;
    [SerializeField]
    private string inputScaleKey = "";

    public SummonMinion() {
        effectStepName = "SummonMinion";
    }

    public override IEnumerator invoke(EffectDocument document) {
        int finalScale = scale;
        if (getScaleFromKey && document.intMap.ContainsKey(inputScaleKey)) {
            finalScale = document.intMap[inputScaleKey];
        }

        List<CompanionInstance> companions = document.companionMap.getList(inputKey);
        if (companions.Count == 0) {
            EffectError("No valid input targets under key " + inputKey);
            yield return null;
        }
        List<MinionInstance> summonedMinions = new List<MinionInstance>();

        // No idea why we'd want multiple companions to get minions summoned at once
        // but it's gonna be possible because theoretically we can pass in multiple companion inputs
        foreach (CompanionInstance companion in companions) {
            for (int i = 0; i < finalScale; i++) {
                summonedMinions.Add(PrefabInstantiator.InstantiateMinion(
                    CombatEntityManager.Instance.encounterConstants.minionPrefab,
                    new Minion(minionType),
                    // Temp
                    companion.deckInstance.getNextMinionSpawnPosition()
                ));
            }
        }

        document.minionMap.addItems(outputKey, summonedMinions);

        yield return null;
    }
}
