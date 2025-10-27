using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    This effect step takes two ints, does the boolean comparison on them,
    and outputs true or false given the operation.
*/
public class CheckTypeOfMostRecentlyPlayedCard : EffectStep, IEffectStepCalculation
{
    [SerializeField]
    private CardCategory cardCategory;

    [SerializeField]
    private string outputKey = "";

    public CheckTypeOfMostRecentlyPlayedCard() {
        effectStepName = "CheckTypeOfMostRecentlyPlayedCard";
    }

    public override IEnumerator invoke(EffectDocument document) {
        bool result = false;
        Card lastPlayed = EnemyEncounterManager.Instance.combatEncounterState.GetLastCastCard();
        if (lastPlayed != null )
        {
            result = lastPlayed.cardType.cardCategory == cardCategory;
        }
        Debug.Log("cardCategory" + cardCategory);
        Debug.Log("result" + result);
        document.boolMap[outputKey] = result;
        yield return null;
    }

    public IEnumerator invokeForCalculation(EffectDocument document)
    {
        yield return invoke(document);
    }
}