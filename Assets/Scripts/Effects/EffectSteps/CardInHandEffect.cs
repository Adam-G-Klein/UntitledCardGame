using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Effect that deals with various actions to take on a card in the player hand

    Input: Card(s) in the hand to target
    Output: NA
    Parameters:
        - Effect: The effect to enact on the card(s)
*/
public class CardInHandEffect : EffectStep
{
    [SerializeField]
    private string inputKey = "";
    [SerializeField]
    private CardInHandEffectName effect;

    public CardInHandEffect() {
        effectStepName = "CardInHandEffect";
    }

    public override IEnumerator invoke(EffectDocument document) {
        if (!document.playableCardMap.containsValueWithKey(inputKey)) {
           EffectError("No input PlayableCard for given key " + inputKey);
            yield return null;
        }

        List<PlayableCard> playableCards = document.playableCardMap.getList(inputKey);
        foreach (PlayableCard card in playableCards) {
            switch (effect) {
                case CardInHandEffectName.Discard:
                    card.DiscardCardFromHand();
                break;

                case CardInHandEffectName.Exhaust:
                    card.DiscardCardFromHand();
                    card.ExhaustCard();
                break;

                case CardInHandEffectName.Retain:
                    card.retained = true;
                break;
            }
        }

        yield return null;
    }

    public enum CardInHandEffectName {
        Discard,
        Exhaust,
        Retain
    }
}