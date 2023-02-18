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
        foreach(Card cardInfo in info.cards) {
            newCard = PrefabInstantiator.instantiateCard(cardPrefab, transform, cardInfo, info.entityFrom);
            cardsInHand.Add(newCard);
        }
        displayCards();
    }

    public void cardCastEventHandler(CardCastEventInfo info){
        PlayableCard cardToDiscard = null;
        foreach(PlayableCard card in cardsInHand) {
            if(card.card.id == info.cardInfo.id) {
                cardToDiscard = card;
            }
        }
        if(cardToDiscard != null) {
            discardCard(cardToDiscard);
        }
    }

    public void turnPhaseChangedEventHandler(TurnPhaseEventInfo info){
        if(info.newPhase == TurnPhase.END_PLAYER_TURN) {
            discardHand();
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

    private void discardHand(){
        List<PlayableCard> retainedCards = new List<PlayableCard>();
        foreach(PlayableCard card in cardsInHand) {
            if(card.retained) {
                retainedCards.Add(card);
                card.retained = false;
            } else {
                Destroy(card.gameObject);
                card.discardFromDeck();
            }
        }
        // do this instead of calling remove for each
        // to prevent enumeration issues in the for loop
        cardsInHand = retainedCards;
        displayCards();
    }

    // Do not call on whole hand, only call on individual cards
    // modifies the list of cards in hand 
    public void discardCard(PlayableCard card){
        cardsInHand.Remove(card);
        Destroy(card.gameObject);
        card.discardFromDeck();
        displayCards();
    }
}