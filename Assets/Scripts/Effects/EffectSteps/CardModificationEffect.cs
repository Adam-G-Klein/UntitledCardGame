using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardModificationEffect : EffectStep {
    [SerializeField]
    private string inputKey = "";
    [SerializeField]
    private int scale = 0;
    [SerializeField]
    private bool affectsAllCardsOfType = false;
    [SerializeField]
    // CardTypeToModify is only used if the `affectsAllCardsOfType` is checked.
    private CardType cardTypeToModify;
    [SerializeField]
    private bool getScaleFromKey = false;
    [SerializeField]
    private string scaleKey = "";
    [SerializeField]
    private CardModification modification;

    public CardModificationEffect() {
        effectStepName = "CardModificationEffect";
    }

    public override IEnumerator invoke(EffectDocument document) {
        int newScale = scale;
        if (getScaleFromKey) {
            if (!document.intMap.ContainsKey(scaleKey)) {
                EffectError("No input scale for given key " + scaleKey);
                yield return null;
            }
            newScale = document.intMap[scaleKey];
        }

        if (affectsAllCardsOfType) {
            cardTypeToModify.ChangeCardModification(modification, newScale);
            yield break;
        }
        if (!document.map.ContainsValueWithKey<Card>(inputKey)) {
            EffectError("No input Card for given key " + inputKey);
            yield return null;
        }

        List<Card> cards = document.map.GetList<Card>(inputKey);
        foreach (Card card in cards) {
            card.ChangeCardModification(modification, newScale);
        }

        yield return null;
    }
}