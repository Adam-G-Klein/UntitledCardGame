using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Effect that globally transforms cards for the rest of combat.
*/
public class TransmogrifyCard : EffectStep
{
    [SerializeField]
    private string inputDeckInstancesKey;

    [SerializeField]
    private CardType targetCardType;

    [SerializeField]
    private CardType cardToTransformInto;

    [SerializeField]
    private bool getCardTypeFromKey = false;
    [SerializeField]
    private string inputCardKey = "";

    [SerializeField]
    private bool includeDrawPile = true;
    [SerializeField]
    private bool includeDiscardPile = true;
    [SerializeField]
    private bool includeCardsInHand = true;



    public TransmogrifyCard() {
        effectStepName = "TransmogrifyCard";
    }

    public override IEnumerator invoke(EffectDocument document) {
        List<DeckInstance> deckInstances = document.map.GetList<DeckInstance>(inputDeckInstancesKey);
        if (deckInstances.Count == 0) {
            EffectError("No valid entity with deck input for key " + inputDeckInstancesKey);
            yield return null;
        }

        CardType destinationCardType = cardToTransformInto;
        if (getCardTypeFromKey) {
            if (!document.map.ContainsValueWithKey<Card>(inputCardKey)) {
                EffectError("No valid card input but getCardTypeFromKey was set");
                yield return null;
            }
            List<Card> cardsFromMap = document.map.GetList<Card>(inputCardKey);
            if (cardsFromMap.Count == 0 || cardsFromMap.Count > 1) {
                EffectError("Expected exactly 1 card for key " + inputCardKey);
                yield break;
            }
            destinationCardType = cardsFromMap[0].cardType;
        }

        foreach (DeckInstance deck in deckInstances) {
            deck.TransformAllCardsOfType(
                targetCardType,
                destinationCardType,
                includeCardsInHand,
                includeDrawPile,
                includeDiscardPile
            );
        }

        yield return null;
    }
}