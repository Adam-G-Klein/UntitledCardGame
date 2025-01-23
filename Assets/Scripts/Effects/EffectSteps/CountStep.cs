using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using UnityEngine;

public class CountStep : EffectStep, IEffectStepCalculation {
    [SerializeField]
    private string inputKey = "";
    [SerializeField]
    private string outputKey = "";

    public CountStep() {
        effectStepName = "CountStep";
    }

    public override IEnumerator invoke(EffectDocument document) {
        document.intMap[outputKey] = document.map.CountItemsWithKeyString(inputKey);
        yield return null;
    }

    public IEnumerator invokeForCalculation(EffectDocument document)
    {
        yield return invoke(document);
    }
}