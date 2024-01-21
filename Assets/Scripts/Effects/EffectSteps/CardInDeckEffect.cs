using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Effect that deals with various actions to take on a card in a deck

    Input: 1) Card(s) in the deck to target, 2) Entity with deck to do card effect on
    Output: NA
    Parameters:
        - Effect: The effect to enact on the card(s)
*/
public class CardInDeckEffect : EffectStep
{
    [SerializeField]
    private string inputCardsKey = "";
    [SerializeField]
    private string inputDeckKey = "";
    [SerializeField]
    private CardInDeckEffectName effect;

    public CardInDeckEffect() {
        effectStepName = "CardInDeckEffect";
    }

    public override IEnumerator invoke(EffectDocument document) {
        // Check for valid entity with deck target(s)
        List<DeckInstance> deckInstances = document.map.GetList<DeckInstance>(inputDeckKey);
        if (deckInstances.Count == 0 || deckInstances.Count > 1) {
            EffectError("No valid entity with deck input for key " + inputDeckKey);
            yield return null;
        }

        if (!document.map.ContainsValueWithKey<Card>(inputCardsKey)) {
            EffectError("No valid card inputs with key " + inputCardsKey);
            yield return null;
        }
        List<Card> cards = document.map.GetList<Card>(inputCardsKey);
        foreach (Card card in cards) {
            switch (effect) {
                case CardInDeckEffectName.Exhaust:
                    deckInstances[0].ExhaustCard(card);
                break;

                case CardInDeckEffectName.Purge:
                    deckInstances[0].sourceDeck.PurgeCard(card.id);
                break;
            }
        }

        yield return null;
    }
    
    public enum CardInDeckEffectName {
        Exhaust,
        Purge
    }
}