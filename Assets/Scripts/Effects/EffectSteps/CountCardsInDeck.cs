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

    public CountCardsInDeck(
        string inputDeckKey,
        CardFilter cardFilter,
        string outputKey,
        bool includeCardsInHand = true,
        bool includeDiscardPile = true,
        bool includeDrawPile = true
    ) {
        this.inputDeckKey = inputDeckKey;
        this.filter = cardFilter;
        this.outputKey = outputKey;
        this.includeCardsInHand = includeCardsInHand;
        this.includeDiscardPile = includeDiscardPile;
        this.includeDrawPile = includeDrawPile;
    }

    public override IEnumerator invoke(EffectDocument document) {
        List<DeckInstance> deckInstances = document.map.GetList<DeckInstance>(inputDeckKey);
        if (deckInstances.Count == 0 || deckInstances.Count > 1) {
            EffectError("We only expect there to be 1 deck for CountCardsInDeck for input deck key " + inputDeckKey + ", instead there are " + deckInstances.Count);
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
