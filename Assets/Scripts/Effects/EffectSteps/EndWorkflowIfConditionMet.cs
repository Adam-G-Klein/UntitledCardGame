using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Effect that breaks from the current workflow if a condition is met

    Input: inputKey that maps to input value, and condition that is being
    checked against
    Output: NA, workflow is interupted if the condition is met
    Parameters:
        -
*/
public class EndWorkflowIfConditionMet : EffectStep, IEffectStepCalculation {
    [SerializeField]
    private string inputKey1 = "";

    [SerializeField]
    private bool conditionToEndOn = false;

    public EndWorkflowIfConditionMet() {
        effectStepName = "EndWorkflowIfConditionMet";
    }

    public override IEnumerator invoke(EffectDocument document) {
        bool interupt = shouldInterrupt(document);
        if (interupt) {
            document.Interrupt();
        }
        document.boolMap["highlightCard"] = !interupt;
        yield return null;
    }

    public IEnumerator invokeForCalculation(EffectDocument document)
    {
        bool interupt = shouldInterrupt(document);
        if (interupt) {
            document.Interrupt();
        }
        document.boolMap["highlightCard"] = !interupt;
        yield return null;
    }

    private bool shouldInterrupt(EffectDocument document) {
        return !document.boolMap.ContainsKey(inputKey1) || document.boolMap[inputKey1] == conditionToEndOn;
    }

    public enum MapToCheck {
        Companion,
        Minion,
        Enemy,
        Card,
        PlayableCard,
        Integer,
        String,
        Boolean
    }
}