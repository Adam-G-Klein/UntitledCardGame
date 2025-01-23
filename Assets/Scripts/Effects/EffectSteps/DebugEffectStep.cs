using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugEffectStep : EffectStep, IEffectStepCalculation {
    [SerializeField]
    private bool genericMap;
    [SerializeField]
    private bool ints;
    [SerializeField]
    private bool strings;

    public DebugEffectStep() {
        effectStepName = "DebugEffectStep";
    }

    public override IEnumerator invoke(EffectDocument document) {
        if (genericMap)
            document.map.Print();
        if (ints)
            document.printIntMap();
        if (strings)
            document.printStringMap();

        yield return null;
    }

    public IEnumerator invokeForCalculation(EffectDocument document)
    {
        yield return invoke(document);
    }
}