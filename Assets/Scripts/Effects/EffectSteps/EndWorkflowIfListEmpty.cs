using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Effect that breaks from the current workflow if the list for the key is empty.

    Input: inputKey that maps to input value, and condition that is being
    checked against
    Output: NA, workflow is interupted if the condition is met
    Parameters:
        -
*/
public class EndWorkflowIfListEmpty : EffectStep, IEffectStepCalculation {
    [SerializeField]
    private string inputKey1 = "";

    public EndWorkflowIfListEmpty() {
        effectStepName = "EndWorkflowIfListEmpty";
    }

    public override IEnumerator invoke(EffectDocument document) {
        bool interupt = shouldInterrupt(document);
        if (interupt) {
            document.workflowInterrupted = true;
        }
        yield return null;
    }

    public IEnumerator invokeForCalculation(EffectDocument document)
    {
        bool interupt = shouldInterrupt(document);
        if (interupt) {
            document.workflowInterrupted = true;
        }
        yield return null;
    }

    private bool shouldInterrupt(EffectDocument document) {
        return document.map.CountItemsWithKeyString(inputKey1) == 0;
    }
}