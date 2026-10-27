using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Shimmers the cards in hand that the previous step changed (a cost discount, extra damage or block), so the
    change is visible on the cards themselves. Reads the same card list the change used.
*/
public class CardsChangedVFX : EffectStep
{
    [SerializeField]
    private string inputKey = "";
    [SerializeField]
    private GameObject vfxPrefab;

    public CardsChangedVFX() {
        effectStepName = "CardsChangedVFX";
    }

    public override IEnumerator invoke(EffectDocument document) {
        if (!document.map.ContainsValueWithKey<Card>(inputKey)) yield break;
        List<Card> cards = document.map.GetList<Card>(inputKey);
        foreach (PlayableCard pc in PlayerHand.Instance.GetCardsOrdered())
            if (pc != null && cards.Contains(pc.card)) CardVFX.Play(vfxPrefab, pc, follow: true);
        yield break;
    }
}
