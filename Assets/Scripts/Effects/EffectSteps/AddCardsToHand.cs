using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

/*
    Effect that adds card(s) to the player hand

    Input: Using EntityFromKey, an entity that is set as the card's origin entity
    Output: The cards that have been added to the player's hand
    Parameters:
        - CardTypes: List of card types to add to deck if GetCardsFromKey is unchecked
        - GetCardsFromKey: If checked, uses cards from input docoument to add to the hand
        - InputCardsKey: The key to get the card(s) from when GetCardsFromKey is checked
        - Scale: The fixed scale if GetScaleFromKey is not enabled
        - GetScaleFromKey: If checked, the scale will be pulled from a previous step
        - InputScaleKey: The key from which to pull the scale integer from
*/
public class AddCardsToHand : EffectStep, ITooltipProvider {
    [SerializeField]
    private string entityFromKey = "";
    [SerializeField]
    private List<CardType> cardTypes;
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
    [SerializeField]
    private string outputKey = "";

    public AddCardsToHand() {
        effectStepName = "AddCardsToHand";
    }

    public override IEnumerator invoke(EffectDocument document) {
        // Check for valid entity target
        List<DeckInstance> deckInstances = document.map.GetList<DeckInstance>(entityFromKey);
        if (deckInstances.Count == 0) {
            EffectError("No valid target to use as entity from for Card");
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
        document.map.AddItems(outputKey, addCardsToHand(cardTypesToAdd, finalScale, deckInstances));

        yield return null;
    }

    private List<PlayableCard> addCardsToHand(
            List<CardType> cardTypes,
            int scale,
            List<DeckInstance> entitiesFrom) {
        List<PlayableCard> cardsAdded = new List<PlayableCard>();
        foreach (DeckInstance entityFrom in entitiesFrom) {
            CompanionTypeSO companionTypeSO = entityFrom.GetCompanionTypeSO();
            if(companionTypeSO != null) {
                for (int i = 0; i < scale; i++) {
                    List<Card> cards = new List<Card>();
                    foreach (CardType cardType in cardTypes) {
                        Card newCard = new Card(cardType, companionTypeSO);
                        newCard.generated = true;
                        cards.Add(newCard);
                    }
                    // Add to both the hand and the deck.
                    entityFrom.inHand.AddRange(cards);
                    cardsAdded.AddRange(PlayerHand.Instance.DealCards(cards, entityFrom, false));
                }
            } else {
                Debug.LogError("DeckInstance does not have a companion type, cannot create new cards to add to it. won't know how to display their frames and icons without the companion reference");
            }

        }
        return cardsAdded;
    }

    public TooltipViewModel GetTooltip() {
        if (cardTypes.Count == 0) {
            return new TooltipViewModel(empty: true);
        }
        List<TooltipLine> lines = new();
        foreach (CardType cardType in cardTypes) {
            TooltipLine l = new TooltipLine(cardType.GetName(), cardType.Cost + ". " + cardType.GetDescription());
            lines.Add(l);
        }
        return new TooltipViewModel(lines);
    }

}