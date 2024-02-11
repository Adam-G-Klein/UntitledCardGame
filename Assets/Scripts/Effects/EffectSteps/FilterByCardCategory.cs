using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Rendering.Universal.Internal;
using Unity.VisualScripting;

public class FilterByCardCategory : EffectStep {

    [SerializeField]
    private string inputKey = "";
    [SerializeField]
    private List<CardCategory> cardCategoriesToInclude;
    [SerializeField]
    private string outputKey = "";

    public override IEnumerator invoke(EffectDocument document) {
        List<Card> inputCards  = document.map.GetList<Card>(inputKey);
        if (inputCards.Count == 0) {
            EffectError("Found no cards to filter in the effect document!");
            yield break;
        }

        List<Card> outputCards = new List<Card>();
        foreach (Card card in inputCards) {
            if (cardCategoriesToInclude.Contains(card.cardType.cardCategory)) {
                outputCards.Add(card);
            }
        }

        document.map.AddItems<Card>(outputKey, outputCards);
        yield return null;
    }
}