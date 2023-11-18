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
public class GetPercentOfMaxHP : EffectStep {
    [SerializeField]
    private string inputKey = "";
    [SerializeField]
    private double percent = 0.5;
    [SerializeField]
    private string outputKey = "";

    public GetPercentOfMaxHP() {
        effectStepName = "GetPercentOfMaxHP";
    }

    public override IEnumerator invoke(EffectDocument document) {
        // Check for valid entity target
        List<CombatInstance> instances = document.GetCombatInstances(inputKey);
        if (instances.Count == 0 || instances.Count > 1) {
            EffectError("Either no valid target or too many valid targets" + 
                " to get percent of max health from under key " + inputKey);
            yield return null;
        }

        int output = Convert.ToInt32(instances[0].combatStats.maxHealth * percent);
        document.intMap[outputKey] = output;
        yield return null;
    }
}