using System.Collections;
using UnityEngine;

/*
    Flashes the card that set off this workflow (a card's discard, draw, exhaust or end-of-turn trigger), so
    it's clear which card in hand just fired before its effect plays.
*/
public class CardTriggeredVFX : EffectStep
{
    [SerializeField]
    private GameObject vfxPrefab;

    public CardTriggeredVFX() {
        effectStepName = "CardTriggeredVFX";
    }

    public override IEnumerator invoke(EffectDocument document) {
        if (document.map.ContainsValueWithKey<PlayableCard>(EffectDocument.ORIGIN))
            CardVFX.Play(vfxPrefab, document.map.TryGetItem<PlayableCard>(EffectDocument.ORIGIN, 0));
        yield break;
    }
}
