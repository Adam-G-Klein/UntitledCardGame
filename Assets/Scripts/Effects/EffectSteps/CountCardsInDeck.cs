using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CountCardsInDeck : EffectStep, IEffectStepCalculation
{
    [SerializeField]
    private string inputDeckKey = "";
    [SerializeField]
    private CardFilter filter;
    [SerializeField]
    private bool includeDrawPile = true;
    [SerializeField]
    private bool includeDiscardPile = false;
    [SerializeField]
    private bool includeCardsInHand = false;
    [SerializeField]
    private string outputKey = "";


    public CountCardsInDeck() {
        effectStepName = "CountCardsInDeck";
    }

    public override IEnumerator invoke(EffectDocument document) {
        List<DeckInstance> deckInstances = document.map.GetList<DeckInstance>(inputDeckKey);
        if (deckInstances == null || deckInstances.Count == 0 || deckInstances.Count > 1) {
            EffectError("No valid entity with deck input for key " + inputDeckKey);
            yield break;
        }

        DeckInstance deckInstance = deckInstances[0];
        int total = 0;
        if (includeDrawPile) {
            total += countCards(deckInstance.drawPile);
        }
        if (includeDiscardPile) {
            total += countCards(deckInstance.discardPile);
        }
        if (includeCardsInHand) {
            total += countCards(deckInstance.inHand);
        }

        document.intMap[outputKey] = total;
        yield return null;
    }

    public IEnumerator invokeForCalculation(EffectDocument document)
    {
        yield return invoke(document);
    }

    private int countCards(List<Card> cards) {
        int count = 0;
        foreach (Card c in cards) {
            if (filter.ApplyFilter(c)) {
                count++;
            }
        }
        return count;
    }
}
