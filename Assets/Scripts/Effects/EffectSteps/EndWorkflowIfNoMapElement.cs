using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Effect that breaks from the current workflow if there is no element in the
    specified map.

    Input:
    Output: NA
    Parameters:
        -
*/
public class EndWorkflowIfNoMapElement : EffectStep, IEffectStepCalculation {
    [SerializeField]
    private string keyToCheck = "";

    public EndWorkflowIfNoMapElement() {
        effectStepName = "EndWorkflowIfNoMapElement";
    }

    public override IEnumerator invoke(EffectDocument document) {
        bool found = false;
        foreach(KeyValuePair<Tuple<string, Type>, List<object>> pair in document.map.GetDict()) {
            if (pair.Key.Item1 == keyToCheck && pair.Value.Count > 0) {
                found = true;
                break;
            }
        }
        if (!found) {
            EffectManager.Instance.interruptEffectWorkflow = true;
        }
        yield return null;
    }

    public IEnumerator invokeForCalculation(EffectDocument document)
    {
        yield return invoke(document);
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