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
public class EndWorkflowIfNoMapElement : EffectStep {
    [SerializeField]
    private string keyToCheck = "";

    public EndWorkflowIfNoMapElement() {
        effectStepName = "EndWorkflowIfNoMapElement";
    }

    public override IEnumerator invoke(EffectDocument document) {
        foreach(KeyValuePair<Tuple<string, Type>, List<object>> pair in document.map.GetDict()) {
            if (pair.Key.Item1 == keyToCheck && pair.Value.Count > 0) {
                EffectManager.Instance.interruptEffectWorkflow = true;
            }
        }
        yield return null;
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