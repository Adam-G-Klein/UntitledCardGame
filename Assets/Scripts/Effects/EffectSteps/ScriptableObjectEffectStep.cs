using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptableObjectEffectStep : EffectStep {
    public EffectStepSO effectStepSO;

    public ScriptableObjectEffectStep() {
        effectStepName = "ScriptableObjectEffectStep";
    }

    public override IEnumerator invoke(EffectDocument document) {
        yield return effectStepSO.effectStep.invoke(document);
    }
}