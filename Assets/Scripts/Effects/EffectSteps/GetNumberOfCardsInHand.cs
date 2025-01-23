using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Effect that gets and stores the number of cards in the player hand

    Input: NA
    Output: Integer number of cards in the player's hand
    Parameters: NA
*/
public class GetNumberOfCardsInHand : EffectStep, IEffectStepCalculation {
    [SerializeField]
    private string outputKey = "";

    public GetNumberOfCardsInHand() {
        effectStepName = "GetNumberOfCardsInHand";
    }

    public override IEnumerator invoke(EffectDocument document) {
        document.intMap[outputKey] = PlayerHand.Instance.cardsInHand.Count;
        yield return null;
    }

    public IEnumerator invokeForCalculation(EffectDocument document)
    {
        yield return invoke(document);
    }
}