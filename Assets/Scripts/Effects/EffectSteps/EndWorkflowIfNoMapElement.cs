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
    [SerializeField]
    private MapToCheck mapToCheck;

    public EndWorkflowIfNoMapElement() {
        effectStepName = "EndWorkflowIfNoMapElement";
    }

    public override IEnumerator invoke(EffectDocument document) {
        switch(mapToCheck) {
            case MapToCheck.PlayableCard:
                if (document.playableCardMap.getList(keyToCheck).Count == 0) {
                    EffectManager.Instance.interruptEffectWorkflow = true;
                }
            break;
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