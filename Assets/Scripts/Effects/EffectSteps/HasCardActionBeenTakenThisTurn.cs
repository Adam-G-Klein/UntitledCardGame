using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    This effect step takes two ints, does the boolean comparison on them,
    and outputs true or false given the operation.
*/
public class HasCardActionBeenTakenThisTurn : EffectStep, IEffectStepCalculation
{
    [SerializeField]
    private CardAction cardAction; 
    
    [SerializeField]
    private string outputKey = "";
    
    public HasCardActionBeenTakenThisTurn() {
        effectStepName = "HasCardActionBeenTakenThisTurn";
    }

    public override IEnumerator invoke(EffectDocument document) {
        List<Card> cards = new List<Card>();
        if (cardAction == CardAction.Exhaust) {
            cards = EnemyEncounterManager.Instance.combatEncounterState.cardsExhaustThisTurn;
        }
        bool result = cards.Count != 0;
        document.boolMap[outputKey] = result;
        yield return null;
    }

    public IEnumerator invokeForCalculation(EffectDocument document)
    {
        yield return invoke(document);
    }

    private enum CardAction {
        Exhaust,
    }
}
