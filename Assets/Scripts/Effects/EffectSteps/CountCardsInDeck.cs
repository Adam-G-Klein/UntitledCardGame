using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CountCardsInDeck : EffectStep
{
    [SerializeField]
     private string inputDeckKey = "";
    [SerializeField]
    private CardCategory categoryToCount = CardCategory.None;
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
        if (deckInstances.Count == 0 || deckInstances.Count > 1) {
            EffectError("No valid entity with deck input for key " + inputDeckKey);
            yield return null;
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

    private int countCards(List<Card> cards) {
        int count = 0;
        foreach (Card c in cards) {
            if (categoryToCount == CardCategory.None || c.cardType.cardCategory == categoryToCount) {
                count++;
            }
        }
        return count;
    }
}
