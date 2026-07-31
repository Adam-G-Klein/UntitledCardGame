using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    This effect step takes two ints, does the boolean comparison on them,
    and outputs true or false given the operation.
*/
public class CheckCurrentTurnPhase : EffectStep
{
    [SerializeField]
    private TurnPhase turnPhase;

    [SerializeField]
    private string outputKey = "";

    public CheckCurrentTurnPhase() {
        effectStepName = "CheckCurrentTurnPhase";
    }

    public override IEnumerator invoke(EffectDocument document) {
        TurnPhase currentTurnPhase = TurnManager.Instance.GetTurnPhase();
        document.boolMap[outputKey] = currentTurnPhase == turnPhase;
        yield return null;
    }
}
