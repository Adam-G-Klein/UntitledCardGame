using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/*
    Effect that stores a value into the cache on the combat instance from the
    current effect document.

    It assumes the type from which map the key is stored in.
    Only allows storing primitive values for now.
*/
public class CombatInstanceCacheStore : EffectStep
{
    [SerializeField]
    private string inputCombatInstanceKey = "";


    [SerializeField]
    private bool hardCodedBool = false;

    [SerializeField]
    private bool useHardCodedBool = false;

    [SerializeField]
    private int hardCodedInt = 0;

    [SerializeField]
    private bool useHardCodedInt = false;

    [SerializeField]
    private string currentWorkflowKey = "";

    [SerializeField]
    private string cacheKey = "";

    public CombatInstanceCacheStore() {
        effectStepName = "CombatInstanceCacheStore";
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

        // Start with the hard-coded values, and if those are not specified, fallback to the current workflow key.
        int hardCodeSum = Convert.ToInt32(useHardCodedBool) + Convert.ToInt32(useHardCodedInt);
        if (hardCodeSum == 0) {
            // Now we attempt to use the current workflow key.
            bool isBool = document.boolMap.ContainsKey(currentWorkflowKey);
            bool isInt = document.intMap.ContainsKey(currentWorkflowKey);

            int sum = Convert.ToInt32(isBool) + Convert.ToInt32(isInt);
            if (sum == 0) {
                EffectError("Current workflow key [" + currentWorkflowKey + "] not found in the bool or int map");
                yield break;
            }
            if (sum > 1) {
                EffectError("Current workflow key [" + currentWorkflowKey + "] ambiguous because it exists in multiple maps, breaking!!!");
                yield break;
            }
            if (isBool) {
                target.cachedEffectValues.boolMap[cacheKey] = document.boolMap[currentWorkflowKey];
            }
            if (isInt) {
                target.cachedEffectValues.intMap[cacheKey] = document.intMap[currentWorkflowKey];
            }
            yield break;
        }

        if (useHardCodedBool) {
            target.cachedEffectValues.boolMap[cacheKey] = hardCodedBool;
        }
        if (useHardCodedInt) {
            target.cachedEffectValues.intMap[cacheKey] = hardCodedInt;
        }
    }
}
