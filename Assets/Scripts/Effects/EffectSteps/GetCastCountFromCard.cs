using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

/*
    Effect that gets the amount of times that a provided card has been cast this encounter

    Inputs: The card
    Output: The amount of times the card has been cast this encounter
    Paramters:
        - countFromThisCard: if checked, will get the cast count of this card
*/
public class GetCastCountFromCard : EffectStep, IEffectStepCalculation {
    [SerializeField]
    private string inputKey = "";
    [SerializeField]
    private string outputKey = "";


    [SerializeField]
    private bool countFromThisCard = true;
    public GetCastCountFromCard() {
        effectStepName = "GetCastCountFromCard";
    }

    public override IEnumerator invoke(EffectDocument document) {
        Card card;
        if (countFromThisCard) {
            PlayableCard originCard = document.map.GetItem<PlayableCard>(EffectDocument.ORIGIN, 0);
            if(originCard == null) {
                Debug.LogError("Can't get cast count from a non-card origin!");
            }
            card = originCard.card;
        } else {
            card = document.map.GetItem<Card>(inputKey, 0);
            if (card == null) {
                Debug.LogError("No card by id " + inputKey + " in effect document");
            }
        }
        document.intMap[outputKey] = card.castCount;
        yield return null;


    }

    public IEnumerator invokeForCalculation(EffectDocument document)
    {
        yield return invoke(document);
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