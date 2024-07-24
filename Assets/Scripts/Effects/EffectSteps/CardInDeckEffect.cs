using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Effect that deals with various actions to take on a card in a deck

    Input: 1) Card(s) in the deck to target, 2) Entity with deck to do card effect on
    Output: the number of cards action was taken on
    Parameters:
        - Effect: The effect to enact on the card(s)
*/
public class CardInDeckEffect : EffectStep, ITooltipProvider
{
    [SerializeField]
    private string inputCardsKey = "";
    [SerializeField]
    private string inputDeckKey = "";
    [SerializeField]
    private CardInDeckEffectName effect;
    [SerializeField]
    private CardType cardToTransformInto;
    [SerializeField]
    private string outputKey = "";

    private static Dictionary<CardInDeckEffectName, TooltipKeyword> tooltipMapping = new Dictionary<CardInDeckEffectName, TooltipKeyword>() {
        {CardInDeckEffectName.Discard, TooltipKeyword.Discard},
        {CardInDeckEffectName.Exhaust, TooltipKeyword.Exhaust},
        {CardInDeckEffectName.Purge, TooltipKeyword.Purge}
    };

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
        int numberOfCardsTakenActionOn = 0;
        List<Card> cards = document.map.GetList<Card>(inputCardsKey);
        foreach (Card card in cards) {
            numberOfCardsTakenActionOn++;
            switch (effect) {
                case CardInDeckEffectName.Exhaust:
                    deckInstances[0].ExhaustCard(card);
                break;

                case CardInDeckEffectName.Transform:
                    Card transformedCard = new Card(cardToTransformInto, card.getCompanionFrom());
                    transformedCard.generated = true;
                    deckInstances[0].sourceDeck.cards.Add(transformedCard);
                    deckInstances[0].sourceDeck.PurgeCard(card.id);
                break;

                case CardInDeckEffectName.Purge:
                    deckInstances[0].sourceDeck.PurgeCard(card.id);
                break;

                case CardInDeckEffectName.Discard:
                    deckInstances[0].DiscardCard(card);
                break;

                case CardInDeckEffectName.AddToHand:
                    deckInstances[0].AddCardFromDeckToHand(card);
                break;
            }
        }
        document.intMap.Add(outputKey, numberOfCardsTakenActionOn);
        yield return null;
    }

    public enum CardInDeckEffectName {
        Exhaust,
        Purge,
        Discard,
        AddToHand,
        Transform,
    }
    public TooltipViewModel GetTooltip(){
        if(KeywordTooltipProvider.Instance.HasTooltip(tooltipMapping[effect])){
            return KeywordTooltipProvider.Instance.GetTooltip(tooltipMapping[effect]);
        }
        else return new TooltipViewModel(empty: true);
    }
}