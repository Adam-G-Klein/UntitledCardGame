using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

/*
    Sets the target card to use a specific effect workflow. Used to advance the chapters of all sagas

    Inputs: The card
    Output: NA
    Paramters:
        - Increment: if checked, will increase the workflowIndex by 1, capping at the amount of workflows present
        - modifyThisCard: if checked, effect will target the card that cast this
*/
public class SetCardEffectWorkflow : EffectStep {
    [SerializeField]
    private string cardInputKey = "";
    [SerializeField]
    private string chapterInputKey = "";

    [SerializeField]
    private bool increment = true;
    [SerializeField]
    private bool modifyThisCard = true;
    public SetCardEffectWorkflow() {
        effectStepName = "SetCardEffectWorkflow";
    }

    public override IEnumerator invoke(EffectDocument document) {
        Card card;
        if (modifyThisCard) {
            PlayableCard originCard = document.map.GetItem<PlayableCard>(EffectDocument.ORIGIN, 0);
            if(originCard == null) {
                Debug.LogError("Can't get cast count from a non-card origin!");
            }
            card = originCard.card;
        } else {
            card = document.map.GetItem<Card>(cardInputKey, 0);
            if (card == null) {
                Debug.LogError("No card by id " + cardInputKey + " in effect document");
            }
        }
        if (increment) {
            // SetWorkflowIndex handles the possible overflow
            card.SetWorkflowIndex(card.GetWorkflowIndex() + 1);
        } else {
            int chapter = document.intMap[chapterInputKey];
            card.SetWorkflowIndex(chapter);

        }
        Debug.Log("new workflow index" + card.GetWorkflowIndex());
        yield return null;


    }

    private void getCardsFromInCombatDeck(
            DeckInstance deckInstance,
            int num,
            List<Card> cardList) {
        if (num == 0) {
            EffectError("Can't get 0 cards from a deck");
            return;
        }

        if (deckInstance.drawPile.Count <= num) {
            cardList.AddRange(deckInstance.drawPile);
            return;
        }

        cardList.AddRange(deckInstance.drawPile.GetRange(0, num));
    }
}