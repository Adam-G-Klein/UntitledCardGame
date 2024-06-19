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
public class CardInHandEffect : EffectStep, ITooltipProvider
{
    [SerializeField]
    private string inputKey = "";
    [SerializeField]
    private CardInHandEffectName effect;

    private static Dictionary<CardInHandEffectName, TooltipKeyword> tooltipMapping = new Dictionary<CardInHandEffectName, TooltipKeyword>() {
        {CardInHandEffectName.Discard, TooltipKeyword.Discard},
        {CardInHandEffectName.Exhaust, TooltipKeyword.Exhaust}
    };

    public CardInHandEffect() {
        effectStepName = "CardInHandEffect";
    }

    public override IEnumerator invoke(EffectDocument document) {
        if (!document.map.ContainsValueWithKey<PlayableCard>(inputKey)) {
           EffectError("No input PlayableCard for given key " + inputKey);
            yield return null;
        }

        List<PlayableCard> playableCards = document.map.GetList<PlayableCard>(inputKey);
        foreach (PlayableCard card in playableCards) {
            switch (effect) {
                case CardInHandEffectName.Discard:
                    card.DiscardCardFromHand();
                break;

                case CardInHandEffectName.Exhaust:
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

    public TooltipViewModel GetTooltip(){
        if(KeywordTooltipProvider.Instance.HasTooltip(tooltipMapping[effect])){
            return KeywordTooltipProvider.Instance.GetTooltip(tooltipMapping[effect]);
        }
        else return new TooltipViewModel(empty: true);
    }

}