using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


[ExecuteInEditMode]
[RequireComponent(typeof(CardCastEventListener))]
[RequireComponent(typeof(CardDealtEventListener))]
[RequireComponent(typeof(TurnPhaseEventListener))]
public class PlayerHand : MonoBehaviour
{
    public List<PlayableCard> cardsInHand;

    [SerializeField]
    private GameObject cardPrefab;

    [Header("Values For PrototypeUI Card Placement")]
    [SerializeField]
    private int cardYCoord = -200;
    [SerializeField]
    private int startXCoord = -700;
    [SerializeField]
    private int cardSpacing = 20;

    public void cardDealtEventHandler(CardsDealtEventInfo info){
        PlayableCard newCard;
        foreach(CardInfo cardInfo in info.cards) {
            newCard = PrefabInstantiator.instantiateCard(cardPrefab, transform, cardInfo, info.fromStats, info.fromDeck);
            cardsInHand.Add(newCard);
        }
        displayCards();
    }

    public void cardCastEventHandler(CardCastEventInfo info){
        PlayableCard cardToDiscard = null;
        foreach(PlayableCard card in cardsInHand) {
            if(card.cardInfo.id == info.cardInfo.id) {
                cardToDiscard = card;
            }
        }
        if(cardToDiscard != null) {
            cardsInHand.Remove(cardToDiscard);
            Destroy(cardToDiscard.gameObject);
            cardToDiscard.discard();
        }
        displayCards();
    }

    public void turnPhaseChangedEventHandler(TurnPhaseEventInfo info){
        if(info.newPhase == TurnPhase.END_PLAYER_TURN) {
            foreach(PlayableCard card in cardsInHand) {
                card.discard();
            }
            cardsInHand.Clear();
        }
    }

    private void displayCards(){
        float xLoc = startXCoord;
        PlayableCard card;
        for(int i = 0; i < cardsInHand.Count; i++) {
            card = cardsInHand[i];
            card.transform.localPosition = new Vector2(
                xLoc, 
                card.hovered ? cardYCoord + card.hoverYDiff : cardYCoord);
            xLoc += cardSpacing;
        }
    }
}