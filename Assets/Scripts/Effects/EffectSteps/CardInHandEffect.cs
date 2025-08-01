using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

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
        {CardInHandEffectName.Exhaust, TooltipKeyword.Exhaust},
        {CardInHandEffectName.Retain, TooltipKeyword.Retain},
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

        // go through and disable all cards to be discarded or exhausted so player can't interact with them during the effects processing
        foreach (PlayableCard card in playableCards)
        {
            switch (effect)
            {
                case CardInHandEffectName.Discard:
                case CardInHandEffectName.Exhaust:
                    card.interactable = false;
                    break;
                default:
                    continue;
            }
        }
        foreach (PlayableCard card in playableCards)
        {
            switch (effect)
            {
                case CardInHandEffectName.Discard:
                    yield return PlayerHand.Instance.DiscardCard(card);
                break;

                case CardInHandEffectName.Exhaust:
                    yield return PlayerHand.Instance.ExhaustCard(card);
                    break;

                case CardInHandEffectName.Retain:
                    card.retained = true;
                    break;

                case CardInHandEffectName.Copy:
                    Card duplicatedCard = new Card(card.card.cardType, card.card.getCompanionFrom());
                    card.deckFrom.inHand.Add(duplicatedCard);
                    List<Card> cardsToBeDealt = new();
                    cardsToBeDealt.Add(duplicatedCard);
                    PlayerHand.Instance.DealCards(cardsToBeDealt, card.deckFrom);
                    break;
            }
        }

        yield return null;
    }

    public enum CardInHandEffectName {
        Discard,
        Exhaust,
        Retain,
        Copy
    }

    public TooltipViewModel GetTooltip(){
        if(KeywordTooltipProvider.Instance.HasTooltip(tooltipMapping[effect])){
            return KeywordTooltipProvider.Instance.GetTooltip(tooltipMapping[effect]);
        }
        else return new TooltipViewModel(empty: true);
    }

}