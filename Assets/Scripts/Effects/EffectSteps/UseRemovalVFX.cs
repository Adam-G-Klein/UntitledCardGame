using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    For a card that's exhausted from hand and then permanently removed from its deck (Disappear): the card plays
    the removal experience (CardConsumeVFX) instead of the usual exhaust burn, so a card that's gone for good looks
    different from one that's only gone for this combat. Put it before the CardInHandEffect that exhausts the card.
    VFX only: it swaps which experience that card instance plays (the card is destroyed after its exhaust).
*/
public class UseRemovalVFX : EffectStep
{
    [SerializeField]
    private string inputKey = "target";
    [SerializeField]
    private GameObject removalVFXPrefab;

    public UseRemovalVFX() {
        effectStepName = "UseRemovalVFX";
    }

    public override IEnumerator invoke(EffectDocument document) {
        CardStepSfx.Play(this, document, CardStepSfx.Kind.Removal);   // a sound when the card is otherwise silent
        if (removalVFXPrefab != null && document.map.ContainsValueWithKey<PlayableCard>(inputKey)) {
            List<PlayableCard> cards = document.map.GetList<PlayableCard>(inputKey);
            foreach (PlayableCard card in cards)
                if (card != null) card.cardExhaustVFXPrefab = removalVFXPrefab;
        }
        yield break;
    }
}
