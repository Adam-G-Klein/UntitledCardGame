using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    This effect step takes two ints, does the boolean comparison on them,
    and outputs true or false given the operation.
*/
public class QuantityOfCardTypePlayedThisTurn : EffectStep
{
    [SerializeField]
    private CardCategory cardCategory; 
    [SerializeField]
    private bool getAll = false;
    [SerializeField]
    private string outputKey = "";
    
    public QuantityOfCardTypePlayedThisTurn() {
        effectStepName = "QuantityOfCardTypePlayedThisTurn";
    }

    public override IEnumerator invoke(EffectDocument document) {
        List<Card> cards = EnemyEncounterManager.Instance.combatEncounterState.cardsCastThisTurn;
        int count = 0;
        foreach (Card card in cards) {
            if (getAll) {
                count += 1;
            } else if (card.cardType.cardCategory == cardCategory) {
                count += 1;
            }
        }
        document.intMap[outputKey] = count;
        yield return null;
    }
}
