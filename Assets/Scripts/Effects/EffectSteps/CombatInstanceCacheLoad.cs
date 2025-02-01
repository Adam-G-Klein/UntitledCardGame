using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Effect that loads a value into the current effect document from the
    cached effect document attached to the combat instance.

    It assumes the type from which map the key is stored in.
    Only allows loading primitive values for now.
*/
public class CombatInstanceCacheLoad : EffectStep
{
    [SerializeField]
    private string inputCombatInstanceKey = "";

    [SerializeField]
    private string cacheKey = "";

    [SerializeField]
    private string currentWorkflowKey = "";

    public CombatInstanceCacheLoad() {
        effectStepName = "CombatInstanceCacheLoad";
    }

    public override IEnumerator invoke(EffectDocument document) {
        List<CombatInstance> instances = document.map.GetList<CombatInstance>(inputCombatInstanceKey);
        if (instances.Count == 0) {
            EffectError("No input targets present for key " + inputCombatInstanceKey);
            yield break;
        }
        if (instances.Count > 1) {
            EffectError("Too many input targets for key " + inputCombatInstanceKey + ", only 1 allowed instead have " + instances.Count.ToString());
            yield break;
        }
        CombatInstance target = instances[0];

        bool isBool = target.cachedEffectValues.boolMap.ContainsKey(cacheKey);
        bool isInt = target.cachedEffectValues.intMap.ContainsKey(cacheKey);
        bool isString = target.cachedEffectValues.stringMap.ContainsKey(cacheKey);
        int sum = Convert.ToInt32(isBool) + Convert.ToInt32(isInt) + Convert.ToInt32(isString);
        if (sum == 0) {
            EffectError("Cache key [" + currentWorkflowKey + "] not found in the bool, string, or int map");
            yield break;
        }
        if (sum > 1) {
            EffectError("Cache key [" + currentWorkflowKey + "] ambiguous because it exists in multiple maps, breaking!!!");
            yield break;
        }
        if (isBool) {
            document.boolMap[currentWorkflowKey] = target.cachedEffectValues.boolMap[cacheKey];
        }
        if (isInt) {
            document.intMap[currentWorkflowKey] = target.cachedEffectValues.intMap[cacheKey];
        }
        if (isString) {
            document.stringMap[currentWorkflowKey] = target.cachedEffectValues.stringMap[cacheKey];
        }
    }
}
