using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
        bool found = false;
        foreach(KeyValuePair<Tuple<string, Type>, List<object>> pair in document.map.GetDict()) {
            if (pair.Key.Item1 == keyToCheck && pair.Value.Count > 0) {
                found = true;
                break;
            }
        }
        Debug.Log("EndWorkflowIfNoMapElement, key [" + keyToCheck + "] found: " + found.ToString());
        return !found;
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