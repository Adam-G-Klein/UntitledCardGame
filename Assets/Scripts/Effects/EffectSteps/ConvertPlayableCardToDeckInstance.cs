using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Effect that gets the underlying DeckInstance that a PlayableCard
    belongs to.
    This allows us to create effects that target a card and then do
    an effect to the underlying companion.

    Note: this only takes one PlayableCard at a time.
*/
public class ConvertPlayableCardToDeckInstance : EffectStep
{
    [SerializeField]
    private string inputKey = "";
    [SerializeField]
    private string outputKey = "";

    public ConvertPlayableCardToDeckInstance() {
        effectStepName = "ConvertPlayableCardToDeckInstance";
    }

    public override IEnumerator invoke(EffectDocument document) {
        if (!document.map.ContainsValueWithKey<PlayableCard>(inputKey)) {
            EffectError("No value under InputKey " + inputKey);
            yield return null;
        }
        List<PlayableCard> playableCards = document.map.GetList<PlayableCard>(inputKey);
        if (playableCards.Count == 0 || playableCards.Count > 1) {
            EffectError("ConvertPlayableCardToDeckInstance only takes one PlayableCard");
            yield break;
        }
        List<DeckInstance> decks = new();
        decks.Add(playableCards[0].deckFrom);
        document.map.AddItems<DeckInstance>(outputKey, decks);
        yield return null;
    }
}
