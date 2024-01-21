using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using UnityEngine;

public class CountStep : EffectStep {
    [SerializeField]
    private string inputKey = "";
    [SerializeField]
    private string outputKey = "";

    public CountStep() {
        effectStepName = "CountStep";
    }

    public override IEnumerator invoke(EffectDocument document) {
        document.intMap[outputKey] = document.map.GetAllItemsWithKeyString(inputKey).Count;
        yield return null;
    }
}