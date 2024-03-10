using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardModificationEffect : EffectStep {
    [SerializeField]
    private string inputKey = "";
    [SerializeField]
    private int scale = 0;
    [SerializeField]
    private CardType cardType;
    [SerializeField]
    private bool useHardCodedCardTypes;
    [SerializeField]
    private bool affectsAllCardsOfType = false;
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
        if (useHardCodedCardTypes) {
            cardType.ChangeCardModification(modification, newScale);
            yield return null;
        }
        
        if (!document.map.ContainsValueWithKey<Card>(inputKey)) {
            EffectError("No input Card for given key " + inputKey);
            yield return null;
        }

        int newScale = scale;
        if (getScaleFromKey) {
            if (!document.intMap.ContainsKey(scaleKey)) {
                EffectError("No input scale for given key " + scaleKey);
                yield return null;
            }
            newScale = document.intMap[scaleKey];
        }

        List<Card> cards = document.map.GetList<Card>(inputKey);
        foreach (Card card in cards) {
            if (affectsAllCardsOfType) {
                    card.cardType.ChangeCardModification(modification, newScale);
                }
            } else {
                card.ChangeCardModification(modification, newScale);
            }
        }

        yield return null;
    }
}