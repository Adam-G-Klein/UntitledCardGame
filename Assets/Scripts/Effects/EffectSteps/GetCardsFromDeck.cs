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

    public GetCardsFromDeck() {
        effectStepName = "GetCardsFromDeck";
    }

    public override IEnumerator invoke(EffectDocument document) {
        // Check for valid entity with deck target(s)
        List<CombatEntityWithDeckInstance> entities = 
            document.getCombatEntitiesWithDeckInstance(inputKey);
        if (entities.Count == 0 || entities.Count > 1) {
            EffectError("No valid entity with deck input");
            yield return null;
        }

        List<Card> outputCards = new List<Card>();
        int num;
        if (getLimitedNumber) {
            num = numberOfCardsToGet;
        } else {
            num = entities[0].inCombatDeck.drawPile.Count;
        }
        getCardsFromInCombatDeck(entities[0].inCombatDeck, num, outputCards);
        document.cardMap.addItems(outputKey, outputCards);
        yield return null;
    }

    private void getCardsFromInCombatDeck(InCombatDeck deck, int num, List<Card> cardList) {
        if (num == 0) {
            EffectError("Can't get 0 cards from a deck");
            return;
        }

        if (deck.drawPile.Count <= num) {
            cardList.AddRange(deck.drawPile);
            return;
        }

        cardList.AddRange(deck.drawPile.GetRange(0, num));
    }
}