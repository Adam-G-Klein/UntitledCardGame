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
        List<CombatEntityInstance> entities = document.getCombatEntityInstances(inputKey);
        if (entities.Count == 0 || entities.Count > 1) {
            EffectError("Either no valid target or too many valid targets" + 
                " to get percent of max health from under key " + inputKey);
            yield return null;
        }

        int output = Convert.ToInt32(entities[0].baseStats.getMaxHealth() * percent);
        document.intMap[outputKey] = output;
        yield return null;
    }
}