using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManualOnCombineInShop : EffectStep
{
    [SerializeField]
    public ManualOnCombineType selectedOnCombineWorkflow;

    [SerializeField]
    public CardType mabTargetCardType;
    [SerializeField]
    public CardType mabDestCardType;

    public ManualOnCombineInShop() {
        effectStepName = "ManualOnCombineInShop";
    }

    public override IEnumerator invoke(EffectDocument document) {
        List<GameStateVariableSO> gameStates = document.map.GetList<GameStateVariableSO>("gameState");
        if (gameStates == null || gameStates.Count != 1) {
            EffectError("Expected exactly one gameState variable under key `gameState`");
            yield return null;
        }
        GameStateVariableSO gameState = gameStates[0];

        Companion originCompanion = document.map.GetItem<Companion>(EffectDocument.ORIGIN, 0);

        switch (selectedOnCombineWorkflow) {
            case ManualOnCombineType.Mab:
                // Transform all copies of Mab's Boon into advanced cards.
                Debug.Log("trigged mab's combined ability");

                List<Companion> allCompanions = gameState.companions.allCompanions;

                foreach (Companion companion in allCompanions) {
                    companion.deck.TransformAllCardsOfType(mabTargetCardType, mabDestCardType);
                }

                break;
        }
    }

    public enum ManualOnCombineType {
        Mab,
    }
}
