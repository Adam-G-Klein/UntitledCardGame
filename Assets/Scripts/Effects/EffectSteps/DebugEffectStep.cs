using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugEffectStep : EffectStep, IEffectStepCalculation {
    [SerializeField]
    private string inputCombatInstanceCacheKey;

    [SerializeField]
    private bool genericMap;
    [SerializeField]
    private bool ints;
    [SerializeField]
    private bool strings;
    [SerializeField]
    private bool bools;

    public DebugEffectStep() {
        effectStepName = "DebugEffectStep";
    }

    public override IEnumerator invoke(EffectDocument document) {
        printValues(document);

        if (inputCombatInstanceCacheKey != "") {
            List<CombatInstance> instances = document.map.GetList<CombatInstance>(inputCombatInstanceCacheKey);
            if (instances.Count == 0) {
                EffectError("No input targets present for key " + inputCombatInstanceCacheKey);
                yield break;
            }

            foreach (CombatInstance instance in instances) {
                Debug.Log("Printing out the cache map for combat instance " + instance.name);
                printValues(instance.cachedEffectValues);
            }
        }

        yield return null;
    }

    private void printValues(EffectDocument document) {
        if (genericMap)
            document.map.Print();
        if (ints)
            document.printIntMap();
        if (strings)
            document.printStringMap();
        if (bools)
            document.printBoolMap();
    }

    public IEnumerator invokeForCalculation(EffectDocument document)
    {
        yield return invoke(document);
    }
}