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
    private string inputEntityKey = "";
    [SerializeField]
    private CardInDeckEffectName effect;

    public CardInDeckEffect() {
        effectStepName = "CardInDeckEffect";
    }

    public override IEnumerator invoke(EffectDocument document) {
        // Check for valid entity with deck target(s)
        List<CombatEntityWithDeckInstance> entities = 
            document.getCombatEntitiesWithDeckInstance(inputEntityKey);
        if (entities.Count == 0 || entities.Count > 1) {
            EffectError("No valid entity with deck input for key " + inputEntityKey);
            yield return null;
        }

        if (!document.cardMap.containsValueWithKey(inputCardsKey)) {
            EffectError("No valid card inputs with key " + inputCardsKey);
            yield return null;
        }
        List<Card> cards = document.cardMap.getList(inputCardsKey);
        foreach (Card card in cards) {
            switch (effect) {
                case CardInDeckEffectName.Exhaust:
                    entities[0].inCombatDeck.removeFromDraw(card);
                break;

                case CardInDeckEffectName.Purge:
                    entities[0].deckEntity.getDeck().purgeCard(card.id);
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