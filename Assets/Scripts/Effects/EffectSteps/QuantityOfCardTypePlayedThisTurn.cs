using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    This effect step takes two ints, does the boolean comparison on them,
    and outputs true or false given the operation.
*/
public class QuantityOfCardTypePlayedThisTurn : EffectStep, IEffectStepCalculation
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
        int count = EnemyEncounterManager.Instance.combatEncounterState.GetNumCardsOfCategoryPlayedThisTurn(cardCategory);;
        if (getAll)
        {
            count = EnemyEncounterManager.Instance.combatEncounterState.cardsCastThisTurn.Count;
        }
        document.intMap[outputKey] = count;
        yield return null;
    }

    public IEnumerator invokeForCalculation(EffectDocument document)
    {
        yield return invoke(document);
    }
}
