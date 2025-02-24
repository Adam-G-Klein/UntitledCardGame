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
        List<Card> cardsCastThisCombat = EnemyEncounterManager.Instance.combatEncounterState.cardsCastThisCombat;
        List<Card> cardsCastThisTurn = EnemyEncounterManager.Instance.combatEncounterState.cardsCastThisTurn;
        bool result = false;
        Debug.Log("------------------");
        if (cardsCastThisTurn.Count == 0) {
            if (cardsCastThisCombat.Count > 0 && cardsCastThisCombat[cardsCastThisCombat.Count - 1].cardType.cardCategory == cardCategory) {
                    result = true;
            }
        } else {
            if (cardsCastThisTurn[cardsCastThisTurn.Count - 1].cardType.cardCategory == cardCategory) {
                result = true;
            }
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