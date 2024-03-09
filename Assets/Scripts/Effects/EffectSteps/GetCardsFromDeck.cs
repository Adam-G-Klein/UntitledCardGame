using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Effect that gets and stores cards from a deck. If GetLimitedNumber is checked,
    only the first X cards in the draw pile will be retrieved and stored.

    Inputs: An entity with a deck. Required to only be 1 entity.
    Output: The cards retrieved from the deck
    Paramters:
        - GetLimitedNumber: If checked, will retrieve NumberOfCardsToGet from the top of the draw pile
            otherwise will just get the entire deck
        - NumberOfCardsToGet: The number of cards to retrieve and store from the draw pile
*/
public class GetCardsFromDeck : EffectStep {
    [SerializeField]
    private string inputKey = "";
    [SerializeField]
    private bool getLimitedNumber = false;
    [SerializeField]
    private int numberOfCardsToGet = 1;
    [SerializeField]
    private string outputKey = "";
    [SerializeField]
    private bool getCardsFromSourceDeck = false;

    public GetCardsFromDeck() {
        effectStepName = "GetCardsFromDeck";
    }

    public override IEnumerator invoke(EffectDocument document) {
        // Check for valid entity with deck target(s)
        List<DeckInstance> instances = document.map.GetList<DeckInstance>(inputKey);
        if (instances.Count == 0 || instances.Count > 1) {
            EffectError("No valid entity with deck input");
            yield return null;
        }

        List<Card> outputCards = new List<Card>();
        DeckInstance instance = instances[0];
        if (getCardsFromSourceDeck) {
            outputCards.AddRange(instance.sourceDeck.cards);
        } else {
            int num;
            if (getLimitedNumber) {
                num = numberOfCardsToGet;
                // Need to see if the number to get exceeds all possible cards to retrieve
                num = Mathf.Min(num, instance.drawPile.Count + instance.discardPile.Count);
                if (num > instance.drawPile.Count) {
                    instance.ShuffleDiscardIntoDraw();
                }
            } else {
                num = instance.drawPile.Count;
                if (num == 0) {
                    // If we need to retrieve cards from a deck's draw pile and it's empty, then we need to shuffle
                    instance.ShuffleDiscardIntoDraw();
                    num = instance.drawPile.Count;
                }
            }
            getCardsFromInCombatDeck(instances[0], num, outputCards);
        }
        document.map.AddItems<Card>(outputKey, outputCards);
        yield return null;
    }

    private void getCardsFromInCombatDeck(
            DeckInstance deckInstance,
            int num,
            List<Card> cardList) {
        if (num == 0) {
            EffectError("Can't get 0 cards from a deck");
            return;
        }

        if (deckInstance.drawPile.Count <= num) {
            cardList.AddRange(deckInstance.drawPile);
            return;
        }

        cardList.AddRange(deckInstance.drawPile.GetRange(0, num));
    }
}