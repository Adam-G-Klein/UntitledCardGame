using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Effect that gets a percent of a entity's max health

    Input: A combat entity
    Output: The integer output of getting the given percent of the entities max hp
    Parameters:
        - Percent: Percentage of max hp to get
*/
public class GetHPStatistics : EffectStep, IEffectStepCalculation {
    [SerializeField]
    private string inputKey = "";
    [SerializeField]
    private string outputMaxHPKey = "";

    [SerializeField]
    private string outputCurrentHPKey = "";
    [SerializeField]
    private string outputMissingHPKey = "";

    public GetHPStatistics() {
        effectStepName = "GetHPStatistics";
    }

    public override IEnumerator invoke(EffectDocument document) {
        // Check for valid entity target
        List<CombatInstance> instances = document.map.GetList<CombatInstance>(inputKey);
        if (instances.Count == 0 || instances.Count > 1) {
            EffectError("Either no valid target or too many valid targets" +
                " to get percent of max health from under key " + inputKey);
            yield return null;
        }

        document.intMap[outputMaxHPKey] = Convert.ToInt32(instances[0].combatStats.maxHealth);
        document.intMap[outputCurrentHPKey] = Convert.ToInt32(instances[0].combatStats.currentHealth);
        document.intMap[outputMissingHPKey] = Convert.ToInt32(instances[0].combatStats.maxHealth - instances[0].combatStats.currentHealth);
        yield return null;
    }

    public IEnumerator invokeForCalculation(EffectDocument document)
    {
        yield return invoke(document);
    }
}