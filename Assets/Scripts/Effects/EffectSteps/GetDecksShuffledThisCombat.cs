using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetDecksShuffledThisCombat : EffectStep
{
    [SerializeField]
    private string outputKey = "";

    public GetDecksShuffledThisCombat() {
        effectStepName = "GetDecksShuffledThisCombat";
    }

    public override IEnumerator invoke(EffectDocument document) {
        document.intMap[outputKey] = EnemyEncounterManager.Instance.combatEncounterState.GetNumberOfDecksShuffled();
        yield return null;
    }
}
