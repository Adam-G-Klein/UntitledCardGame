using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Effect that adds card(s) to a deck

    Input: One or more entities with a deck (companion or minion)
    Output: NA
    Parameters:
        - CardTypes: List of card types to add to deck if GetCardsFromKey is unchecked
        - GetCardsFromKey: If checked, uses cards from input docoument to add to a deck(s)
        - InputCardsKey: The key to get the card(s) from when GetCardsFromKey is checked
        - Scale: The fixed scale if GetScaleFromKey is not enabled
        - GetScaleFromKey: If checked, the scale will be pulled from a previous step
        - InputScaleKey: The key from which to pull the scale integer from
*/
public class AddCardsToDeck : EffectStep, ITooltipProvider {
    [SerializeField]
    private string inputKey = "";
    [SerializeField]
    private List<CardType> cardTypes;
    [SerializeField]
    private AddMethod addToDeckMethod = AddMethod.ShuffleIn;
    [SerializeField]
    private bool getCardsFromKey = false;
    [SerializeField]
    private string inputCardsKey = "";
    [SerializeField]
    private int scale = 1;
    [SerializeField]
    private bool getScaleFromKey = false;
    [SerializeField]
    private string inputScaleKey = "";

    public AddCardsToDeck() {
        effectStepName = "AddCardsToDeck";
    }

    public override IEnumerator invoke(EffectDocument document) {
        // Check for valid entity with deck target(s)
        List<DeckInstance> deckInstances = document.map.GetList<DeckInstance>(inputKey);
        if (deckInstances.Count == 0) {
            EffectError("No valid inputs for adding card to deck");
            yield return null;
        }

        // Setup list of card types to add to the target(s)
        List<CardType> cardTypesToAdd = new List<CardType>();
        if (getCardsFromKey) {
            if (!document.map.ContainsValueWithKey<Card>(inputCardsKey)) {
                EffectError("No valid card input but GetCardsFromKey was set");
                yield return null;
            }
            List<Card> cardsFromMap = document.map.GetList<Card>(inputCardsKey);
            foreach (Card card in cardsFromMap) {
                cardTypesToAdd.Add(card.cardType);
            }
        } else {
            cardTypesToAdd.AddRange(cardTypes);
        }

        // Setup the scale
        int finalScale = scale;
        if (getScaleFromKey && document.intMap.ContainsKey(inputScaleKey)) {
            finalScale = document.intMap[inputScaleKey];
        }

        // Add the cards
        addCardsToEntities(deckInstances, cardTypesToAdd, finalScale);

        yield return null;
    }

    private void addCardsToEntities(
            List<DeckInstance> deckInstances,
            List<CardType> cardTypes,
            int scale) {
        foreach (DeckInstance deckInstance in deckInstances) {
            CompanionTypeSO companionType = deckInstance.GetCompanionTypeSO();
            if(companionType != null) {
                addCardsToEntity(deckInstance, cardTypes, scale, companionType);
            } else {
                Debug.LogError("DeckInstance does not have a companion type, cannot create new cards to add to it. won't know how to display their frames and icons without the companion reference");
            }
        }
    }

    private void addCardsToEntity(DeckInstance deckInstance, List<CardType> cardTypes, int scale, CompanionTypeSO companionType) {
        for (int i = 0; i < scale; i++) {
            List<Card> cards = new List<Card>();
            foreach (CardType cardType in cardTypes) {
                Card newCard = new Card(cardType, companionType);
                newCard.generated = true;
                cards.Add(newCard);
            }

            switch (addToDeckMethod) {
                case AddMethod.ShuffleIn:
                    deckInstance.ShuffleIntoDraw(cards);
                    break;
                case AddMethod.AddToTop:
                    deckInstance.AddCardsToTopOfDeck(cards);
                    break;
                case AddMethod.AddToBottom:
                    deckInstance.AddCardsToBottomOfDeck(cards);
                    break;
                case AddMethod.PermanentlyAdd:
                    deckInstance.sourceDeck.cards.AddRange(cards);
                    break;
                case AddMethod.AddToDiscard:
                    deckInstance.AddCardsToDiscard(cards);
                    break;
            }
        }
    }


    public TooltipViewModel GetTooltip() {
        if (cardTypes.Count == 0) {
            return new TooltipViewModel(empty: true);
        }
        List<TooltipLine> lines = new();
        foreach (CardType cardType in cardTypes) {
            TooltipLine l = new TooltipLine(cardType.Name, cardType.Cost + ". " + cardType.GetDescription());
            lines.Add(l);
        }
        return new TooltipViewModel(lines);
    }

    private enum AddMethod {
        AddToTop,
        ShuffleIn,
        AddToBottom,
        PermanentlyAdd,
        AddToDiscard,
    }
}