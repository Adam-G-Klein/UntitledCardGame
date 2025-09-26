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
    [Header("Accepts a list of entities with decks")]
    [SerializeField]
    private string inputKey = "";
    [SerializeField]
    private bool getLimitedNumber = false;
    [SerializeField]
    private int numberOfCardsToGet = 1;

    [SerializeField]
    [Header("If checked, we shuffle the discard pile into the draw pile if the draw pile is empty\n NOTE: this will modify game state")]
    private bool shuffleIfEmpty = true;

    [SerializeField]
    private string outputKey = "";
    [Header("Gets all of the cards in draw and discard from provided entities")]
    [SerializeField]
    private bool getAllCardsFromEntities;
    [Header("If checked, we get all cards from the source deck, not the in-combat deck")]
    [SerializeField]
    private bool getCardsFromSourceDeck = false;
    [SerializeField]
    [Header("If checked, ignore the numberOfCards and return the whole draw pile of the target deck")]
    private bool getAllFromOnlyDrawPile = false;
    public GetCardsFromDeck() {
        effectStepName = "GetCardsFromDeck";
    }

    public override IEnumerator invoke(EffectDocument document) {
        // Check for valid entity with deck target(s)
        List<DeckInstance> instances = document.map.GetList<DeckInstance>(inputKey);
        if (instances.Count == 0) {
            EffectError("No valid entity with deck input");
            yield return null;
        }

        List<Card> outputCards = new List<Card>();
        foreach(DeckInstance instance in instances) {
            addInstanceCards(instance, outputCards);
        }
        foreach(Card card in outputCards) {
            Debug.Log("Got card from deck: " + card.cardType.GetName() + " id: " + card.id);
        }
        document.map.AddItems<Card>(outputKey, outputCards);
        yield return null;
    }

    private void addInstanceCards(
            DeckInstance instance,
            List<Card> outputCards) {
            if (getCardsFromSourceDeck) {
                outputCards.AddRange(instance.sourceDeck.cards);
            } else if (getAllFromOnlyDrawPile) {
                outputCards.AddRange(instance.drawPile);
            } else {
                int num;
                if (getLimitedNumber) {
                    num = numberOfCardsToGet;
                    // Need to see if the number to get exceeds all possible cards to retrieve
                    num = Mathf.Min(num, instance.drawPile.Count + instance.discardPile.Count);
                    if (num > instance.drawPile.Count && shuffleIfEmpty) {
                        instance.ShuffleDiscardIntoDraw();
                    }
                } else {
                    num = instance.drawPile.Count;
                    if (num == 0 && shuffleIfEmpty) {
                        // If we need to retrieve cards from a deck's draw pile and it's empty, then we need to shuffle
                        instance.ShuffleDiscardIntoDraw();
                        num = instance.drawPile.Count;
                    }
                }
                getCardsFromDraw(instance, num, outputCards);
            }
    }

    private void getCardsFromDraw(
            DeckInstance deckInstance,
            int num,
            List<Card> cardList) {
        if (num < 0) {
            EffectError("Can't get a negative number of cards from a deck");
            return;
        }

        if (deckInstance.drawPile.Count <= num) {
            cardList.AddRange(deckInstance.drawPile);
            return;
        }

        cardList.AddRange(deckInstance.drawPile.GetRange(0, num));
    }
}