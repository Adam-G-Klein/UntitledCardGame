using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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
    [SerializeField]
    private string outputMaximumOfMaxHP = "";

    public GetHPStatistics() {
        effectStepName = "GetHPStatistics";
    }

    public override IEnumerator invoke(EffectDocument document) {
        List<CombatInstance> instances = document.map.TryGetList<CombatInstance>(inputKey);
        if (instances.Count == 0) {
            EffectError("Either no valid target under key " + inputKey);
            yield break;
        }

        foreach (CombatInstance instance in instances)
        {
            Debug.Log("Combat instance rat " + instance.GetCompanionInstance().GetName());
        }

        document.intMap[outputMaxHPKey] = Convert.ToInt32(instances[0].combatStats.maxHealth);
        document.intMap[outputCurrentHPKey] = Convert.ToInt32(instances[0].combatStats.currentHealth);
        document.intMap[outputMissingHPKey] = Convert.ToInt32(instances[0].combatStats.maxHealth - instances[0].combatStats.currentHealth);

        if (instances.Count > 0)
        {
            int maxOfMaxHPs = 0;
            foreach (CombatInstance i in instances)
            {
                if (i.combatStats.maxHealth > maxOfMaxHPs)
                {
                    maxOfMaxHPs = i.combatStats.maxHealth;
                }
            }
            document.intMap[outputMaximumOfMaxHP] = maxOfMaxHPs;
        }

        yield return null;
    }

    public IEnumerator invokeForCalculation(EffectDocument document)
    {
        yield return invoke(document);
    }
}