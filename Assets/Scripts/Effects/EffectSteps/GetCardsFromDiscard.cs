using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

/*
    This just currently gets all cards from the discard pile(s) of the
    inputs.
*/
public class GetCardsFromDiscard : EffectStep
{
    [SerializeField]
    private string inputKey = "";
    [SerializeField]
    private string outputKey = "";

    public GetCardsFromDiscard() {
        effectStepName = "GetCardsFromDiscard";
    }

    public override IEnumerator invoke(EffectDocument document) {
        List<DeckInstance> instances = document.map.GetList<DeckInstance>(inputKey);
        if (instances.Count == 0) {
            EffectError("No valid entity with deck input");
            yield return null;
        }
        List<Card> cardList = new List<Card>();
        foreach (DeckInstance instance in instances) {
            cardList.AddRange(instance.discardPile);
        }
        document.map.AddItems<Card>(outputKey, cardList);
        yield return null;
    }
}
