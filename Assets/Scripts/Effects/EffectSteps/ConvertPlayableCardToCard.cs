using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Effect that converts a PlayableCard input to a Card output. This is to help
    simplify effects that use a specific type of card input / output. For example,
    the draw cards effect outputs the PlayableCards (the cards that are actually
    in the player hand) that were drawn, but if we want to pass the cards to an
    effect that only takes in Card (the in-memory card), then we can use this
    effect to take the playable cards and output a list of the cards themselves.
    This way, we don't need an effect like SelectCardsFromList to have to account
    for both PlayableCard and Card.

    Input: A list of PlayableCards (can be a list of just one item)
    Output: A list of Cards
    Parameters: NA
*/
public class ConvertPlayableCardToCard : EffectStep
{
    [SerializeField]
    private string inputKey = "";
    [SerializeField]
    private string outputKey = "";

    public ConvertPlayableCardToCard() {
        effectStepName = "ConvertPlayableCardToCard";
    }

    public override IEnumerator invoke(EffectDocument document) {
        if (!document.playableCardMap.containsValueWithKey(inputKey)) {
            Debug.LogError("ConvertPlayableCardToCard Effect: No value under InputKey " + inputKey);
            yield return null;
        }
        List<PlayableCard> playableCards = document.playableCardMap.getList(inputKey);
        List<Card> cards = new List<Card>();
        foreach (PlayableCard playableCard in playableCards) {
            cards.Add(playableCard.card);
        }
        document.cardMap.addItems(outputKey, cards);
        yield return null;
    }
}
